using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.DocAsCode.Build.Common;
using Microsoft.DocAsCode.Common;
using Microsoft.DocAsCode.DataContracts.Common;
using Microsoft.DocAsCode.Plugins;

namespace Microsoft.DocAsCode.Build.ConceptualDocuments
{
    [Export(nameof(ConceptualDocumentProcessor), typeof(IDocumentBuildStep))]
    public class ProcessLists : BaseDocumentBuildStep
    {
        // match [!list ]
        private static readonly Regex listExpression = 
            new Regex(@"\[!list (?<content>.+)\]", RegexOptions.Compiled);
        // match key=value or "key"="value", or any combinaiton of either
        private static readonly Regex keyValueExpression = 
            new Regex(@"(?:(?:""(?<quotedKey>[^""]+)"")|(?<key>[a-zA-Z_\-\.]+))=(?:(?:""(?<quotedValue>[^""]+)"")|(?<value>[a-zA-Z_\-\.]+))", RegexOptions.Compiled);

        private const string Frontmatter = "_frontmatter";

        private Dictionary<FrontmatterVariableAndValue, List<FileModel>> modelsPerVariableWithValue;

        public override string Name => nameof(ProcessLists);

        // after BuildConceptualDocument
        public override int BuildOrder => -1;

        public override IEnumerable<FileModel> Prebuild(ImmutableList<FileModel> models, IHostService host)
        {
            // DO the most stupid thing in the world
            Parallel.ForEach(models, model =>
            {
                var yamlHeader = ParseYamlHeader(model);
                ((Dictionary<string, object>)model.Content)[Frontmatter] = yamlHeader;
            });

            modelsPerVariableWithValue = BuildModelsWithFrontmatterVariableValues(models);

            return models;
        }

        private FrontmatterVariableAndValue[] ParseYamlHeader(FileModel model)
        {
            var content = (Dictionary<string, object>)model.Content;
            var markdown = (string)content[Constants.PropertyName.Conceptual];
            using var reader = new StringReader(markdown);
            HashSet<FrontmatterVariableAndValue> values = null;
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
                        return Array.Empty<FrontmatterVariableAndValue>();
                    }
                    isFirst = false;
                    values = new HashSet<FrontmatterVariableAndValue>();
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
                            var fv = new FrontmatterVariableAndValue(parts[0].Trim(), parts[1].Trim());
                            values.Add(fv);
                        }
                    }
                }
            }

            if (isFirst || !isClosed)
            {
                return Array.Empty<FrontmatterVariableAndValue>();
            }
            else
            {
                return values.ToArray();
            }
        }

        private static Dictionary<FrontmatterVariableAndValue, List<FileModel>> BuildModelsWithFrontmatterVariableValues(
            ImmutableList<FileModel> models)
        {
            var modelsPerVariableWithValue = new Dictionary<FrontmatterVariableAndValue, List<FileModel>>();
            foreach (var model in models)
            {
                if ((model.Content is Dictionary<string, object> content
                    && content.TryGetValue(Frontmatter, out var frontMatterRaw)
                    && frontMatterRaw is FrontmatterVariableAndValue[] frontMatter))
                {
                    foreach (var variableWithValue in frontMatter)
                    {
                        if (!modelsPerVariableWithValue.TryGetValue(variableWithValue, out var appropriateModels))
                        {
                            appropriateModels = new List<FileModel>();
                            modelsPerVariableWithValue.Add(variableWithValue, appropriateModels);
                        }
                        appropriateModels.Add(model);
                    }
                }
            }

            return modelsPerVariableWithValue;
        }

        private static LinkToArticle[] ModelsWithAllVariables(
           Dictionary<FrontmatterVariableAndValue, List<FileModel>> modelsPerVariableWithValue,
           List<FrontmatterVariableAndValue> requiredVariables)
        {
            HashSet<FileModel> models = new HashSet<FileModel>();
            int countSatisfied = 0;
            foreach (var variable in requiredVariables)
            {
                if (modelsPerVariableWithValue.TryGetValue(variable, out var relatedModels))
                {
                    relatedModels.ForEach(x => models.Add(x));
                    countSatisfied++;
                }
                else
                {
                    break;
                }
            }

            if (countSatisfied == requiredVariables.Count)
            {
                LinkToArticle[] sorted = new LinkToArticle[models.Count];
                int index = 0;
                foreach (var item in models)
                {
                    var relatedContent = (Dictionary<string, object>)item.Content;

                    string outputDir = EnvironmentContext.OutputDirectory;
                    string title = GetTitle(item);
                    string href = item.Key;

                    sorted[index] = new LinkToArticle(title, href);
                    index++;
                }
                Array.Sort(sorted);
                return sorted;
            }
            else
            {
                return Array.Empty<LinkToArticle>();
            }
        }

        static string GetTitle(FileModel model)
        {
            Dictionary<string, object> content = (Dictionary<string, object>)model.Content;
            FrontmatterVariableAndValue[] frontmatter = (FrontmatterVariableAndValue[])content[Frontmatter];
            string markdown = (string)content[Constants.PropertyName.Conceptual];

            // title from YAML header
            foreach (var fmv in frontmatter)
            {
                if ("title" == fmv.Key)
                {
                    return fmv.OriginalValue;
                }
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

            string titleFromMarkdown = GetTitleFromMarkdown(markdown);
            if (!string.IsNullOrEmpty(titleFromMarkdown))
                return titleFromMarkdown;

            return null;
        }

        static readonly Regex headingExpression = new Regex(@"^#{1,3}(?<title>.+)$", RegexOptions.Compiled);

        static string GetTitleFromMarkdown(string markdown)
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

        public override void Build(FileModel model, IHostService host)
        {
            var content = (Dictionary<string, object>)model.Content;
            var initialMarkdown = (string)content[BuildConceptualDocument.ConceptualKey];
            bool change = false;
            var fixedMarkdown = listExpression.Replace(initialMarkdown, listOperatorMatch =>
            {
                var listOperator = listOperatorMatch.Groups["content"].Value.Replace("&quot;", "\"");
                var output = new StringBuilder();
                var matches = keyValueExpression.Matches(listOperator);

                string defaultText = string.Empty;
                List<FrontmatterVariableAndValue> requiredVariablesAndValues = new List<FrontmatterVariableAndValue>();

                foreach (Match keyValueMatch in matches)
                {
                    var quotedKeyGroup = keyValueMatch.Groups["quotedKey"];
                    var keyGroup = keyValueMatch.Groups["key"];
                    var quotedValueGroup = keyValueMatch.Groups["quotedValue"];
                    var valueGroup = keyValueMatch.Groups["value"];

                    string key = quotedKeyGroup.Success ? quotedKeyGroup.Value : keyGroup.Value;
                    string value = quotedValueGroup.Success ? quotedValueGroup.Value : valueGroup.Value;

                    if ("default-text".Equals(key, System.StringComparison.OrdinalIgnoreCase))
                    {
                        defaultText = value;
                    }
                    else
                    {
                        requiredVariablesAndValues.Add(new FrontmatterVariableAndValue(key, value));
                    }
                }

                bool listIsRendered = false;
                if (requiredVariablesAndValues.Count > 0)
                {
                    var relatedModels = ModelsWithAllVariables(modelsPerVariableWithValue, requiredVariablesAndValues);

                    if (relatedModels.Length > 0)
                    {
                        output.Append("\n\n");
                        foreach (var related in relatedModels)
                        {
                            output
                                .Append("* [")
                                .Append(related.Title ?? related.Href)
                                .Append("](")
                                .Append(related.Href)
                                .Append(")\n");
                        }
                        output.Append("\n");
                        listIsRendered = true;
                    }
                }

                if (!listIsRendered && !string.IsNullOrEmpty(defaultText))
                {
                    output.Append("\n").Append(defaultText).Append("\n");
                }

                if (output.Length != 0)
                {
                    change = true;
                }

                return output.ToString();
            });

            if (change)
            {
                content[BuildConceptualDocument.ConceptualKey] = fixedMarkdown;
            }
        }

        struct LinkToArticle: IComparable<LinkToArticle>
        {
            public readonly string Title;
            public readonly string Href;

            public LinkToArticle(string title, string href)
            {
                Title = title;
                Href = href;
            }

            public int CompareTo(LinkToArticle other)
            {
                return string.Compare(Title ?? "", other.Title ?? "");
            }
        }
    }
}
