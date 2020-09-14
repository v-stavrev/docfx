// Copyright (c) ERP.bg. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.DocAsCode.Dfm
{
    using System;
    using System.Text.RegularExpressions;
    using Microsoft.DocAsCode.MarkdownLite;

    /// <summary>
    /// Replace <c>@@identifier</c> with the contents of <c>~/include/identifier.md</c>
    /// </summary>
    /// <see cref="DfmIncludeInlineRule"/>
    public class DfmIncludeInlineShortcutRule : IMarkdownRule
    {
        public virtual string Name => "DfmIncludeInlineShortcut";
        internal static readonly Regex _inlineIncludeRegex = new Regex(@"@@(?<identifier>\w+)", RegexOptions.Compiled | RegexOptions.IgnoreCase, TimeSpan.FromSeconds(10));
        public virtual Regex Include => _inlineIncludeRegex;

        public IMarkdownToken TryMatch(IMarkdownParser parser, IMarkdownParsingContext context)
        {
            var match = Include.Match(context.CurrentMarkdown);
            if (match.Length == 0)
            {
                return null;
            }
            var sourceInfo = context.Consume(match.Length);

            // @@file
            var identifier = match.Groups["identifier"].Value;
            var path = $"~/include/{identifier}.md";

            // 3. Apply inline rules to the included content
            return new DfmIncludeInlineShortcutToken(this, parser.Context, path, identifier, sourceInfo);
        }
    }
}
