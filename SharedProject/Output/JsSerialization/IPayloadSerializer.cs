namespace FineCodeCoverage.Output.JsSerialization
{
    internal interface IPayloadSerializer
    {
        string Serialize<T>(string type, T data);
    }
}
