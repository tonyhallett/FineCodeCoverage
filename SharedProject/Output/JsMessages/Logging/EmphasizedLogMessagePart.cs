namespace FineCodeCoverage.Output.JsMessages.Logging
{
    public class Emphasized : LogMessagePart
    {
        public Emphasized(string message, Emphasis emphasis = Emphasis.None) : base()
        {
            this.emphasis = emphasis;
            this.message = message;
            this.type = "emphasized";
        }
        public string message { get; set; }
        public Emphasis emphasis { get; set; }
    }
}
