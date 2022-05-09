using Newtonsoft.Json;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using System.Collections.Generic;
using System.Linq;

namespace FineCodeCoverage.Output.JsSerialization
{
    public class ClassJson
    {
#pragma warning disable IDE1006 // Naming Styles
        [JsonIgnore]
        public int index { get; }

        [JsonIgnore]
        public Class Class { get; }

        public List<CodeFileJson> files { get; }

        public string name { get; }
        
        public string displayName { get; }
#pragma warning restore IDE1006 // Naming Styles

        public ClassJson(Class @class, int index)
        {
            Class = @class;
            files = @class.Files.Select(file => new CodeFileJson(file)).ToList();
            name = @class.Name;
            displayName = @class.DisplayName;
            this.index = index;
        }
    }

}
