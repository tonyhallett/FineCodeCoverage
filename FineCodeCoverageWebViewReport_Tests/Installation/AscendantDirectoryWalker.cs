namespace FineCodeCoverageWebViewReport_Tests.Installation
{
    using System;
    using System.IO;

    internal static class AscendantDirectoryWalker
    {
        public static DirectoryInfo WalkAscendantsUntil(this DirectoryInfo start, Func<DirectoryInfo, bool> predicate)
        {
            var ascendant = start;
            while (!predicate(ascendant))
            {
                ascendant = ascendant.Parent;
                if (ascendant == null)
                {
                    break;
                }
            }
            return ascendant;
        }
    }
}

