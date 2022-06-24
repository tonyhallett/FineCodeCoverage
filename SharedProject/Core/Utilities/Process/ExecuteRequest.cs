using System;

namespace FineCodeCoverage.Core.Utilities
{
    internal class ExecuteRequest : IEquatable<ExecuteRequest>
    {
        public string FilePath { get; set; }
        public string Arguments { get; set; }
        public string WorkingDirectory { get; set; }

        public bool Equals(ExecuteRequest other)
        {
            return FilePath == other.FilePath && Arguments == other.Arguments && WorkingDirectory == other.WorkingDirectory;
        }
    }
}
