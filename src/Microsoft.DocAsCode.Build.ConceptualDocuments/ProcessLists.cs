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
using Microsoft.DocAsCode.Common;
using Microsoft.DocAsCode.DataContracts.Common;
using Microsoft.DocAsCode.Plugins;

using ModelsPerFrontmatter = System.Collections.Generic.Dictionary<
    Microsoft.DocAsCode.Build.ConceptualDocuments.FrontmatterVariableAndValue,
    System.Collections.Generic.List<Microsoft.DocAsCode.Plugins.FileModel>>;

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

        

        private ModelsPerFrontmatter modelsPerVariableWithValue = new ModelsPerFrontmatter();
        private ImmutableList<FileModel> allModels = ImmutableList<FileModel>.Empty;
        private VirtualDirectory rootDirectory = new VirtualDirectory("~", "~");

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

            BuildDirectoryHierarchy(models);

            modelsPerVariableWithValue = BuildModelsWithFrontmatterVariableValues(models);

            this.allModels = models;

            return models;
        }

        private void BuildDirectoryHierarchy(ImmutableList<FileModel> models)
        {
            rootDirectory = new VirtualDirectory("~", "~");
            Dictionary<string, VirtualDirectory> diretories = new Dictionary<string, VirtualDirectory>();
            diretories.Add("~", rootDirectory);

            (string name, string fullName) ParseFileBaseDirectory(string key)
            {
                int idxFull = key.LastIndexOf('/');
                if (idxFull == -1) return ("~", "~");

                string fullName = key.Substring(0, idxFull);
                if (fullName == "~") return ("~", "~");

                int idxShort = fullName.LastIndexOf('/');
                if (idxShort == -1) throw new FormatException();

                string shortName = fullName.Substring(idxShort + 1);

                return (shortName, fullName);
            }

            VirtualDirectory? FindBaseDirectory(VirtualDirectory dir)
            {
                if (dir.FullPath == "~") return null;

                var (parentName, parentFullName) = ParseFileBaseDirectory(dir.FullPath);

                var parent = GetDir(parentName, parentFullName);
                return parent;
            }

            VirtualDirectory GetDir(string name, string fullPath)
            {
                if (!diretories.TryGetValue(fullPath, out var dir))
                {
                    dir = new VirtualDirectory(name, fullPath);
                    dir.Parent = FindBaseDirectory(dir);
                    if (dir.Parent != null)
                    {
                        dir.Parent.SubDirectories.Add(dir);
                    }
                    diretories.Add(fullPath, dir);
                }
                return dir;
            }

            foreach (var model in models)
            {
                var path = model.Key;
                var (name, fullPath) = ParseFileBaseDirectory(model.Key);
                var dir = GetDir(name, fullPath);

                var vf = model.GetVirtualFile();
                vf.Directory = dir;

                dir.Files.Add(vf);
            }
        }

        private static FrontmatterVariableAndValue[] ParseYamlHeader(FileModel model)
        {
            var content = (Dictionary<string, object>)model.Content;
            var markdown = (string)content[Constants.PropertyName.Conceptual];
            using var reader = new StringReader(markdown);
            HashSet<FrontmatterVariableAndValue>? values = null;
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
                            values!.Add(fv);
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

        private static ModelsPerFrontmatter BuildModelsWithFrontmatterVariableValues(
            ImmutableList<FileModel> models)
        {
            var modelsPerVariableWithValue = new ModelsPerFrontmatter();
            foreach (var model in models)
            {
                var frontMatter = model.GetFrontmatter();
                
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

            return modelsPerVariableWithValue;
        }

        private static readonly Dictionary<string, Regex> CachedRegexPatterns =
            new Dictionary<string, Regex>();

        private static bool MatchesFilePattern(string name, string pattern)
        {
            Regex regexPattern;
            lock (CachedRegexPatterns)
            {
                if (!CachedRegexPatterns.TryGetValue(pattern, out regexPattern))
                {
                    StringBuilder builder = new StringBuilder();
                    foreach (char symbol in pattern)
                    {
                        switch (symbol)
                        {
                            case '*':
                                builder.Append(".*");
                                break;

                            case '?':
                                builder.Append(".");
                                break;

                            case '.':
                            case '$':
                            case '^':
                            case '{':
                            case '[':
                            case '(':
                            case '|':
                            case ')':
                            //case '*':
                            case '+':
                            // case '?':
                            case '\\':
                                builder.Append('\\');
                                builder.Append(symbol);
                                break;

                            default:
                                builder.Append(symbol);
                                break;
                        }
                    }

                    regexPattern = new Regex(builder.ToString(), RegexOptions.Compiled);
                    CachedRegexPatterns.Add(pattern, regexPattern);
                }
            }

            bool matches = regexPattern.IsMatch(name);

            return matches;
        }

        private int CalculateDepthRelativeTo(FileModel root, FileModel needle)
        {
            var vfRoot = root.GetVirtualFile();
            var vfNeedle = root.GetVirtualFile();

            int depth = 0;
            VirtualDirectory? needleDir = vfNeedle.Directory;
            while (vfRoot.Directory != needleDir)
            {
                if (needleDir == null || needleDir == rootDirectory)
                {
                    depth = int.MaxValue;
                    break;
                }

                needleDir = needleDir.Parent;
                depth++;
            }

            return depth;
        }

        private LinkToArticle[] FindRelatedArticles(ListContext list)
        {
            IEnumerable<FileModel> src = list.AllFiles;

            // directory pattern
            if (!string.IsNullOrEmpty(list.FolderPattern))
            {
                string pattern = list.FolderPattern;
                if (pattern == ".")
                    pattern = (list.File.GetVirtualDirectory()?.FullPath ?? "") + "*";
                src = src
                    .Where(x => MatchesFilePattern(x.GetVirtualDirectory()?.FullPath ?? "", pattern))
                    .ToArray();
            }

            // exclude pattern
            if (string.IsNullOrEmpty(list.ExcludePattern))
            {
                // exclude self
                src = src.Where(x => x != list.File).ToArray();
            }
            else
            {
                src = src
                    .Where(x => 
                        x != list.File 
                        && !MatchesFilePattern(x.GetVirtualFile().Name, list.ExcludePattern)
                    ).ToArray();
            }

            // directory depth
            if (list.Depth > 0)
            {
                src = src
                    .Where(x => CalculateDepthRelativeTo(list.File, x) <= list.Depth)
                    .ToArray();
            }

            // file pattern
            if (!string.IsNullOrEmpty(list.FilePattern) && list.FilePattern != "*")
            {
                src = src
                    .Where(x => MatchesFilePattern(x.GetVirtualFile().Name, list.FilePattern))
                    .ToArray();
            }

            if (list.Conditions.Count > 0)
            {
                src = src
                    .Where(x => list.Conditions.All(cond => x.GetFrontmatter().Contains(cond)))
                    .ToArray();
            }

            if (list.Limit > 0)
            {
                src = src.Take(list.Limit).ToArray();
            }

            FileModel[] models = (src as FileModel[]) ?? src.ToArray();

            IEnumerable<LinkToArticle> resultBuilder = models
                .Select(item =>
                {
                    var relatedContent = (Dictionary<string, object>)item.Content;

                    string outputDir = EnvironmentContext.OutputDirectory;
                    string title = GetTitle(item) ?? string.Empty;
                    string href = item.Key;

                    return new LinkToArticle(title, href);
                })
                .OrderBy(x => x.Title);
            LinkToArticle[] sorted = resultBuilder.ToArray();
            return sorted;
        }

        static string? GetTitle(FileModel model)
        {
            IDictionary<string, object> content = model.GetContent();
            FrontmatterVariableAndValue[] frontmatter = model.GetFrontmatter();
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

            string? titleFromMarkdown = GetTitleFromMarkdown(markdown);
            if (!string.IsNullOrEmpty(titleFromMarkdown))
                return titleFromMarkdown;

            return null;
        }

        static readonly Regex headingExpression = new Regex(@"^#{1,3}(?<title>.+)$", RegexOptions.Compiled);

        static string? GetTitleFromMarkdown(string markdown)
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
            var content = model.GetContent();
            var initialMarkdown = (string)content[BuildConceptualDocument.ConceptualKey];
            bool change = false;
            var fixedMarkdown = listExpression.Replace(initialMarkdown, listOperatorMatch =>
            {
                var listOperator = listOperatorMatch.Groups["content"].Value.Replace("&quot;", "\"");
                
                var matches = keyValueExpression.Matches(listOperator);

                var builder = new ListContextBuilder();
                
                builder.File(model);
                builder.AllFiles(allModels);
                builder.ModelsPerVariable(modelsPerVariableWithValue);

                foreach (Match keyValueMatch in matches)
                {
                    var quotedKeyGroup = keyValueMatch.Groups["quotedKey"];
                    var keyGroup = keyValueMatch.Groups["key"];
                    var quotedValueGroup = keyValueMatch.Groups["quotedValue"];
                    var valueGroup = keyValueMatch.Groups["value"];

                    string key = quotedKeyGroup.Success ? quotedKeyGroup.Value : keyGroup.Value;
                    string value = quotedValueGroup.Success ? quotedValueGroup.Value : valueGroup.Value;

                    builder.Condition(key, value);
                }

                var list = builder.Build();
                
                ListOutputContext outputContext;
                outputContext.DefaultText = list.DefaultText;
                outputContext.Links = FindRelatedArticles(list);

                string output = list.RenderAsMarkdown(outputContext);

                if (output.Length != 0)
                {
                    change = true;
                }

                return output;
            });

            if (change)
            {
                content[BuildConceptualDocument.ConceptualKey] = fixedMarkdown;
            }
        }

        private static string RenderListOrdered(ListOutputContext list)
        {
            var output = new StringBuilder();

            if (list.Links.Count > 0)
            {
                output.Append("\n\n");

                int linkIndex = 0;
                foreach (var link in list.Links)
                {
                    output
                        .Append(linkIndex + 1)
                        .Append(" [")
                        .Append(link.Title ?? link.Href)
                        .Append("](")
                        .Append(link.Href)
                        .Append(")\n");

                    linkIndex++;
                }

                output.Append("\n");
            }
            else
            {
                if (!string.IsNullOrEmpty(list.DefaultText))
                {
                    output.Append("\n").Append(list.DefaultText).Append("\n");
                }
            }

            return output.ToString();
        }

        private static string RenderListBullets(ListOutputContext list)
        {
            StringBuilder output = new StringBuilder();

            if (list.Links.Count > 0)
            {
                output.Append("\n\n");

                foreach (var link in list.Links)
                {
                    output
                        .Append("* [")
                        .Append(link.Title ?? link.Href)
                        .Append("](")
                        .Append(link.Href)
                        .Append(")\n");
                }

                output.Append("\n");
            }
            else
            {
                if (!string.IsNullOrEmpty(list.DefaultText))
                {
                    output.Append("\n").Append(list.DefaultText).Append("\n");
                }
            }

            return output.ToString();
        }

        private static string RenderListHeading(ListOutputContext list)
        {
            const int HeadingLevel = 2;

            StringBuilder output = new StringBuilder();

            if (list.Links.Count > 0)
            {
                output.Append("\n\n");

                int linkIndex = 0;
                foreach (var link in list.Links)
                {
                    output
                        .Append('#', HeadingLevel)
                        .Append(" [")
                        .Append(link.Title ?? link.Href)
                        .Append("](")
                        .Append(link.Href)
                        .Append(")\n");

                    linkIndex++;
                }

                output.Append("\n");
            }
            else
            {
                if (!string.IsNullOrEmpty(list.DefaultText))
                {
                    output.Append("\n").Append(list.DefaultText).Append("\n");
                }
            }

            return output.ToString();
        }

        struct ListOutputContext
        {
            public string DefaultText;
            public IReadOnlyCollection<LinkToArticle> Links;
        }

        delegate string RenderListInMarkdown(ListOutputContext list);

        enum ListStyle
        {
            Bullet,
            Number,
            Heading
        }

        class ListContextBuilder
        {
            private string filePattern = "*";
            private string folderPattern = ".";
            private string excludePattern = "";
            private int depth = 2;
            private int limit = 10;
            private ListStyle style = ListStyle.Bullet;
            private string defaultText = "";
            private readonly HashSet<FrontmatterVariableAndValue> conditions = new HashSet<FrontmatterVariableAndValue>();
            private FileModel? file;
            private ImmutableList<FileModel> allFiles = ImmutableList<FileModel>.Empty;
            private ModelsPerFrontmatter modelsPerFrontmatter = new ModelsPerFrontmatter();

            public ListContextBuilder ModelsPerVariable(ModelsPerFrontmatter modelsPerVariableWithValue)
            {
                this.modelsPerFrontmatter = modelsPerVariableWithValue;
                return this;
            }

            public ListContextBuilder AllFiles(ImmutableList<FileModel> allFiles)
            {
                this.allFiles = allFiles ?? ImmutableList<FileModel>.Empty;
                return this;
            }

            public ListContextBuilder File(FileModel file)
            {
                this.file = file;
                return this;
            }

            public ListContextBuilder DefaultText(string text)
            {
                this.defaultText = text;
                return this;
            }

            public ListContextBuilder FilePattern(string pattern)
            {
                this.filePattern = pattern;
                return this;
            }

            public ListContextBuilder FolderPattern(string pattern)
            {
                this.folderPattern = pattern;
                return this;
            }

            public ListContextBuilder Depth(int depth)
            {
                this.depth = depth;
                return this;
            }

            public ListContextBuilder Limit(int limit)
            {
                this.limit = limit;
                return this;
            }

            public ListContextBuilder Style(ListStyle style)
            {
                this.style = style;
                return this;
            }

            public ListContextBuilder ExcludePattern(string pattern)
            {
                this.excludePattern = pattern;
                return this;
            }

            public ListContextBuilder Condition(string key, string value)
            {
                return Condition(new FrontmatterVariableAndValue(key, value));
            }

            public ListContextBuilder Condition(FrontmatterVariableAndValue var)
            {
                switch (var.Key)
                {
                    case "file":
                        filePattern = var.OriginalValue;
                        break;

                    case "folder":
                        filePattern = var.OriginalValue;
                        break;

                    case "depth":
                        if (!int.TryParse(var.OriginalValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out this.depth))
                        {
                            throw new FormatException($"Cannot parse depth as integer: '{var.OriginalValue}'");
                        }
                        break;

                    case "limit":
                        if (!int.TryParse(var.OriginalValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out this.limit))
                        {
                            throw new FormatException($"Cannot parse limit as integer: '{var.OriginalValue}'");
                        }
                        break;

                    case "style":
                        if (!Enum.TryParse<ListStyle>(var.OriginalValue, ignoreCase: true, out this.style))
                        {
                            var possibleValues = string.Join(", " , Enum.GetNames(typeof(ListStyle)));
                            throw new FormatException($"Cannot parse style '{var.OriginalValue}'. Must be one of {possibleValues}");
                        }
                        break;

                    case "default-text":
                        this.defaultText = var.OriginalValue;
                        break;

                    default:
                        conditions.Add(var);
                        break;
                }

                return this;
            }

            public ListContext Build()
            {
                if (file == null) throw new NotSupportedException("Must set file");

                return new ListContext(
                    filePattern: filePattern,
                    folderPattern: folderPattern,
                    excludePattern: excludePattern,
                    defaultText: defaultText,
                    allFiles: allFiles,
                    modelsPerFrontmatter: modelsPerFrontmatter,
                    style: style,
                    depth: depth,
                    limit: limit,
                    file: file,
                    conditions: conditions);
            }
        }

        class ListContext
        {
            public readonly string FilePattern;
            public readonly string FolderPattern;
            public readonly string ExcludePattern;
            public readonly ImmutableHashSet<FrontmatterVariableAndValue> Conditions;
            public readonly ImmutableList<FileModel> AllFiles;
            public readonly ModelsPerFrontmatter ModelsPerFrontmatter;
            public readonly ListStyle Style;
            public readonly int Depth;
            public readonly int Limit;
            public readonly FileModel File;
            public readonly string DefaultText;
            public readonly RenderListInMarkdown RenderAsMarkdown;
            

            public ListContext(
                string filePattern,
                string folderPattern,
                string excludePattern,
                string defaultText,
                ListStyle style,
                int depth,
                int limit,
                FileModel file,
                ImmutableList<FileModel> allFiles,
                ModelsPerFrontmatter modelsPerFrontmatter,
                IEnumerable<FrontmatterVariableAndValue> conditions)
            {
                this.FilePattern = filePattern;
                this.FolderPattern = folderPattern;
                this.ExcludePattern = excludePattern;
                this.DefaultText = defaultText;
                this.Style = style;
                this.Depth = depth;
                this.Limit = limit;
                this.File = file;
                this.AllFiles = allFiles;
                this.ModelsPerFrontmatter = modelsPerFrontmatter;
                this.Conditions = ImmutableHashSet.Create(conditions.ToArray());
                
                switch (style)
                {
                    case ListStyle.Heading:
                        this.RenderAsMarkdown = ProcessLists.RenderListHeading;
                        break;

                    case ListStyle.Number:
                        this.RenderAsMarkdown = ProcessLists.RenderListOrdered;
                        break;

                    case ListStyle.Bullet:
                    default:
                        this.RenderAsMarkdown = ProcessLists.RenderListBullets;
                        break;
                }
            }
        }

        [DebuggerDisplay("({Title})[{Href}]")]
        struct LinkToArticle: IComparable<LinkToArticle>, IEquatable<LinkToArticle>
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

    [DebuggerDisplay("D {FullPath}")]
    class VirtualDirectory
    {
        public string FullPath;
        public string Name;
        public VirtualDirectory? Parent;
        public readonly HashSet<VirtualDirectory> SubDirectories;
        public readonly HashSet<VirtualFile> Files;

        public VirtualDirectory(string shortPath, string fullPath)
        {
            FullPath = fullPath;
            Name = shortPath;
            Files = new HashSet<VirtualFile>();
            SubDirectories = new HashSet<VirtualDirectory>();
        }
    }

    [DebuggerDisplay("F {FullName}")]
    class VirtualFile
    {
        private readonly FileModel fileModel;

        public string Name { get; private set; }
        public string FullName { get; private set; }
        public VirtualDirectory? Directory { get; set; }
        public FileModel FileModel => fileModel;

        public VirtualFile(FileModel model)
        {
            this.fileModel = model;
            this.Name = model.FileAndType.File;
            this.FullName = model.FileAndType.FullPath;
        }
    }

    static class PrivateFileModelExtensions
    {
        private const string Frontmatter = "_frontmatter";
        private const string Directory = "_virtual_directory";
        private const string File = "_virtual_file";

        public static VirtualDirectory? GetVirtualDirectory(this FileModel model)
        {
            return model.GetVirtualFile().Directory;
        }

        public static VirtualFile GetVirtualFile(this FileModel model)
        {
            var content = model.GetContent();
            VirtualFile virtualFile;
            if (!content.TryGetValue(File, out object fileRaw))
            {
                virtualFile = new VirtualFile(model);
                content.Add(File, virtualFile);
            }
            else
            {
                virtualFile = (VirtualFile)fileRaw;
            }
            return virtualFile;
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

        public static void SetFrontmatter(this FileModel model, FrontmatterVariableAndValue[] frontmatter)
        {
            var content = model.GetContent();
            content[Frontmatter] = frontmatter;
        }

        public static FrontmatterVariableAndValue[] GetFrontmatter(this FileModel model)
        {
            var content = model.GetContent();
            if (content.TryGetValue(Frontmatter, out object fmRaw)
                && fmRaw is FrontmatterVariableAndValue[] fmArray)
            {
                return fmArray;
            }
            else
            {
                return Array.Empty<FrontmatterVariableAndValue>();
            }
        }
    }
}
