#nullable enable

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.DocAsCode.Build.Common;
using Microsoft.DocAsCode.Build.ConceptualDocuments.ListOperatorHelpers;
using Microsoft.DocAsCode.Common;
using Microsoft.DocAsCode.DataContracts.Common;
using Microsoft.DocAsCode.Plugins;

namespace Microsoft.DocAsCode.Build.ConceptualDocuments
{
    [Export(nameof(ConceptualDocumentProcessor), typeof(IDocumentBuildStep))]
    public class ProcessLists : BaseDocumentBuildStep
    {
#if false
        // match [!list ]
        private static readonly Regex listExpression = 
            new Regex(@"\[!list (?<content>.+)\]", RegexOptions.Compiled);
        // match key=value or "key"="value", or any combinaiton of either
        private static readonly Regex keyValueExpression = 
            new Regex(@"(?:(?:""(?<quotedKey>[^""]+)"")|(?<key>[a-zA-Z_\-\.]+))=(?:(?:""(?<quotedValue>[^""]+)"")|(?<value>[a-zA-Z_\-\.]+))", RegexOptions.Compiled);
#endif

        private ModelsPerFrontmatter modelsPerVariableWithValue = new ModelsPerFrontmatter();
        private ImmutableList<FileModel> allModels = ImmutableList<FileModel>.Empty;
        //private readonly VirtualFilesystem vfs = new VirtualFilesystem();

        public override string Name => nameof(ProcessLists);

        // after BuildConceptualDocument
        public override int BuildOrder => -1;

        public override IEnumerable<FileModel> Prebuild(ImmutableList<FileModel> models, IHostService host)
        {
            // DO the most stupid thing in the world
            Parallel.ForEach(models, model =>
            {
                var yamlHeader = ParseYamlHeader(model);
                model.SetFrontmatter(yamlHeader);
            });

            //vfs.BuildDirectoryHierarchy(models);

            modelsPerVariableWithValue = BuildModelsWithFrontmatterVariableValues(models);

            this.allModels = models;

            return models;
        }

        private static Dictionary<string, string> ParseYamlHeader(FileModel model)
        {
            var content = (Dictionary<string, object>)model.Content;
            var markdown = (string)content[Constants.PropertyName.Conceptual];
            using var reader = new StringReader(markdown);
            Dictionary<string, string>? values = null;
            bool isFirst = true;
            bool isClosed = false;
            string line;
            char[] separator = new char[] { ':' };
            while (!isClosed && (line = reader.ReadLine()) != null)
            {
                if (isFirst)
                {
                    if (!line.StartsWith("---"))
                    {
                        return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    }
                    isFirst = false;
                    values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                }
                else
                {
                    if (line.StartsWith("---"))
                    {
                        isClosed = true;
                    }
                    else
                    {
                        string[] parts = line.Split(separator, 2);
                        if (parts.Length == 2)
                        {
                            values![parts[0].Trim()] = parts[1].Trim();
                        }
                    }
                }
            }

            if (isFirst || !isClosed)
            {
                return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }
            else
            {
                return values!;
            }
        }

        private static ModelsPerFrontmatter BuildModelsWithFrontmatterVariableValues(
            ImmutableList<FileModel> models)
        {
            var modelsPerVariableWithValue = new ModelsPerFrontmatter();
            foreach (var model in models)
            {
                var frontMatter = model.GetFrontmatter();
                
                foreach (var (variable, value) in frontMatter)
                {
                    if (!modelsPerVariableWithValue.TryGetModelWithVariableEqualTo(variable, value, out var appropriateModels))
                    {
                        appropriateModels = modelsPerVariableWithValue.AddNewVariableWithValue(variable, value);
                    }
                    appropriateModels.Add(model);
                }
            }

            return modelsPerVariableWithValue;
        }

        public override void Build(FileModel model, IHostService host)
        {
            var context = new ListContext(model, allModels, modelsPerVariableWithValue/*, vfs*/);
            
            var content = model.GetContent();
            var initialMarkdown = (string)content[BuildConceptualDocument.ConceptualKey];

            var (fixedMarkdown, errors) = ProcessMarkdown(context, initialMarkdown);

            if (fixedMarkdown != initialMarkdown)
            {
                content[BuildConceptualDocument.ConceptualKey] = fixedMarkdown;
            }

            foreach (var error in errors)
            {
                host.LogError(error.Message, model.File, error.Line.ToString());
            }
        }

        internal static (string, ParseError[]) ProcessMarkdown(
            ListContext context,
            string initialMarkdown)
        {
            string fixedMarkdown = initialMarkdown;
            var parseResult = ListOperatorParser.Parse(initialMarkdown);

            if (parseResult.Lists.Length > 0)
            {
                StringBuilder sb = new StringBuilder();
                int lastIndex = 0;

                foreach (var list in parseResult.Lists)
                {
                    if (list.MatchedExpression.StartingIndex - lastIndex > 0)
                    {
                        string prefix = initialMarkdown.Substring(lastIndex, list.MatchedExpression.StartingIndex - lastIndex);
                        sb.Append(prefix);
                        lastIndex = list.MatchedExpression.StartingIndex;
                    }

                    string rendererd = RenderForFile(context, list);
                    sb.Append(rendererd);

                    // ending index points at closing ']', so we need to advance past it
                    lastIndex = list.MatchedExpression.EndingIndex + 1;
                }

                if (lastIndex < initialMarkdown.Length)
                {
                    string suffix = initialMarkdown.Substring(lastIndex);
                    sb.Append(suffix);
                }

                fixedMarkdown = sb.ToString();
            }

            return (fixedMarkdown, parseResult.Errors);
        }

        internal static string RenderForFile(ListContext context, ListOperator list)
        {
            ListOutputContext outputContext;
            outputContext.DefaultText = list.DefaultText;
            (outputContext.Links, outputContext.SomeItemsAreHidden) = context.FindRelatedArticles(list);

            string output = context.RenderAsMarkdown(list.Style)(outputContext);
            return output;
        }
    }

    internal static class PrivateFileModelExtensions
    {
        private const string Frontmatter = "_frontmatter";

        public static void Deconstruct(this KeyValuePair<string, string> kvp, out string key, out string value)
        {
            key = kvp.Key;
            value = kvp.Value;
        }

        public static IDictionary<string, object> GetContent(this FileModel model)
        {
            if (model != null && model.Content is IDictionary<string, object> cont)
            {
                return cont;
            }
            else
            {
                return new Dictionary<string, object>();
            }
        }

        public static void SetFrontmatter(this FileModel model, Dictionary<string, string> frontmatter)
        {
            var content = model.GetContent();
            content[Frontmatter] = frontmatter;
        }

        public static bool HasFrontmatterWithValue(this FileModel model, string variableName, string variableValue)
        {
            var content = model.GetContent();
            if (content.TryGetValue(Frontmatter, out object fmRaw)
                && fmRaw is Dictionary<string, string> fm
                && fm.TryGetValue(variableName, out var valueInFile))
            {
                return string.Equals(valueInFile, variableValue, StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                return false;
            }
        }

        public static Dictionary<string, string> GetFrontmatter(this FileModel model)
        {
            var content = model.GetContent();
            if (content.TryGetValue(Frontmatter, out object fmRaw)
                && fmRaw is Dictionary<string, string> fm)
            {
                return fm;
            }
            else
            {
                return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }
        }

        public static string? GetTitle(this FileModel model)
        {
            IDictionary<string, object> content = model.GetContent();
            Dictionary<string, string> frontmatter = model.GetFrontmatter();
            string markdown = (string)content[Constants.PropertyName.Conceptual];

            // title from YAML header
            if (frontmatter.TryGetValue("title", out var titleFromFrontmatter))
            {
                return titleFromFrontmatter;
            }

            object metadataValue;

            if (content.TryGetValue(Constants.PropertyName.TitleOverwriteH1, out metadataValue)
                && metadataValue is string titleOverwrite && !string.IsNullOrEmpty(titleOverwrite))
            {
                return titleOverwrite;
            }

            if (content.TryGetValue(Constants.PropertyName.Title, out metadataValue)
                && metadataValue is string titleFromMetadata && !string.IsNullOrEmpty(titleFromMetadata))
            {
                return titleFromMetadata;
            }

            string? titleFromMarkdown = GetTitleFromMarkdown(markdown);
            if (!string.IsNullOrEmpty(titleFromMarkdown))
                return titleFromMarkdown;

            return null;
        }

        static readonly Regex headingExpression = new Regex(@"^#{1,3}(?<title>.+)$", RegexOptions.Compiled);

        public static string? GetTitleFromMarkdown(string markdown)
        {
            using var reader = new StringReader(markdown);

            bool isFirstLine = false;
            bool insideFrontmatter = false;
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                // skip frontmatter
                if (isFirstLine)
                {
                    isFirstLine = false;
                    if (line.StartsWith("---"))
                        insideFrontmatter = true;
                }
                else if (insideFrontmatter)
                {
                    if (line.StartsWith("---"))
                        insideFrontmatter = false;
                    continue;
                }

                Match m;

                m = headingExpression.Match(line);
                if (m.Success) return m.Groups["title"].Value;
            }
            return null;
        }
    }
}
