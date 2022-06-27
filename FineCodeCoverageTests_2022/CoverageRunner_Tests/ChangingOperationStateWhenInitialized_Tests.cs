namespace FineCodeCoverageTests.CoverageRunner_Tests
{
    using Microsoft.VisualStudio.TestWindow.Extensibility;
    using System.Collections.Generic;
    using AutoMoq;
    using FineCodeCoverage.Impl;
    using NUnit.Framework;
    using Moq;
    using FineCodeCoverage.Core.Initialization;

    internal class ChangingOperationStateWhenInitialized_Tests
    {
        private class OperationStateChangeHandler : IOperationStateChangedHandler
        {
            public List<OperationStateChangedEventArgs> HandledEventsArgs { get; } = new List<OperationStateChangedEventArgs>();
            public void Initialize(IChangingOperationState changingOperationState) =>
                changingOperationState.OperationStateChanged += this.ChangingOperationState_OperationStateChanged;

            private void ChangingOperationState_OperationStateChanged(object sender, OperationStateChangedEventArgs e) =>
                this.HandledEventsArgs.Add(e);
        }

        private AutoMoqer mocker;
        private OperationStateChangeHandler operationStateChangeHandler;

        [SetUp]
        public void SetUp()
        {
            this.mocker = new AutoMoqer();
            this.operationStateChangeHandler = new OperationStateChangeHandler();
            this.mocker.SetInstance<IOperationStateChangedHandler>(this.operationStateChangeHandler);

            _ = this.mocker.Create<ChangingOperationStateWhenInitialized>();
        }

        private void SetUpInitializeStatus(bool initialized) =>
            _ = this.mocker.GetMock<IInitializeStatusProvider>()
                .SetupGet(initilizeStatusProvider => initilizeStatusProvider.InitializeStatus)
                .Returns(initialized ? InitializeStatus.Initialized : InitializeStatus.Initializing);

        [Test]
        public void Should_Only_Raise_The_Event_If_Initialized_When_TestExecutionStarting()
        {
            this.SetUpInitializeStatus(false);
            _ = this.RaiseOperationStateChangedEvent(TestOperationStates.TestExecutionStarting);
            this.SetUpInitializeStatus(true);
            _ = this.RaiseOperationStateChangedEvent(TestOperationStates.TestExecutionFinished);

            Assert.That(this.operationStateChangeHandler.HandledEventsArgs, Is.Empty);

            var args = this.RaiseOperationStateChangedEvent(TestOperationStates.TestExecutionStarting);
            Assert.Multiple(() =>
            {
                Assert.That(this.operationStateChangeHandler.HandledEventsArgs, Has.Count.EqualTo(1));
                Assert.That(this.operationStateChangeHandler.HandledEventsArgs[0], Is.SameAs(args));
            });
        }

        private OperationStateChangedEventArgs RaiseOperationStateChangedEvent(TestOperationStates kind)
        {
            var args = new OperationStateChangedEventArgs(kind);
            this.mocker.GetMock<IOperationState>()
                .Raise(operationState => operationState.StateChanged += null, null, args);
            return args;
        }
    }
}
