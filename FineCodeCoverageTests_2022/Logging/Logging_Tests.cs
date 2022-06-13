namespace FineCodeCoverageTests.Logging_Tests
{
    using AutoMoq;
    using FineCodeCoverage.Core.Utilities;
    using NUnit.Framework;
    using FineCodeCoverage.Logging.OutputPane;
    using FineCodeCoverage.Impl;
    using FineCodeCoverage.Logging;
    using Moq;
    using System.Collections.Generic;
    using System;

    internal class Logger_Tests
    {
        private AutoMoqer mocker;
        private FCCOutputPaneLogger logger;

        private class NullToString
        {
            public override string ToString() => null;
        }

        private class MsgObject
        {
            private readonly string msg;

            public MsgObject(string msg) => this.msg = msg;
            public override string ToString() => this.msg;
        }

        [SetUp]
        public void SetUp()
        {
            this.mocker = new AutoMoqer();
            this.logger = this.mocker.Create<FCCOutputPaneLogger>();
        }

        [Test]
        public void Should_Listen_For_ShowFCCOutputPaneMessage() =>
            this.mocker.Verify<IEventAggregator>(
                eventAggregator => eventAggregator.AddListener(this.logger, null)
            );

        [Test]
        public void Should_Activate_The_FCC_Output_Pane_When_Receives_ShowFCCOutputPaneMessage()
        {
            this.logger.Handle(new ShowFCCOutputPaneMessage());

            this.mocker.Verify<IFCCOutputPane>(fccOutputPane => fccOutputPane.ActivateAsync());
        }

        [Test]
        public void Should_Log_With_FCC_Title_On_New_Line_By_Default_Enumerable()
        {
            this.logger.Log(new List<string> { "Message" });

            var expectedMessage = $"{Environment.NewLine}Fine Code Coverage : Message{Environment.NewLine}";
            this.VerifyLogsMsgToFccOutputWindow(expectedMessage);
        }

        [Test]
        public void Should_Log_With_FCC_Title_On_New_Line_By_Default_String_Params()
        {
            (this.logger as ILogger).Log("Message");

            var expectedMessage = $"{Environment.NewLine}Fine Code Coverage : Message{Environment.NewLine}";
            this.VerifyLogsMsgToFccOutputWindow(expectedMessage);
        }

        [Test]
        public void Should_Log_With_FCC_Title_On_New_Line_By_Default_Object_Params()
        {
            this.logger.Log("Message");

            var expectedMessage = $"{Environment.NewLine}Fine Code Coverage : Message{Environment.NewLine}";
            this.VerifyLogsMsgToFccOutputWindow(expectedMessage);
        }

        [Test]
        public void Should_Log_With_FCC_Title_On_New_Line_By_Default_Enumerable_Of_Object()
        {
            this.logger.Log(new List<object> { "Message" });

            var expectedMessage = $"{Environment.NewLine}Fine Code Coverage : Message{Environment.NewLine}";
            this.VerifyLogsMsgToFccOutputWindow(expectedMessage);
        }

        [Test]
        public void Should_Log_Without_Title_Ending_With_New_Line_Object_Params()
        {
            this.logger.LogWithoutTitle(new MsgObject("Message"));

            var expectedMessage = $"Message{Environment.NewLine}";
            this.VerifyLogsMsgToFccOutputWindow(expectedMessage);
        }

        [Test]
        public void Should_Log_Without_Title_Ending_With_New_Line_Enumerable_Of_Object()
        {
            this.logger.LogWithoutTitle(new List<object> { new MsgObject("Message") });

            var expectedMessage = $"Message{Environment.NewLine}";
            this.VerifyLogsMsgToFccOutputWindow(expectedMessage);
        }

        [Test]
        public void Should_Log_Without_Title_Ending_With_New_Line_Enumerable_Of_Sring()
        {
            this.logger.LogWithoutTitle(new List<string> { "Message" });

            var expectedMessage = $"Message{Environment.NewLine}";
            this.VerifyLogsMsgToFccOutputWindow(expectedMessage);
        }

        [Test]
        public void Should_Log_Multiple_Messages_On_New_Line()
        {
            this.logger.LogWithoutTitle("Message1", "Message2");

            var expectedMessage = $"Message1{Environment.NewLine}Message2{Environment.NewLine}";
            this.VerifyLogsMsgToFccOutputWindow(expectedMessage);
        }

        [Test]
        public void Should_Log_With_Trimmed_Whitespace()
        {
            this.logger.Log(new List<string> { " Message " });

            var expectedMessage = $"{Environment.NewLine}Fine Code Coverage : Message{Environment.NewLine}";
            this.VerifyLogsMsgToFccOutputWindow(expectedMessage);
        }

        [Test]
        public void Should_Log_With_Trimmed_New_Lines()
        {
            this.logger.Log(new List<string> { "Message\r\n" });

            var expectedMessage = $"{Environment.NewLine}Fine Code Coverage : Message{Environment.NewLine}";
            this.VerifyLogsMsgToFccOutputWindow(expectedMessage);
        }

        [Test]
        public void Should_Not_Log_FCCOutputWindow_When_Just_Null_Messages()
        {
            this.logger.Log(new List<string> { null });

            this.VerifyDoesNotLogToFccOutputWindow();
        }

        [Test]
        public void Should_Not_Log_FCCOutputWindow_When_Just_Whitespace_Messages()
        {
            this.logger.Log(new List<string> { "   " });

            this.VerifyDoesNotLogToFccOutputWindow();
        }

        [Test]
        public void Should_Not_Log_FCCOutputWindow_When_Just_Object_ToString_Null_Messages()
        {
            this.logger.Log(new NullToString());

            this.VerifyDoesNotLogToFccOutputWindow();
        }

        private void VerifyLogsMsgToFccOutputWindow(string message) =>
            this.mocker.Verify<IFCCOutputPane>(
                fccOutputPane => fccOutputPane.OutputString(message)
            );

        private void VerifyDoesNotLogToFccOutputWindow() =>
            this.mocker.Verify<IFCCOutputPane>(
                fccOutputPane => fccOutputPane.OutputString(It.IsAny<string>()),
                Times.Never()
            );

    }
}
