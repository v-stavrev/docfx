using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.DocAsCode.Plugins;

namespace Microsoft.DocAsCode.Build.ConceptualDocuments.ListOperatorHelpers
{
    public static class ListOperatorRenderer
    {
        

        public static string RenderListOrdered(ListOutputContext list)
        {
            var output = new StringBuilder();

            if (list.Links.Count > 0)
            {
                output.Append("\n\n");

                int linkIndex = 1;
                foreach (var link in list.Links)
                {
                    output
                        .Append(linkIndex)
                        .Append(". [")
                        .Append(link.Title ?? link.Href)
                        .Append("](")
                        .Append(link.Href)
                        .Append(")\n");

                    linkIndex++;
                }

                if (list.SomeItemsAreHidden)
                {
                    output
                        .Append(linkIndex)
                        .Append(". ...\n");
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

        public static string RenderListBullets(ListOutputContext list)
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

                if (list.SomeItemsAreHidden)
                {
                    output
                        .Append("* ...\n");
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

        public static string RenderListHeading(ListOutputContext list)
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

                if (list.SomeItemsAreHidden)
                {
                    output
                        .Append('#', HeadingLevel)
                        .Append(" ...\n");
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
    }
}
