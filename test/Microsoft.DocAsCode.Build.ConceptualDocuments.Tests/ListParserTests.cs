using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.DocAsCode.Build.ConceptualDocuments.ListOperatorHelpers;
using Xunit;

namespace Microsoft.DocAsCode.Build.ConceptualDocuments.Tests
{
    public class ListParserTests
    {
        [Fact]
        public void CanParseFile1()
        {
            const string file1 = @"# Advanced Section

This section contains information for advanced concepts, systems and deep dive in the security.
It is intended primarily for advanced ERP implementation consultants.

## Advanced Systems

- [Documents](documents/index.md) - document-related concepts and services.
- [Custom Attributes](custom-attributes/overview.md) - user-defined data attributes.
- [Calculated Attributes](calculated-attributes/overview.md) - user-defined calculations.
- [Business Rules](business-rules/overview.md) - user-defined and system business rules.
- [Data Objects](data-objects/index.md) - data object extensibility systems.

## Advanced Concepts

- [Master / Detail Attributes](concepts/master-detail-attributes.md)
- [Object / Relational Mapping](concepts/object-relational-mapping.md)
- [Aggregates](concepts/aggregates.md)

## Temp

[!list folder=""/calculated-attributes"" file=""*"" depth=2 limit=50 style=bullet]
";

            var result = ListOperatorParser.Parse(file1);

            Assert.Empty(result.Errors);
            Assert.Single(result.Lists);

            var list = result.Lists[0];

            Assert.Equal("/calculated-attributes", list.FolderPattern);
            Assert.Equal("*", list.FilePattern);
            Assert.Equal(2, list.Depth);
            Assert.Equal(50, list.Limit);
            Assert.Equal(ListStyle.Bullet, list.Style);
        }

        [Theory]
        [InlineData("[!list fruit=bananna]")]
        [InlineData("[!list \"fruit\"=bananna]")]
        [InlineData("[!list fruit=\"bananna\"]")]
        [InlineData("[!list \"fruit\"=\"bananna\"]")]
        public void CanParseAnyCombinationOfQuotedKeyValuePair(string input)
        {
            var result = ListOperatorParser.Parse(input);
            Assert.Empty(result.Errors);
            Assert.Single(result.Lists);
            
            var list = result.Lists[0];
            Assert.Single(list.Conditions);
            
            var cond = list.Conditions.First();
            Assert.Equal("fruit", cond.Key);
            Assert.Equal("bananna", cond.Value);
        }

        [Fact]
        public void CanParseEmptyList()
        {
            var r = ListOperatorParser.Parse(@"[!list]");

            Assert.Empty(r.Errors);
            Assert.Single(r.Lists);
        }

        [Fact]
        public void CanParseDefaultText()
        {
            var r = ListOperatorParser.Parse(@"# Operators

[!list items=Operators default-text=""None""]");

            Assert.Empty(r.Errors);
            Assert.Single(r.Lists);

            var list = r.Lists[0];
            Assert.Single(list.Conditions);

            Assert.Equal("None", list.DefaultText);

            var cond = list.Conditions.First();
            Assert.Equal("items", cond.Key);
            Assert.Equal("Operators", cond.Value);
        }

        [Fact]
        public void StartingAndEndingIndexAreCorrectlySet()
        {
            const string input = @"# Operators

[!list items=Operators default-text=""None""]
";

            int startingIndex = input.IndexOf("[!list ");
            int endingIndex = input.IndexOf("]", startingIndex + "[!list ".Length);

            var r = ListOperatorParser.Parse(input);

            Assert.Empty(r.Errors);

            var list = r.Lists[0];
            
            Assert.Equal(startingIndex, list.MatchedExpression.StartingIndex);
            Assert.Equal(endingIndex, list.MatchedExpression.EndingIndex);
        }

        [Theory]
        [InlineData("long-text-with-dash", "long-text-with-dash")]
        [InlineData("t23_asd", "t23_asd")]
        public void ReadWhileIdentifierCanHandleSpecialSymbols(string input, string output)
        {
            var context = ListOperatorParser.Context.Start(input);
            StringBuilder sink = new StringBuilder();
            var nextContext = ListOperatorParser.ReadWhileIdentifier(context, sink);

            Assert.Equal(output, sink.ToString());
        }

        [Theory]
        [InlineData("")]
        [InlineData((string)null)]
        public void DoesntFailOnEmptyInput(string input)
        {
            var r = ListOperatorParser.Parse(input);

            Assert.Empty(r.Errors);
            Assert.Empty(r.Lists);
        }

        [Fact]
        public void CanParseMultipleLists()
        {
            var r = ListOperatorParser.Parse(@"abra cadabra [!list style=number file=oops]
some random text
more random text ad alsdlasdlasdl
[!list style=bullet limit=40 exclude=""dogecoin""]
adad a da dgjksd gglkasl f
sfskdflkd;lsfk
");

            Assert.Empty(r.Errors);
            Assert.Equal(2, r.Lists.Length);
        }
    }
}
