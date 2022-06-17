namespace FineCodeCoverage.Output.JsMessages.Logging
{
    public class FCCLink : LogMessagePart
    {
        public FCCLink()
        {
            this.type = "fcclink";
        }

#pragma warning disable IDE1006 // Naming Styles
        public string hostObject { get; set; }

        public string methodName { get; set; }
        public object[] arguments { get; set; }
        public string title { get; set; }
        public string ariaLabel { get; set; }
#pragma warning restore IDE1006 // Naming Styles
    }
}
