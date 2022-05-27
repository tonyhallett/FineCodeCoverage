namespace FineCodeCoverage.Output.JsMessages.Logging
{
    public class LogMessage
    {
        public MessageContext context { get; set; }
        public static LogMessage Simple(MessageContext messageContext, string message)
        {
            return new LogMessage
            {
                context = messageContext,
                message = new LogMessagePart[] { new Emphasized(message) }
            };
        }
        public LogMessagePart[] message { get; set; }
    }
}
