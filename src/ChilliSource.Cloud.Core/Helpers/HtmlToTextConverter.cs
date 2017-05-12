using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Core
{
    //TODO attribution

    /// <summary>
    /// Contains methods to convert HTML to text.
    /// </summary>
    public static class HtmlToTextConverter
    {
        /// <summary>
        /// Removes HTML tags from the specified string.
        /// </summary>
        /// <param name="source">The specified string.</param>
        /// <returns>A string value without HTML tags.</returns>
        public static string StripHtml(string source, bool preserveFormatting = false)
        {
            if (String.IsNullOrEmpty(source)) return source;

            string result = source;

            if (!preserveFormatting)
            {
                // Remove HTML Development formatting
                // Replace line breaks with space
                // because browsers inserts space
                result = result.Replace("\r", " ");
                // Replace line breaks with space
                // because browsers inserts space
                result = result.Replace("\n", " ");
                // Remove step-formatting
                result = result.Replace("\t", string.Empty);
                // Remove repeating spaces because browsers ignore them
                result = Regex.Replace(result, @"( )+", " ");
            }

            // Remove the header (prepare first by clearing attributes)
            result = Regex.Replace(result,
                        @"<( )*head([^>])*>", "<head>",
                        RegexOptions.IgnoreCase);
            result = Regex.Replace(result,
                        @"(<( )*(/)( )*head( )*>)", "</head>",
                        RegexOptions.IgnoreCase);
            result = Regex.Replace(result,
                        "(<head>).*(</head>)", string.Empty,
                        RegexOptions.IgnoreCase);

            // remove all scripts (prepare first by clearing attributes)
            result = Regex.Replace(result,
                        @"<( )*script([^>])*>", "<script>",
                        RegexOptions.IgnoreCase);
            result = Regex.Replace(result,
                        @"(<( )*(/)( )*script( )*>)", "</script>",
                        RegexOptions.IgnoreCase);
            //result = Regex.Replace(result,
            //         @"(<script>)([^(<script>\.</script>)])*(</script>)",
            //         string.Empty,
            //         RegexOptions.IgnoreCase);
            result = Regex.Replace(result,
                        @"(<script>).*(</script>)", string.Empty,
                        RegexOptions.IgnoreCase);

            // remove all styles (prepare first by clearing attributes)
            result = Regex.Replace(result,
                        @"<( )*style([^>])*>", "<style>",
                        RegexOptions.IgnoreCase);
            result = Regex.Replace(result,
                        @"(<( )*(/)( )*style( )*>)", "</style>",
                        RegexOptions.IgnoreCase);
            result = Regex.Replace(result,
                        "(<style>).*(</style>)", string.Empty,
                        RegexOptions.IgnoreCase);

            // insert tabs in spaces of <td> tags
            result = Regex.Replace(result,
                        @"<( )*td([^>])*>", "\t",
                        RegexOptions.IgnoreCase);

            // insert line breaks in places of <BR> and <LI> tags
            result = Regex.Replace(result,
                        @"<( )*br( )*>", "\r",
                        RegexOptions.IgnoreCase);
            result = Regex.Replace(result,
                        @"<( )*li( )*>", "\r",
                        RegexOptions.IgnoreCase);

            // insert line paragraphs (double line breaks) in place
            // if <P>, <DIV> and <TR> tags
            result = Regex.Replace(result,
                        @"<( )*div([^>])*>", "\r\r",
                        RegexOptions.IgnoreCase);
            result = Regex.Replace(result,
                        @"<( )*tr([^>])*>", "\r\r",
                        RegexOptions.IgnoreCase);
            result = Regex.Replace(result,
                        @"<( )*p([^>])*>", "\r\r",
                        RegexOptions.IgnoreCase);

            // Remove remaining tags like <a>, links, images,
            // comments etc - anything that's enclosed inside < >
            result = Regex.Replace(result,
                        @"<[^>]*>", string.Empty,
                        RegexOptions.IgnoreCase);

            // replace special characters:
            result = Regex.Replace(result,
                        @" ", " ",
                        RegexOptions.IgnoreCase);

            result = Regex.Replace(result,
                        @"•", " * ",
                        RegexOptions.IgnoreCase);
            result = Regex.Replace(result,
                        @"‹", "<",
                        RegexOptions.IgnoreCase);
            result = Regex.Replace(result,
                        @"›", ">",
                        RegexOptions.IgnoreCase);
            result = Regex.Replace(result,
                        @"™", "(tm)",
                        RegexOptions.IgnoreCase);
            result = Regex.Replace(result,
                        @"⁄", "/",
                        RegexOptions.IgnoreCase);
            result = Regex.Replace(result,
                        @"<", "<",
                        RegexOptions.IgnoreCase);
            result = Regex.Replace(result,
                        @">", ">",
                        RegexOptions.IgnoreCase);
            result = Regex.Replace(result,
                        @"©", "(c)",
                        RegexOptions.IgnoreCase);
            result = Regex.Replace(result,
                        @"®", "(r)",
                        RegexOptions.IgnoreCase);
            // Remove all others. More can be added, see
            // http://hotwired.lycos.com/webmonkey/reference/special_characters/
            result = Regex.Replace(result,
                        @"&(.{2,6});", string.Empty,
                        RegexOptions.IgnoreCase);

            // for testing
            //Regex.Replace(result,
            //       this.txtRegex.Text,string.Empty,
            //       RegexOptions.IgnoreCase);

            // Remove extra line breaks and tabs:
            // replace over 2 breaks with 2 and over 4 tabs with 4.
            // Prepare first to remove any whitespaces in between
            // the escaped characters and remove redundant tabs in between line breaks
            result = Regex.Replace(result,
                        "(\r)( )+(\r)", "\r\r",
                        RegexOptions.IgnoreCase);
            result = Regex.Replace(result,
                        "(\t)( )+(\t)", "\t\t",
                        RegexOptions.IgnoreCase);
            result = Regex.Replace(result,
                        "(\t)( )+(\r)", "\t\r",
                        RegexOptions.IgnoreCase);
            result = Regex.Replace(result,
                        "(\r)( )+(\t)", "\r\t",
                        RegexOptions.IgnoreCase);
            // Remove redundant tabs
            result = Regex.Replace(result,
                        "(\r)(\t)+(\r)", "\r\r",
                        RegexOptions.IgnoreCase);
            // Remove multiple tabs following a line break with just one tab
            result = Regex.Replace(result,
                        "(\r)(\t)+", "\r\t",
                        RegexOptions.IgnoreCase);
            // Initial replacement target string for line breaks
            string breaks = "\r\r\r";
            // Initial replacement target string for tabs
            string tabs = "\t\t\t\t\t";
            for (int index = 0; index < result.Length; index++)
            {
                result = result.Replace(breaks, "\r\r");
                result = result.Replace(tabs, "\t\t\t\t");
                breaks = breaks + "\r";
                tabs = tabs + "\t";
            }

            // That's it.
            return result;
        }
    }
}
