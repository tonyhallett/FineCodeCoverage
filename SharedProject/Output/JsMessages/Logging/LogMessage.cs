namespace FineCodeCoverage.Output.JsMessages.Logging
{
    public class LogMessage
    {
#pragma warning disable IDE1006 // Naming Styles
        public MessageContext context { get; set; }
        public LogMessagePart[] message { get; set; }
#pragma warning restore IDE1006 // Naming Styles
        public static LogMessage Simple(MessageContext messageContext, string message)
        {
            return new LogMessage
            {
                context = messageContext,
                message = new LogMessagePart[] { new Emphasized(message) }
            };
        }
    }
}
