using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Output.HostObjects;

namespace FineCodeCoverage.Output.JsMessages.Logging
{
    internal static class ToolWindowLogExtensions
    {
        public static void SimpleLogToolWindow(this IEventAggregator eventAggregator, string message, MessageContext messageContext = MessageContext.Info)
        {
            eventAggregator.SendMessage(LogMessage.Simple(messageContext, message));
        }

        public static void LogToolWindowFailure(this IEventAggregator eventAggregator, string message)
        {
            eventAggregator.LogToolWindowLinkFCCOutputPane(message, MessageContext.Error);
        }

        public static void LogToolWindowLinkFCCOutputPane(this IEventAggregator eventAggregator, string message, MessageContext messageContext)
        {
            var logMessage = new LogMessage
            {
                context = messageContext,
                message = new LogMessagePart[] {
                    new Emphasized(message),
                    new FCCLink{
                        hostObject = FCCOutputPaneHostObjectRegistration.HostObjectName,
                        methodName = nameof(FCCOutputPaneHostObject.show),
                        title = "FCC Output Pane",
                        ariaLabel = "Open FCC Output Pane"
                    }
                }
            };
            eventAggregator.SendMessage(logMessage);
        }
    }

}
