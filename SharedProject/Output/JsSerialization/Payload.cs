namespace FineCodeCoverage.Output.JsSerialization
{
    public class Payload<T>
    {
#pragma warning disable IDE1006 // Naming Styles
        public string type { get; set; }

        public T data { get; set; }
#pragma warning restore IDE1006 // Naming Styles

    }
}
