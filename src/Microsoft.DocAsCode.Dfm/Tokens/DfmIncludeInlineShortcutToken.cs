// Copyright (c) ERP.bg. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.DocAsCode.Dfm
{
    using System;

    using Microsoft.DocAsCode.MarkdownLite;

    public class DfmIncludeInlineShortcutToken : IMarkdownToken
    {
        public IMarkdownRule Rule { get; }
        public IMarkdownContext Context { get; }
        public string Src { get; }
        public string Identifier { get;  }
        public SourceInfo SourceInfo { get; }

        public DfmIncludeInlineShortcutToken(IMarkdownRule rule, IMarkdownContext context, string src, string identifier, SourceInfo sourceInfo)
        {
            Rule = rule;
            Context = context;
            Src = src;
            Identifier = identifier;
            SourceInfo = sourceInfo;
        }
    }
}
