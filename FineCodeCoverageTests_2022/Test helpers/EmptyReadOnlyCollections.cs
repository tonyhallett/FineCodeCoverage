namespace FineCodeCoverageTests
{
    using System.Collections.ObjectModel;
    using System.Linq;

    internal static class EmptyReadOnlyCollection
    {
        public static ReadOnlyCollection<T> Of<T>() => new ReadOnlyCollection<T>(Enumerable.Empty<T>().ToList());
    }
}
