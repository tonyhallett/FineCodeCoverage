namespace FineCodeCoverage.Output.JsMessages.Logging
{
    public class FCCLink : LogMessagePart
    {
        public FCCLink()
        {
            this.type = "fcclink";
        }

        public string hostObject { get; set; }
        public string methodName { get; set; }
        public object[] arguments { get; set; }
        public string title { get; set; }
    }
}
