using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.DocAsCode.Build.ConceptualDocuments.ListOperatorHelpers;
using Microsoft.DocAsCode.Plugins;
using Xunit;

namespace Microsoft.DocAsCode.Build.ConceptualDocuments.Tests
{
    public class ListContextTests
    {
        [Theory]
        [InlineData("~/data.txt", "~")]
        [InlineData("~/dir/data.txt", "~/dir")]
        [InlineData((string)null, (string)null)]
        [InlineData("", "")]
        [InlineData("~/some/long/path/file.vb", "~/some/long/path")]
        public void RootedDirFromFileKey(string input, string expectedOutput)
        {
            string result = ListContext.RootedDirFromFileKey(input);

            Assert.Equal(expectedOutput, result);
        }

        [Theory]
        [InlineData("~", "file.txt", "~/file.txt")]
        [InlineData("~/", "file.txt", "~/file.txt")]
        [InlineData("~/", "/file.txt", "~/file.txt")]
        [InlineData("~/dir", "sub-dir/file.txt", "~/dir/sub-dir/file.txt")]
        [InlineData("~/dir/", "sub-dir/file.txt", "~/dir/sub-dir/file.txt")]
        [InlineData("~/dir", "/sub-dir/file.txt", "~/dir/sub-dir/file.txt")]
        [InlineData("~/dir/", "/sub-dir/file.txt", "~/dir/sub-dir/file.txt")]
        public void AppendKeys(string key, string addon, string expectedOutput)
        {
            string result = ListContext.AppendKeys(key, addon);

            Assert.Equal(expectedOutput, result);
        }

        [Theory]
        [InlineData("~/file.txt", "~")]
        [InlineData("~/sub-dir/file.txt", "sub-dir")]
        [InlineData("~/home/me/pictures/portrait.png", "pictures")]
        [InlineData("~/home/me/crash.txt", "me")]
        public void DirectoryNameFromKey(string key, string expectedOutput)
        {
            string result = ListContext.DirectoryNameFromKey(key);

            Assert.Equal(expectedOutput, result);
        }

        [Theory]
        [InlineData("~/file1.txt", "~/file1.txt", 0)]
        [InlineData("~/file1.txt", "~/file2.txt", 0)]
        [InlineData("~/file1.txt", "~/sub-dir/file2.txt", 1)]
        [InlineData("~/sub-dir/file2.txt", "~/file1.txt", 1)]
        [InlineData("~/file1.txt", "~/home/me/file2.txt", 2)]
        [InlineData("~/home/batman/file1.txt", "~/home/superman/file2.txt", -1)]
        [InlineData("~/home/batman/Pictures/wedding/file2.txt", "~/home/batman/Pictures/file.txt", 1)]
        [InlineData("~/home/batman/bio.txt", "~/home/batman/Documents/HR/CV.txt", 2)]
        [InlineData("~/home/batman/bio.txt", "~/home/batman/Documents/HR/Complaints/joker.txt", 3)]
        public void Depth(string lhs, string rhs, int expectedDepth)
        {
            int actualDepth = ListContext.DepthBetweenKeys(lhs, rhs);

            Assert.Equal(expectedDepth, actualDepth);
        }

        [Theory]
        [InlineData("", "")]
        [InlineData((string)null, (string)null)]
        [InlineData("~", "")]
        [InlineData("~/", "")]
        [InlineData("~/file.txt", "file.txt")]
        [InlineData("~/home/cv.txt", "cv.txt")]
        [InlineData("~/home/pictures/profile.gif", "profile.gif")]
        public void FileNameFromKey(string key, string expectedResult)
        {
            string actualFilename = ListContext.FileNameFromKey(key);

            Assert.Equal(expectedResult, actualFilename);
        }
    }
}
