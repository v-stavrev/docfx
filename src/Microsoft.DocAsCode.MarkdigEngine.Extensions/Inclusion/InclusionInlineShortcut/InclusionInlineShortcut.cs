// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.DocAsCode.MarkdigEngine.Extensions
{
    using Markdig.Syntax.Inlines;

    public class InclusionInlineShortcut : ContainerInline
    {
        public string RawFilename { get; set; }

        public string IncludedFilePath { get; set; }

        public object ResolvedFilePath { get; set; }

        public string GetRawToken() => $"@@{RawFilename}";
    }
}
