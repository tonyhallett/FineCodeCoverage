namespace FineCodeCoverageTests
{
    using Moq;
    using NUnit.Framework;

    internal static class MoqExtensions
    {
        public static void AssertReInvokes(this Mock mock)
        {
            var invocations = mock.Invocations;

            Assert.That(invocations.FirstArgument(0), Is.SameAs(invocations.FirstArgument(1)));
        }

        private static object FirstArgument(this IInvocationList invocations, int index) =>
            invocations[index].Arguments[0];
    }
}
