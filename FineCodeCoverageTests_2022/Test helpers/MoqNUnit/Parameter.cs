namespace FineCodeCoverageTests
{
    /// <summary>
    /// Helper class used to access the parameter being passed to the method.
    /// </summary>
    public static class Parameter
    {
        /// <summary>
        /// Returns an instance of <see cref="Parameter{T}" /> holding the information about the type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the parameter.</typeparam>
        /// <returns>An instance of <see cref="Parameter{T}" />.</returns>
        public static Parameter<T> Is<T>() => new Parameter<T>();
    }

    /// <summary>
    /// An holder class used to access the methods in <see cref="ParameterExtensions" />.
    /// </summary>
    /// <typeparam name="T">The type of the parameter.</typeparam>
    public sealed class Parameter<T>
    {
    }
}
