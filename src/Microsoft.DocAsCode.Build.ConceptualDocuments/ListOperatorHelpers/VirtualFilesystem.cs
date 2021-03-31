//using System;
//using System.Collections.Generic;
//using System.Collections.Immutable;
//using System.Diagnostics;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Microsoft.DocAsCode.Plugins;

//namespace Microsoft.DocAsCode.Build.ConceptualDocuments.ListOperatorHelpers
//{
//    public class VirtualFilesystem
//    {
//        private VirtualDirectory rootDirectory = new VirtualDirectory("~", "~");
//        private ImmutableDictionary<string, VirtualDirectory> virtualDirectoryCache =
//            ImmutableDictionary<string, VirtualDirectory>.Empty;
//        private ImmutableDictionary<string, VirtualFile> virtualFileCache =
//            ImmutableDictionary<string, VirtualFile>.Empty;

//        public VirtualDirectory RootDir => rootDirectory;

//        public void BuildDirectoryHierarchy(ImmutableList<FileModel> models)
//        {
//            rootDirectory = new VirtualDirectory("~", "~");
//            Dictionary<string, VirtualDirectory> diretories = new Dictionary<string, VirtualDirectory>();
//            Dictionary<string, VirtualFile> files = new Dictionary<string, VirtualFile>();
//            diretories.Add("~", rootDirectory);

//            (string name, string fullName) ParseFileBaseDirectory(string key)
//            {
//                int idxFull = key.LastIndexOf('/');
//                if (idxFull == -1) return ("~", "~");

//                string fullName = key.Substring(0, idxFull);
//                if (fullName == "~") return ("~", "~");

//                int idxShort = fullName.LastIndexOf('/');
//                if (idxShort == -1) throw new FormatException();

//                string shortName = fullName.Substring(idxShort + 1);

//                return (shortName, fullName);
//            }

//            VirtualDirectory? FindBaseDirectory(VirtualDirectory dir)
//            {
//                if (dir.FullPath == "~") return null;

//                var (parentName, parentFullName) = ParseFileBaseDirectory(dir.FullPath);

//                var parent = GetDir(parentName, parentFullName);
//                return parent;
//            }

//            VirtualFile GetOrCreateFile(FileModel model)
//            {
//                if (!files.TryGetValue(model.Key, out var file))
//                {
//                    file = new VirtualFile(model);
//                    files.Add(model.Key, file);
//                }
//                return file;
//            }

//            VirtualDirectory GetDir(string name, string fullPath)
//            {
//                if (!diretories.TryGetValue(fullPath, out var dir))
//                {
//                    dir = new VirtualDirectory(name, fullPath);
//                    dir.Parent = FindBaseDirectory(dir);
//                    if (dir.Parent != null)
//                    {
//                        dir.Parent.SubDirectories.Add(dir);
//                    }
//                    diretories.Add(fullPath, dir);
//                }
//                return dir;
//            }

//            foreach (var model in models)
//            {
//                var path = model.Key;
//                var (name, fullPath) = ParseFileBaseDirectory(model.Key);
//                var dir = GetDir(name, fullPath);

//                var vf = GetOrCreateFile(model);
//                vf.Directory = dir;

//                dir.Files.Add(vf);
//            }

//            var dirsBuilder = virtualDirectoryCache.ToBuilder();
//            dirsBuilder.Clear();
//            dirsBuilder.AddRange(diretories);
//            virtualDirectoryCache = dirsBuilder.ToImmutable();

//            var filesBuilder = virtualFileCache.ToBuilder();
//            filesBuilder.Clear();
//            filesBuilder.AddRange(files);
//            virtualFileCache = filesBuilder.ToImmutable();
//        }

//        public int CalculateDepthRelativeTo(FileModel root, FileModel needle)
//        {
//            var vfRoot = GetVirtualFile(root);
//            var vfNeedle = GetVirtualFile(needle);

//            int depth = 0;
//            VirtualDirectory? needleDir = vfNeedle.Directory;
//            while (vfRoot.Directory != needleDir)
//            {
//                if (needleDir == null || needleDir == rootDirectory)
//                {
//                    depth = int.MaxValue;
//                    break;
//                }

//                needleDir = needleDir.Parent;
//                depth++;
//            }

//            return depth;
//        }

//        public VirtualDirectory? GetDirectoryByPath(VirtualFile relativeTo, string path)
//        {
//            if (path == ".")
//            {
//                return relativeTo.Directory;
//            }
//            else 
//            {
//                char separator = path[0];
//                // start search from root
//                string[] parts = path.Split(separator);
//                VirtualDirectory result =
//                    (path.StartsWith("/") || path.StartsWith("~"))
//                    ? rootDirectory
//                    : relativeTo.Directory;
//                for (int i = 0; i < parts.Length; i++)
//                {
//                    var part = parts[i];
//                    VirtualDirectory? child
//                        i == parts.Length = -1
//                        ? rela
                        
//                        = result.SubDirectories.Where(x => string.Equals(x.Name, part, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
//                    if (child == null) return null;
//                    result = child;
//                }
//                return result;
//            }
//        }

//        public VirtualDirectory? GetVirtualDirectory(FileModel model)
//        {
//            VirtualFile virtFile = GetVirtualFile(model);
//            VirtualDirectory? virtDir = virtFile.Directory;
//            return virtDir;
//        }

//        public VirtualFile GetVirtualFile(FileModel model)
//        {
//            if (virtualFileCache.TryGetValue(model.Key, out var file))
//            {
//                return file;
//            }
//            else
//            {
//                throw new KeyNotFoundException(model.Key);
//            }
//        }
//    }

//    [DebuggerDisplay("D {FullPath}")]
//    public class VirtualDirectory
//    {
//        public string FullPath;
//        public string Name;
//        public VirtualDirectory? Parent;
//        public readonly HashSet<VirtualDirectory> SubDirectories;
//        public readonly HashSet<VirtualFile> Files;

//        public VirtualDirectory(string shortPath, string fullPath)
//        {
//            FullPath = fullPath;
//            Name = shortPath;
//            Files = new HashSet<VirtualFile>();
//            SubDirectories = new HashSet<VirtualDirectory>();
//        }

//        public bool IsChild(VirtualFile file)
//        {
//            bool isChild = Files.Contains(file);
//            if (!isChild)
//            {
//                foreach (var subDir in SubDirectories)
//                {
//                    isChild = IsChild(file);
//                    if (isChild)
//                        break;
//                }
//            }
//            return isChild;
//        }

//        public bool MatchesPattern(string pattern)
//        {
//            return ListContext.MatchesFilePattern(FullPath, pattern);
//        }

//        public bool IsSubDirectoryOf(VirtualDirectory potentialParent)
//        {
//            if (potentialParent == null) return false;

//            if (potentialParent.SubDirectories.Contains(this)) return true;

//            foreach (var subDir in potentialParent.SubDirectories)
//            {
//                var res = IsSubDirectoryOf(subDir);
//                if (res) return true;
//            }

//            return false;
//        }
//    }

//    [DebuggerDisplay("F {FullName}")]
//    public class VirtualFile
//    {
//        private readonly FileModel fileModel;

//        public string Name { get; private set; }
//        public string FullName { get; private set; }
//        public VirtualDirectory? Directory { get; set; }
//        public FileModel FileModel => fileModel;

//        public VirtualFile(FileModel model)
//        {
//            this.fileModel = model;
//            this.Name = model.FileAndType.File;
//            this.FullName = model.FileAndType.FullPath;
//        }
//    }
//}
