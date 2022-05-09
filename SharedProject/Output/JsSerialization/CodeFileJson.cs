using Palmmedia.ReportGenerator.Core.Parser.Analysis;

namespace FineCodeCoverage.Output.JsSerialization
{
    public class CodeFileJson
    {
        public string path { get; set; }
        public CodeFileJson(CodeFile file)
        {
            path = file.Path;
        }
    }

}
