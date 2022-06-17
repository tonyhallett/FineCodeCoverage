namespace FineCodeCoverageTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using FineCodeCoverage.Core.Utilities;
    using FineCodeCoverage.Output.HostObjects;
    using FineCodeCoverage.Output.JsMessages;
    using FineCodeCoverage.Output.JsMessages.Logging;
    using Moq;
    using NUnit.Framework;

    internal static class EventAggregatorAssertions
    {
        private static IEnumerable<IInvocation> GetSendMessageInvocations(Mock<IEventAggregator> mockEventAggregator) =>
            mockEventAggregator.Invocations.Where(i => i.Method.Name == nameof(IEventAggregator.SendMessage));

        public static IEnumerable<TMessage> GetSendMessageMessages<TMessage>(this Mock<IEventAggregator> mockEventAggregator) =>
            GetSendMessageInvocations(mockEventAggregator)
                .Where(invocation => invocation.Arguments[0] is TMessage)
                .Select(invocation => (TMessage)invocation.Arguments[0]);

        private static IEnumerable<LogMessage> GetSendMessageLogMessages(this Mock<IEventAggregator> mockEventAggregator) =>
            mockEventAggregator.GetSendMessageMessages<LogMessage>();

        private static bool HasLogMessage(this Mock<IEventAggregator> mockEventAggregator, Func<LogMessage, bool> predicate)
        {
            var matchingLogMessage = GetSendMessageLogMessages(mockEventAggregator).FirstOrDefault(predicate);
            return matchingLogMessage != null;
        }

        public static void AssertLogMessage(this Mock<IEventAggregator> mockEventAggregator, Func<LogMessage, bool> predicate) =>
            Assert.That(mockEventAggregator.HasLogMessage(predicate), Is.True, "No matching SendMessage<LogMessage>");

        public static void AssertHasSimpleLogMessage(
            this Mock<IEventAggregator> mockEventAggregator,
            bool expected,
            string expectedMessage,
            MessageContext messageContext = MessageContext.Info)
        {
            var hasLogMessage = HasLogMessage(mockEventAggregator, logMessage =>
            {
                var match = false;
                if (logMessage.context == messageContext && logMessage.message.Length == 1)
                {
                    var message = logMessage.message[0];
                    if (message is Emphasized emphasized)
                    {
                        match = emphasized.emphasis == Emphasis.None && emphasized.message == expectedMessage;
                    }
                }
                return match;

            });
            Assert.That(hasLogMessage, Is.EqualTo(expected));
        }

        public static void AssertSimpleSingleLog(
            this Mock<IEventAggregator> mockEventAggregator,
            string expectedMessage,
            MessageContext messageContext = MessageContext.Info
        ) => mockEventAggregator.AssertHasSimpleLogMessage(true, expectedMessage, messageContext);

        public static void AssertLogToolWindowLinkShowFCCOutputPane(this Mock<IEventAggregator> mockEventAggregator, string message, MessageContext messageContext) =>
            AssertLogMessage(mockEventAggregator, logMessage =>
            {
                var match = false;
                if (
                    messageContext == logMessage.context &&
                    logMessage.message.Length == 2 &&
                    logMessage.message[0] is Emphasized emphasized &&
                    emphasized.message == message &&
                    logMessage.message[1] is FCCLink fccLink &&
                    fccLink.hostObject == FCCOutputPaneHostObjectRegistration.HostObjectName &&
                    fccLink.methodName == nameof(FCCOutputPaneHostObject.show) &&
                    fccLink.title == "FCC Output Pane" &&
                    fccLink.ariaLabel == "Open FCC Output Pane"
                )
                {
                    match = true;
                }
                return match;
            });

        public static void AssertCoverageStopped(this Mock<IEventAggregator> mockEventAggregator, Times times) =>
            mockEventAggregator.Verify(eventAggregator => eventAggregator.SendMessage(It.IsAny<CoverageStoppedMessage>(), null), times);
    }
}
