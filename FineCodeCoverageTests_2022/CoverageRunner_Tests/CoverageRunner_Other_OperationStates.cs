namespace FineCodeCoverageTests.CoverageRunner_Tests
{
    using Microsoft.VisualStudio.TestWindow.Extensibility;
    using NUnit.Framework;

    internal class CoverageRunner_Other_OperationStates : CoverageRunner_Tests_Base
    {
        [Test]
        public void Should_Not_Throw() => this.RaiseOperationStateChanged(TestOperationStates.ChangeDetection);
    }

}
