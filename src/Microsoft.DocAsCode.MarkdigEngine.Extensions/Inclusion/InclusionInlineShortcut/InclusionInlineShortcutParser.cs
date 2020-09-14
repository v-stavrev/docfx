// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.DocAsCode.MarkdigEngine.Extensions
{
    using Markdig.Helpers;
    using Markdig.Parsers;
    using Markdig.Syntax;

    public class InclusionInlineShortcutParser : InlineParser
    {
        private const string StartString = "@@";

        public InclusionInlineShortcutParser()
        {
            OpeningCharacters = new[] { '@' };
        }

        public override bool Match(InlineProcessor processor, ref StringSlice slice)
        {
            var startPosition = processor.GetSourcePosition(slice.Start, out var line, out var column);

            if (!ExtensionsHelper.MatchStart(ref slice, StartString, false))
            {
                return false;
            }

            var str = StringBuilderCache.Local();

            str.Append(slice.CurrentChar);

            var c = slice.NextChar();
            while (c.IsAlphaNumeric())
            {
                str.Append(c);
                c = slice.NextChar();
            }

            if (str.Length == 0)
            {
                return false;
            }

            string symbol = str.ToString();
            if (string.IsNullOrWhiteSpace(symbol))
            {
                return false;
            }

            processor.Inline = new InclusionInlineShortcut
            {
                RawFilename = symbol,
                IncludedFilePath = $"~/includes/{symbol}.md",
                Line = line,
                Column = column,
                Span = new SourceSpan(startPosition, processor.GetSourcePosition(slice.Start - 1)),
                IsClosed = true,
            };

            return true;
        }
    }
}
