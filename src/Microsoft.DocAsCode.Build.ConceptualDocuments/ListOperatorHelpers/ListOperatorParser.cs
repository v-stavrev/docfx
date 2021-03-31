using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.DocAsCode.Build.ConceptualDocuments.ListOperatorHelpers
{
    public struct ParseError
    {
        public string Message;
        public int Index;
        public int Column;
        public int Line;
    }

    public struct ParseResult
    {
        public ListOperator[] Lists;
        public ParseError[] Errors;

        public static readonly ParseResult Empty = new ParseResult { Lists = Array.Empty<ListOperator>(), Errors = Array.Empty<ParseError>() };
    }

    public static class ListOperatorParser
    {
        /*
         [!list folder="/calculated-attributes" file="*" depth=2 limit=50 style=bullet]

         */

        public struct Context
        {
            public static readonly Context Invalid = new Context("", int.MaxValue, 1, 1);

            public readonly string Input;
            public readonly int Index;
            public readonly int Line;
            public readonly int Column;
            public readonly bool Overflow;
            public readonly char Symbol;

            private Context(string input, int index, int line, int column)
            {
                Input = input;
                Index = index;
                Line = 1;
                Column = 1;
                Overflow = Index >= Input.Length;
                if (!Overflow)
                    Symbol = Input[Index];
                else
                    Symbol = '\0';
            }

            public static Context Start(string input)
            {
                return new Context(input, 0, 1, 1);
            }

            public ParseError Error(string what)
            {
                ParseError error;
                error.Column = Column;
                error.Line = Line;
                error.Index = Index;
                error.Message = what;
                return error;
            }

            public Context? Advance()
            {
                if (Index < Input.Length - 1)
                {
                    char next = Input[Index + 1];
                    if (next == '\n')
                    {
                        return new Context(Input, Index + 1, Line + 1, 1);
                    }
                    else
                    {
                        return new Context(Input, Index + 1, Line, Column + 1);
                    }
                }
                else
                {
                    return null;
                }
            }

            public override string ToString()
            {
                char? current = Index < Input.Length ? Input[Index] : null;
                return $"{Index}@({Line},{Column}), Current='{current}'";
            }
        }

        static bool ContinueParsing(this Context? ctx, ref Context pos)
        {
            if (ctx != null && !ctx.Value.Overflow)
            {
                pos = ctx.Value;
                return true;
            }
            else
            {
                return false;
            }
        }

        private static Context? AdvanceUntil(Context ctx, string needle)
        {
            int idx = ctx.Input.IndexOf(needle, ctx.Index, StringComparison.OrdinalIgnoreCase);
            if (idx != -1)
            {
                Context? next = ctx;
                for (int i = ctx.Index; i < idx + needle.Length && i < ctx.Input.Length; i++)
                {
                    next = next.Value.Advance();
                }
                return next;
            }
            else
            {
                return null;
            }
        }

        private static Context? SkipWhitespace(Context ctx)
        {
            Context? next = ctx;
            for (int i = ctx.Index; i < ctx.Input.Length; i++)
            {
                if (char.IsWhiteSpace(ctx.Input[i]))
                {
                    next = next.Value.Advance();
                }
                else
                {
                    break;
                }
            }
            return next;
        }

        private static Context? ReadUntilQuote(Context ctx, StringBuilder sink)
        {
            Context? next = ctx;

            for (int i = ctx.Index; next?.Overflow == false; i++)
            {
                if (next.Value.Symbol != '"')
                {
                    sink.Append(next.Value.Symbol);
                    next = next.Value.Advance();
                }
                else
                {
                    next = next?.Advance();
                    break;
                }
            }

            return next;
        }

        // NOTE: public so that it can be tested
        internal static Context? ReadWhileIdentifier(Context ctx, StringBuilder sink)
        {
            Context? next = ctx;

            for (int i = ctx.Index; next?.Overflow == false; i++)
            {
                // identifier is anything starting with letter, then followed
                // by digit, underscore (_) or dash(-)
                // allow starting symbol to be digit, so that we can have numbers
                if (char.IsLetterOrDigit(next.Value.Symbol) ||
                    (i > ctx.Index && 
                        (
                            (next.Value.Symbol == '_')
                            || (next.Value.Symbol == '-')
                        )
                    )
                )
                {
                    sink.Append(next.Value.Symbol);
                    next = next.Value.Advance();
                }
                else
                {
                    break;
                }
            }

            return next;
        }

        public static ParseResult Parse(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return ParseResult.Empty;
            }

            Context pos = Context.Start(input);
            Context? next = pos;
            StringBuilder key = new StringBuilder();
            StringBuilder value = new StringBuilder();
            List<ListOperator> lists = new List<ListOperator>();
            List<ParseError> errors = new List<ParseError>();

            while (next.ContinueParsing(ref pos))
            {
                ListOperator newList = new ListOperator();

                next = AdvanceUntil(pos, "[!list");
                if (!next.ContinueParsing(ref pos)) break;

                newList.MatchedExpression.StartingIndex = pos.Index - "[!list".Length;

                next = SkipWhitespace(pos);
                if (!next.ContinueParsing(ref pos))
                {
                    errors.Add(pos.Error("End of string while skipping whitespace"));
                    break;
                }

                while (pos.Symbol != ']')
                {
                    key.Clear();
                    value.Clear();

                    next = SkipWhitespace(pos);
                    if (!next.ContinueParsing(ref pos))
                    {
                        errors.Add(pos.Error("End of string while trying to skip whitespace between key-value pairs"));
                        break;
                    }

                    if (pos.Symbol == '"')
                    {
                        next = pos.Advance();
                        if (next.ContinueParsing(ref pos))
                        {
                            next = ReadUntilQuote(pos, key);
                        }
                        else
                        {
                            errors.Add(pos.Error("Error while reading quoted key"));
                            break;
                        }
                    }
                    else
                        next = ReadWhileIdentifier(pos, key);

                    if (!next.ContinueParsing(ref pos))
                    {
                        errors.Add(pos.Error("End of string while reading key"));
                        break;
                    }

                    next = AdvanceUntil(pos, "=");
                    if (!next.ContinueParsing(ref pos))
                    {
                        errors.Add(pos.Error("End of string while advancing to '='"));
                        break;
                    }

                    next = SkipWhitespace(pos);
                    if (!next.ContinueParsing(ref pos))
                    {
                        errors.Add(pos.Error("End of string while skipping whitespace after '='"));
                        break;
                    }

                    if (pos.Symbol == '"')
                    {
                        next = pos.Advance();
                        if (next.ContinueParsing(ref pos))
                        {
                            next = ReadUntilQuote(pos, value);
                        }
                        else
                        {
                            errors.Add(pos.Error("End of string while reading quoted value"));
                        }
                    }
                    else
                    {
                        next = ReadWhileIdentifier(pos, value);
                    }

                    if (next.ContinueParsing(ref pos))
                    {
                        newList.Condition(key.ToString(), value.ToString());
                    }
                } // while not end of tag

                if (pos.Symbol == ']')
                {
                    newList.MatchedExpression.EndingIndex = pos.Index;
                    newList.MatchedExpression.Length = 
                        newList.MatchedExpression.EndingIndex - newList.MatchedExpression.StartingIndex + 1;
                    newList.MatchedExpression.Expression = input.Substring(
                        newList.MatchedExpression.StartingIndex, 
                        newList.MatchedExpression.Length);
                    lists.Add(newList);
                }
            } // while [!list is found

            ParseResult result;
            result.Lists = lists.ToArray();
            result.Errors = errors.ToArray();
            return result;
        } // method
    }
}
