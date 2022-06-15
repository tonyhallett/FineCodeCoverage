namespace FineCodeCoverageTests
{
    using System;
    using NUnit.Framework.Constraints;

    /// <summary>
    /// Helper class exposing extension methods that can be used to set expectations on Moq parameters using NUnit constraints.
    /// </summary>
    public static class ParameterExtensions
    {
        /// <summary>
        /// Sets an expectation on the value of the parameter being used.
        /// </summary>
        /// <typeparam name="T">The type of the parameter.</typeparam>
        /// <param name="parameter">The parameter.</param>
        /// <param name="constraint">A NUnit constraint used to set the expectation.</param>
        /// <returns>The parameter value.</returns>
        public static T That<T>(this Parameter<T> parameter, IConstraint constraint)
        {
            _ = parameter ?? throw new ArgumentNullException(nameof(parameter));

            return Moq.Match.Create<T>(item => constraint.ApplyTo(item).IsSuccess);
        }

        /// <summary>
        /// Sets an expectation on a value produced off the provided parameter value.
        /// </summary>
        /// <typeparam name="T">The type of the parameter used to produce the test value.</typeparam>
        /// <typeparam name="TValue">The type of the value to test.</typeparam>
        /// <param name="parameter">The parameter.</param>
        /// <param name="valueSelector"></param>
        /// <param name="constraint">A NUnit constraint used to set the expectation.</param>
        /// <returns>The parameter value.</returns>
        public static T That<T, TValue>(this Parameter<T> parameter, Func<T, TValue> valueSelector, IConstraint constraint)
        {
            _ = parameter ?? throw new ArgumentNullException(nameof(parameter));

            return Moq.Match.Create<T>(item =>
            {
                var value = valueSelector(item);
                return constraint.ApplyTo(value).IsSuccess;
            });
        }
    }
}
