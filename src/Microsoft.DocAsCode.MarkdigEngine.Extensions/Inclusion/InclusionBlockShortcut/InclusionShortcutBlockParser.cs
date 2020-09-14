// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.DocAsCode.MarkdigEngine.Extensions
{
    using Markdig.Helpers;
    using Markdig.Parsers;

    public class InclusionShortcutBlockParser : BlockParser
    {
        private const string StartString = "@@";

        public InclusionShortcutBlockParser()
        {
            OpeningCharacters = new char[] { '@' };
        }

        public override BlockState TryOpen(BlockProcessor processor)
        {
            if (processor.IsCodeIndent)
            {
                return BlockState.None;
            }

            // @@identifier
            var column = processor.Column;
            var line = processor.Line;
            var command = line.ToString();

            if (!ExtensionsHelper.MatchStart(ref line, StartString, false))
            {
                return BlockState.None;
            }
            else
            {
                if (line.CurrentChar == '+')
                {
                    line.NextChar();
                }
            }

            string identifier = null;

            if (!ExtensionsHelper.MatchIdentifier(ref line, out identifier))
            {
                return BlockState.None;
            }

            while (line.CurrentChar.IsSpaceOrTab()) line.NextChar();
            if (line.CurrentChar != '\0')
            {
                return BlockState.None;
            }

            processor.NewBlocks.Push(new InclusionShorctutBlock(this)
            {
                Identifier = identifier,
                IncludedFilePath = $"~/includes/{identifier}.md",
                Line = processor.LineIndex,
                Column = column,
            });

            return BlockState.BreakDiscard;
        }
    }
}
