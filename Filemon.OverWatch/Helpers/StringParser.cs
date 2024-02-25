using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Filemon.OverWatch.Helpers
{
    public static partial class StringParser
    {
        public static (string, string) ParseString(string input)
        {
            string cleanInput = StringParser.cleanInput().Replace(input, "");
            string docType = StringParser.docType().Match(cleanInput).Groups[1].Value;
            string format = StringParser.format().Match(cleanInput).Groups[1].Value;

            return (docType, format);
        }

        [GeneratedRegex(@"\u001B\[\d+(;\d+)?m")]
        private static partial Regex cleanInput();
        [GeneratedRegex(@"[^:]+:\s*(.*?)\s*\(")]
        private static partial Regex docType();
        [GeneratedRegex(@"\((.*?)\)")]
        private static partial Regex format();
    }
}
