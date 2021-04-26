using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.DocAsCode.Plugins;

namespace Microsoft.DocAsCode.Build.ConceptualDocuments.ListOperatorHelpers
{
    public class ListContext
    {
        public readonly ImmutableList<FileModel> AllFiles;
        public readonly ModelsPerFrontmatter ModelsPerFrontmatter;
        public readonly FileModel File;

        public RenderListInMarkdown RenderAsMarkdown(ListStyle style) =>
            style switch
            {
                ListStyle.Heading => ListOperatorRenderer.RenderListHeading,
                ListStyle.Bullet => ListOperatorRenderer.RenderListBullets,
                ListStyle.Number => ListOperatorRenderer.RenderListOrdered,
                _ => ListOperatorRenderer.RenderListBullets
            };

        public ListContext(
            FileModel file,
            ImmutableList<FileModel> allFiles,
            ModelsPerFrontmatter modelsPerFrontmatter
        )
        {
            this.File = file;
            this.AllFiles = allFiles;
            this.ModelsPerFrontmatter = modelsPerFrontmatter;
        }

        internal static StringBuilder FileGlobToRegex(string glob)
        {
            StringBuilder builder = new StringBuilder();
            foreach (char symbol in glob)
            {
                switch (symbol)
                {
                    case '*':
                        builder.Append(".*");
                        break;

                    case '?':
                        builder.Append(".?");
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
            return builder;
        }

        internal static string RootedDirFromFileKey(string fileKey)
        {
            // keys start like ~/somepath/sub-dir/file.txt
            return LastSegment(fileKey, "~");
        }

        internal static string LastSegment(string input, string valueOnError)
        {
            if (string.IsNullOrEmpty(input)) return input;

            int lastSeparator = input.LastIndexOf('/');
            if (lastSeparator == -1) return valueOnError;

            string dirKey = input.Substring(0, lastSeparator);
            return dirKey;
        }

        internal static string RelativeDirFromFilePath(string filePath)
        {
            // paths start like somepath/sub-dir/file.txt
            return LastSegment(filePath, "");
        }

        internal static string DirectoryNameFromKey(string fileKey)
        {
            int lastSeparator = fileKey.LastIndexOf('/');
            if (lastSeparator == -1) return null;

            int separatorBeforeThat = fileKey.LastIndexOf('/', lastSeparator - 1);
            if (separatorBeforeThat != -1)
            {
                string dirName = fileKey.Substring(separatorBeforeThat + 1, lastSeparator - separatorBeforeThat - 1);
                return dirName;
            }
            else
            {
                string dirName = fileKey.Substring(0, lastSeparator);
                return dirName;
            }
        }

        internal static string AppendKeys(string key, string addon)
        {
            if (!key.StartsWith("~")) throw new ArgumentException("key must start with ~");

            StringBuilder result = new StringBuilder();
            result.Append(key);

            // key ends with slash
            if (result[result.Length - 1] != '/')
                result.Append('/');

            if (addon.Length > 0)
            {
                // strip addon starting slash
                if (addon[0] == '/')
                    result.Append(addon, 1, addon.Length - 1);
                else
                    result.Append(addon);
            }

            return result.ToString();
        }

        internal static int DepthBetweenKeys(string lhs, string rhs)
        {
            string lhsBase = RootedDirFromFileKey(lhs);
            string rhsBase = RootedDirFromFileKey(rhs);

            if (string.Equals(lhsBase, rhsBase, StringComparison.OrdinalIgnoreCase))
                return 0;

            string leading, trailing;
            if (lhsBase.StartsWith(rhsBase, StringComparison.OrdinalIgnoreCase))
            {
                leading = rhsBase;
                trailing = lhsBase;
            }
            else if (rhsBase.StartsWith(lhsBase, StringComparison.OrdinalIgnoreCase))
            {
                leading = lhsBase;
                trailing = rhsBase;
            }
            else
            {
                return -1; // not related
            }

            int CountSlashes(string input)
            {
                int countSlashes = 0;
                for (int i = 0; i < input.Length; i++)
                {
                    if (trailing[i] == '/')
                        countSlashes++;
                }
                return countSlashes;
            }

            int leadingSlashes = CountSlashes(leading);
            int trailingSlashes = CountSlashes(trailing);

            int levels = trailingSlashes - leadingSlashes;

            return levels;
        }

        internal static string FileNameFromKey(string key)
        {
            if (string.IsNullOrEmpty(key)) return key;

            int lastIdx = key.LastIndexOf('/');
            if (lastIdx == -1 || lastIdx == key.Length - 1) return string.Empty;

            string fileName = key.Substring(lastIdx + 1);
            return fileName;
        }

        public (LinkToArticle[] Links, bool SomeItemsAreHidden) FindRelatedArticles(ListOperator list)
        {
            FileModel[] src = AllFiles.ToArray();

            // directory pattern
            if (!string.IsNullOrEmpty(list.FolderPattern))
            {
                if (list.FolderPattern == "*" || list.FolderPattern == ".")
                {
                    // other file's paths must start with my path

                    string requiredPrefix = RootedDirFromFileKey(File.Key);

                    src = src
                        .Where(other => other.Key.StartsWith(requiredPrefix, StringComparison.OrdinalIgnoreCase))
                        .ToArray();
                }
                else
                {
                    string pattern = list.FolderPattern;
                    if (pattern.StartsWith("~/"))
                    {
                        // ok
                    }
                    else if (pattern.StartsWith("/"))
                    {
                        // prefix so that it looks like key
                        pattern = string.Concat('~', pattern);
                    }
                    else
                    {
                        // local, append current direcotyr
                        pattern = AppendKeys(RootedDirFromFileKey(File.Key), pattern);
                    }

                    var builder = FileGlobToRegex(pattern);

                    // starts with
                    builder.Insert(0, '^');
                    // optional final slash
                    builder.Append(@"/?");
                    var re = new Regex(builder.ToString(), RegexOptions.IgnoreCase | RegexOptions.Compiled);

                    //var orig = src;

                    src = src
                        .Where(other => re.IsMatch(RootedDirFromFileKey(other.Key)))
                        .ToArray();
                }
            }

            // exclude pattern
            if (string.IsNullOrEmpty(list.ExcludePattern))
            {
                // exclude self
                src = src.Where(x => x != File).ToArray();
            }
            else
            {
                var pattern = new Regex(FileGlobToRegex(list.ExcludePattern).ToString(), RegexOptions.IgnoreCase | RegexOptions.Compiled);

                src = src
                    .Where(x => x != File
                        && !pattern.IsMatch(DirectoryNameFromKey(x.Key)))
                    .ToArray();
            }

            // directory depth
            if (list.Depth > 0)
            {
                var countBeforeDepth = src.Length;

                src = src
                    .Where(x =>
                    {
                        int depth = DepthBetweenKeys(x.Key, File.Key);
                        bool include = depth != -1 && depth <= list.Depth;
                        return include;
                    })
                    .ToArray();
            }

            // file pattern
            if (!string.IsNullOrEmpty(list.FilePattern) && list.FilePattern != "*")
            {
                var builder = FileGlobToRegex(list.FilePattern);
                // starts with
                builder.Insert(0, '^');
                // ends with
                builder.Append('$');

                var re = new Regex(builder.ToString(), RegexOptions.IgnoreCase | RegexOptions.Compiled);
                
                src = src
                    .Where(other => re.IsMatch(FileNameFromKey(other.Key)))
                    .ToArray();
            }

            if (list.Conditions.Count > 0)
            {
                src = src
                    .Where(x => list.Conditions.All(cond => x.HasFrontmatterWithValue(cond.Key, cond.Value)))
                    .ToArray();
            }

            bool someItemsAreHiddne = src.Length > list.Limit;

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
                    string title = item.GetTitle() ?? string.Empty;
                    string href = item.Key;

                    return new LinkToArticle(title, href);
                })
                .OrderBy(x => x.Title);
            LinkToArticle[] sorted = resultBuilder.ToArray();
            return (sorted, someItemsAreHiddne);
        }
    }
}
