using Palmmedia.ReportGenerator.Core.Parser.Analysis;

namespace FineCodeCoverage.Output.JsSerialization.ReportGenerator
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
