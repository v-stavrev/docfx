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
            new Regex(@"((?<quotedKey>"".+"")|(?<key>[a-zA-Z_\-\.]+))=((?<quotedValue>"".+"")|(?<value>[a-zA-Z_\-\.]+))", RegexOptions.Compiled);
        
        public override string Name => nameof(ProcessLists);

        // after BuildConceptualDocument
        public override int BuildOrder => 1;

        public override void Postbuild(ImmutableList<FileModel> models, IHostService host)
        {
            // cache for faster lookup
            Dictionary<FrontmatterVariableAndValue, List<FileModel>> modelsPerVariableWithValue 
                = BuildModelsWithFrontmatterVariableValues(models);

            foreach (var model in models)
            {
                BuildList(modelsPerVariableWithValue, model);
            }
        }

        private static readonly List<FileModel> EmptyModelsList = new List<FileModel>();

        private static List<FileModel> ModelsWithAllVariables(
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
                return models.ToList();
            }
            else
            {
                return EmptyModelsList;
            }
        }

        private static void BuildList(
            Dictionary<FrontmatterVariableAndValue, List<FileModel>> modelsPerVariableWithValue,
            FileModel model)
        {
            var content = (Dictionary<string, object>)model.Content;
            var frontMatter = (IImmutableDictionary<string, object>)content[BuildConceptualDocument.FrontMatter];
            var convertedHtml = (string)content[BuildConceptualDocument.ConceptualKey];

            var htmlWithLists = listExpression.Replace(convertedHtml, listOperatorMatch =>
            {
                var listOperator = listOperatorMatch.Groups["content"].Value;
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
                    
                    if (relatedModels.Count > 0)
                    {
                        output.Append("\n<ul class='erp-list'>\n");
                        foreach (var related in relatedModels)
                        {
                            var relatedContent = (Dictionary<string, object>)related.Content;

                            string title = (string)relatedContent[Constants.PropertyName.Title];
                            string href = (string)related.Key;
                            if (href.StartsWith("~")) href = href.Substring(1);
                            if (href.EndsWith(".md", System.StringComparison.OrdinalIgnoreCase))
                                href = Path.ChangeExtension(href, ".html");
                            
                            output.Append("  <li class='erp-list-item'><a href=\"").Append(href).Append("\">")
                                .Append(title).Append("</a></li>\n");
                        }
                        output.Append("</ul>");
                        listIsRendered = true;
                    }
                }

                if (!listIsRendered && !string.IsNullOrEmpty(defaultText))
                {
                    output.Append("<p>").Append(defaultText).Append("</p>");
                }

                return output.ToString();
            });

            content[BuildConceptualDocument.ConceptualKey] = htmlWithLists;
        }

        private static Dictionary<FrontmatterVariableAndValue, List<FileModel>> BuildModelsWithFrontmatterVariableValues(ImmutableList<FileModel> models)
        {
            var modelsPerVariableWithValue = new Dictionary<FrontmatterVariableAndValue, List<FileModel>>();
            foreach (var model in models)
            {
                if ((model.Content is Dictionary<string, object> content
                    && content.TryGetValue(BuildConceptualDocument.FrontMatter, out var frontMatterRaw)
                    && frontMatterRaw is IImmutableDictionary<string, object> frontMatter))
                {
                    foreach (var kvp in frontMatter)
                    {
                        var variableWithValue = new FrontmatterVariableAndValue(kvp);

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
    }
}
