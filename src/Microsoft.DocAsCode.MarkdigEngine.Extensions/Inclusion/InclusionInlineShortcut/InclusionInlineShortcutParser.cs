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

            if (!ExtensionsHelper.MatchIdentifier(ref slice, out string identifier))
            {
                return false;
            }

            processor.Inline = new InclusionInlineShortcut
            {
                Identifier = identifier,
                IncludedFilePath = $"~/includes/{identifier}.md",
                Line = line,
                Column = column,
                Span = new SourceSpan(startPosition, processor.GetSourcePosition(slice.Start - 1)),
                IsClosed = true,
            };

            return true;
        }
    }
}
