using Newtonsoft.Json;
using System.ComponentModel.Composition;

namespace FineCodeCoverage.Output.JsSerialization
{
    [Export(typeof(IPayloadSerializer))]
    internal class PayloadSerializer : IPayloadSerializer
    {
        public string Serialize<T>(string type, T data)
        {
            return JsonConvert.SerializeObject(new Payload<T> { type = type, data = data });
        }
    }


}
