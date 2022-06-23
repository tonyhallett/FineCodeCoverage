using System.Collections.Generic;

namespace FineCodeCoverageWebViewReport
{
    public class NamedArguments
    {
        public const string ReportPathsDebug = "reportpathsdebug";
        public const string ReportPathsPath = "reportpathspath";

        public static string GetNamedArgument(string name, string value)
        {
            return $"--{name}={value}";
        }
        internal static Dictionary<string, string> Get(string[] arguments)
        {
            Dictionary<string, string> namedArguments = new Dictionary<string, string>();
            foreach (var argument in arguments)
            {
                var parts = argument.Substring(2).Split('=');
                namedArguments.Add(parts[0], parts[1]);

            }
            return namedArguments;
        }

    }
}
