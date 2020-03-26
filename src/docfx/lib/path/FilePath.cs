// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;

namespace Microsoft.Docs.Build
{
    /// <summary>
    /// Represents a serializable machine independent file identifier.
    /// </summary>
    internal class FilePath : IEquatable<FilePath>, IComparable<FilePath>
    {
        /// <summary>
        /// Gets the file path relative to the main docset.
        /// </summary>
        public PathString Path { get; }

        /// <summary>
        /// Gets the file format.
        /// Prefer this over file extension because .NET xml file path ends with .xml, but the format is yaml.
        /// </summary>
        public FileFormat Format { get; }

        /// <summary>
        /// Gets the name of the dependency if it is from dependency repo.
        /// </summary>
        public PathString DependencyName { get; }

        /// <summary>
        /// Gets the value to indicate where is this file from.
        /// </summary>
        public FileOrigin Origin { get; }

        /// <summary>
        /// Indicate if the file is from git commit history.
        /// </summary>
        public bool IsGitCommit { get; }

        public FilePath(string path, FileOrigin origin = FileOrigin.Default)
        {
            Debug.Assert(origin != FileOrigin.Dependency);

            Path = new PathString(path);
            Format = GetFormat(path);
            Origin = origin;
        }

        public FilePath(string path, bool isGitCommit)
        {
            Path = new PathString(path);
            Format = GetFormat(path);
            Origin = FileOrigin.Fallback;
            IsGitCommit = isGitCommit;
        }

        public FilePath(PathString path, PathString dependencyName)
        {
            Path = path;
            Format = GetFormat(path);
            DependencyName = dependencyName;
            Origin = FileOrigin.Dependency;

            Debug.Assert(Path.StartsWithPath(DependencyName, out _));
        }

        public static bool operator ==(FilePath? a, FilePath? b) => Equals(a, b);

        public static bool operator !=(FilePath? a, FilePath? b) => !Equals(a, b);

        public override string ToString()
        {
            var tags = "";

            switch (Origin)
            {
                case FileOrigin.Default:
                    break;

                case FileOrigin.Dependency:
                    tags += $"[{DependencyName}]";
                    break;

                default:
                    tags += $"[{Origin.ToString().ToLowerInvariant()}]";
                    break;
            }

            if (IsGitCommit)
            {
                tags += $"!";
            }

            return tags.Length > 0 ? $"{Path} {tags}" : $"{Path}";
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as FilePath);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Path, DependencyName, Origin, IsGitCommit);
        }

        public bool Equals(FilePath? other)
        {
            if (other is null)
            {
                return false;
            }

            return Path.Equals(other.Path) &&
                   DependencyName.Equals(other.DependencyName) &&
                   other.Origin == Origin &&
                   IsGitCommit == other.IsGitCommit;
        }

        public int CompareTo(FilePath other)
        {
            var result = Path.CompareTo(other.Path);
            if (result == 0)
                result = Origin.CompareTo(other.Origin);
            if (result == 0)
                result = DependencyName.CompareTo(other.DependencyName);
            if (result == 0)
                result = IsGitCommit.CompareTo(other.IsGitCommit);

            return result;
        }

        private static FileFormat GetFormat(string path)
        {
            if (path.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
            {
                return FileFormat.Markdown;
            }

            if (path.EndsWith(".yml", StringComparison.OrdinalIgnoreCase))
            {
                return FileFormat.Yaml;
            }

            if (path.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                return FileFormat.Json;
            }

            return FileFormat.Unknown;
        }
    }
}
