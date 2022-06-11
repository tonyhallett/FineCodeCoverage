namespace FineCodeCoverageTests_Process_Tests
{
    using System;
    using AutoMoq;
    using FineCodeCoverage.Core.Utilities;
    using NUnit.Framework;

    public class ProcessResponseProcessor_Tests
    {
        private AutoMoqer mocker;
        private ProcessResponseProcessor processor;
        private Action successCallback;
        private bool successCallbackCalled;

        [SetUp]
        public void SetUp()
        {
            this.mocker = new AutoMoqer();
            this.processor = this.mocker.Create<ProcessResponseProcessor>();
            this.successCallbackCalled = false;
            this.successCallback = () => this.successCallbackCalled = true;
        }

        [Test]
        public void Should_Throw_Exception_If_Non_Success_ExitCode_And_Throw_Error_True()
        {
            var executeResponse = new ExecuteResponse
            {
                ExitCode = 999,
                Output = "This will be exception message"
            };
            var callbackExitCode = 0;
            Assert.Multiple(() =>
            {
                _ = Assert.Throws<Exception>(
                    () => this.processor.Process(
                        executeResponse,
                        exitCode =>
                        {
                            callbackExitCode = exitCode;
                            return false;
                        },
                        true,
                        null,
                        null
                    ),
                    executeResponse.Output
                );
                Assert.That(callbackExitCode, Is.EqualTo(executeResponse.ExitCode));
            });
        }

        [Test]
        public void Should_Log_Response_Output_With_Error_Title_If_Non_Success_ExitCode_And_Throw_Error_False()
        {
            var executeResponse = new ExecuteResponse
            {
                ExitCode = 999,
                Output = "This will be logged"
            };
            Assert.Multiple(() =>
            {
                Assert.That(
                    this.processor.Process(executeResponse, exitCode => false, false, "title", this.successCallback),
                    Is.False
                );

                Assert.That(this.successCallbackCalled, Is.False);
            });
            this.mocker.Verify<ILogger>(l => l.Log("title Error", "This will be logged"));
        }

        [Test]
        public void Should_Log_Response_Output_With_Title_If_Success_ExitCode_And_Call_Callback()
        {
            var executeResponse = new ExecuteResponse
            {
                ExitCode = 0,
                Output = "This will be logged"
            };
            Assert.Multiple(() =>
            {
                Assert.That(
                    this.processor.Process(executeResponse, exitCode => true, true, "title", this.successCallback),
                    Is.True
                );

                Assert.That(this.successCallbackCalled, Is.True);
            });
            this.mocker.Verify<ILogger>(l => l.Log("title", "This will be logged"));
        }
    }
}
