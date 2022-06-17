using Newtonsoft.Json;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;

namespace FineCodeCoverage.Output.JsSerialization.ReportGenerator
{
    public class CodeFileJson {

#pragma warning disable IDE1006 // Naming Styles
        public string path { get; set; }
#pragma warning restore IDE1006 // Naming Styles
        public CodeFileJson(CodeFile file)
        {
            path = file.Path;
        }

        [JsonConstructor]
        public CodeFileJson() { }
    }

}
