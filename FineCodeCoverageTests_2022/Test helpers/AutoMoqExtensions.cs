namespace FineCodeCoverageTests.Test_helpers
{
    using System.Linq;
    using AutoMoq;

    internal static class AutoMoqExtensions
    {
        public static void SetEmptyEnumerable<T>(this AutoMoqer autoMoqer) =>
            autoMoqer.SetInstance(Enumerable.Empty<T>());
    }
}
