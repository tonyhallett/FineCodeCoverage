using Newtonsoft.Json;

namespace FineCodeCoverage.Output.JsSerialization
{
    public class Payload<T>
    {
        public string type { get; set; }
        public T data { get; set; }
        public static string AsJson(string type, T data, JsonSerializerSettings settings = null)
        {
            return JsonConvert.SerializeObject(new Payload<T> { type = type, data = data }, settings);
        }
    }
}
