// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.DocAsCode.MarkdigEngine.Extensions
{
    using Markdig.Parsers;
    using Markdig.Syntax;

    public class InclusionShorctutBlock : ContainerBlock
    {
        public string Identifier { get; set; }

        public string IncludedFilePath { get; set; }

        public object ResolvedFilePath { get; set; }

        public string GetRawToken() => $"@@{Identifier}";

        public InclusionShorctutBlock(BlockParser parser): base(parser)
        {

        }
    }
}
