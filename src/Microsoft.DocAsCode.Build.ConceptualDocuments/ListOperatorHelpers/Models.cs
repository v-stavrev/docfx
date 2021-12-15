using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.DocAsCode.Plugins;

namespace Microsoft.DocAsCode.Build.ConceptualDocuments.ListOperatorHelpers
{
    public struct ListOutputContext
    {
        public string DefaultText;
        public bool SomeItemsAreHidden;
        public IReadOnlyCollection<LinkToArticle> Links;
    }

    public delegate string RenderListInMarkdown(ListOutputContext list);

    public enum ListStyle
    {
        Bullet,
        Number,
        Heading
    }

    public class ModelsPerFrontmatter
    {
        private readonly Dictionary<string, Dictionary<string, List<FileModel>>>
            modelsWithVariable = new Dictionary<string, Dictionary<string, List<FileModel>>>(StringComparer.OrdinalIgnoreCase);

        public ModelsPerFrontmatter() { }

        public bool TryGetModelWithVariableEqualTo(string variableName, string variableValue, out List<FileModel> models)
        {
            models = null;

            if (modelsWithVariable.TryGetValue(variableValue, out var withValue))
            {
                if (withValue.TryGetValue(variableValue, out models))
                {
                    return true;
                }
            }

            return false;
        }

        public List<FileModel> AddNewVariableWithValue(string variableName, string variableValue)
        {
            if (!modelsWithVariable.TryGetValue(variableName, out var withValue))
            {
                withValue = new Dictionary<string, List<FileModel>>(StringComparer.OrdinalIgnoreCase);
                modelsWithVariable.Add(variableName, withValue);
            }
            if (!withValue.TryGetValue(variableValue, out var models))
            {
                models = new List<FileModel>();
                withValue.Add(variableValue, models);
            }
            return models;
        }
    }

    public class MatchedListExression
    {
        public int StartingIndex = -1;
        public int EndingIndex = -1;
        public int Length = 0;
        public string Expression = null;

        public override string ToString()
        {
            return Expression;
        }
    }

    public class ListOperator
    {
        public readonly MatchedListExression MatchedExpression = new MatchedListExression();

        public string FilePattern = "*";
        public string FolderPattern = "";
        public string ExcludePattern = "";
        public int Depth = -1;
        public int Limit = 10;
        public ListStyle Style = ListStyle.Bullet;
        public string DefaultText = "";
        public readonly Dictionary<string, string> Conditions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public void Condition(string key, string value)
        {
            switch (key?.ToLowerInvariant())
            {
                case "file":
                    FilePattern = value;
                    break;

                case "folder":
                    FolderPattern = value;
                    break;

                case "exclude":
                    ExcludePattern = value;
                    break;
            
                case "depth":
                    if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out this.Depth))
                    {
                        throw new FormatException($"Cannot parse depth as integer: '{value}'");
                    }
                    break;

                case "limit":
                    if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out this.Limit))
                    {
                        throw new FormatException($"Cannot parse limit as integer: '{value}'");
                    }
                    break;

                case "style":
                    if (!Enum.TryParse<ListStyle>(value, ignoreCase: true, out this.Style))
                    {
                        var possibleValues = string.Join(", ", Enum.GetNames(typeof(ListStyle)));
                        throw new FormatException($"Cannot parse style '{value}'. Must be one of {possibleValues}");
                    }
                    break;

                case "default-text":
                    DefaultText = value;
                    break;

                default:
                    Conditions[key] = value;
                    break;
            }
        }

        public override string ToString()
        {
            return MatchedExpression.ToString();
        }
    }

    [DebuggerDisplay("({Title})[{Href}]")]
    public struct LinkToArticle : IComparable<LinkToArticle>, IEquatable<LinkToArticle>
    {
        public readonly string Title;
        public readonly string Href;

        public LinkToArticle(string title, string href)
        {
            Title = title?.Trim();
            Href = href;
        }

        public int CompareTo(LinkToArticle other)
        {
            return string.Compare(Title ?? "", other.Title ?? "");
        }

        public bool Equals(LinkToArticle other)
        {
            return string.Equals(Title, other.Title);
        }

        public override bool Equals(object obj)
        {
            if (obj is LinkToArticle art) return this.Equals(art);
            else return object.ReferenceEquals(this, obj);
        }

        public override int GetHashCode()
        {
            return Title?.GetHashCode() ?? 0;
        }
    }
}
