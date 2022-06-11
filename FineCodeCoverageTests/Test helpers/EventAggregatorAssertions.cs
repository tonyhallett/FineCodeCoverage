using Moq;
using System.Linq;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Output.JsMessages.Logging;
using NUnit.Framework;
using System.Collections.Generic;
using System;
using FineCodeCoverage.Output.HostObjects;
using FineCodeCoverage.Output.JsMessages;

namespace FineCodeCoverageTests
{
    internal static class EventAggregatorAssertions
    {
        private static IEnumerable<IInvocation> GetSendMessageInvocations(Mock<IEventAggregator> mockEventAggregator)
        {
            return mockEventAggregator.Invocations.Where(i => i.Method.Name == nameof(IEventAggregator.SendMessage));
        }

        private static IEnumerable<TMessage> GetSendMessageMessages<TMessage>(this Mock<IEventAggregator> mockEventAggregator)
        {
            return GetSendMessageInvocations(mockEventAggregator)
                .Where(invocation => invocation.Arguments[0] is TMessage)
                .Select(invocation => (TMessage)invocation.Arguments[0]);
        }

        private static IEnumerable<LogMessage> GetSendMessageLogMessages(this Mock<IEventAggregator> mockEventAggregator)
        {
            return mockEventAggregator.GetSendMessageMessages<LogMessage>();
        }

        private static bool HasLogMessage(this Mock<IEventAggregator> mockEventAggregator, Func<LogMessage, bool> predicate)
        {
            var matchingLogMessage = GetSendMessageLogMessages(mockEventAggregator).FirstOrDefault(predicate);
            return matchingLogMessage != null;
        }

        public static void AssertLogMessage(this Mock<IEventAggregator> mockEventAggregator,Func<LogMessage,bool> predicate)
        {
            Assert.True(mockEventAggregator.HasLogMessage(predicate), "No matching SendMessage<LogMessage>");
        }

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
            Assert.AreEqual(expected, hasLogMessage);
        }

        public static void AssertSimpleSingleLog(
            this Mock<IEventAggregator> mockEventAggregator, 
            string expectedMessage, 
            MessageContext messageContext = MessageContext.Info)
        {
            mockEventAggregator.AssertHasSimpleLogMessage(true, expectedMessage, messageContext);
        }

        public static void AssertLogToolWindowLinkShowFCCOutputPane(this Mock<IEventAggregator> mockEventAggregator, string message, MessageContext messageContext)
        {
            AssertLogMessage(mockEventAggregator, logMessage =>
            {
                var match = false;
                if (
                    messageContext == logMessage.context && 
                    logMessage.message.Length == 2 && 
                    logMessage.message[0] is Emphasized emphasized && 
                    emphasized.message == message &&
                    logMessage.message[1] is FCCLink fccLink &&
                    fccLink.hostObject == FCCOutputPaneRegistration.HostObjectName &&
                    fccLink.methodName == nameof(FCCOutputPaneHostObject.show) &&
                    fccLink.title == "FCC Output Pane"
                )
                {
                    match = true;
                }
                return match;
            });
        }

        public static void AssertCoverageStopped(this Mock<IEventAggregator> mockEventAggregator,Times times)
        {
            mockEventAggregator.Verify(eventAggregator => eventAggregator.SendMessage(It.IsAny<CoverageStoppedMessage>(), null), times);
        }
    }
}
