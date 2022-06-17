using Newtonsoft.Json;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using System.Collections.Generic;
using System.Linq;

namespace FineCodeCoverage.Output.JsSerialization.ReportGenerator
{
	public class AssemblyJson
	{
		[JsonIgnore]
		public Assembly Assembly { get; }
#pragma warning disable IDE1006 // Naming Styles
		[JsonIgnore]
		public int index { get; }
		public string name { get; }
		
		public string shortName { get; }

        public List<ClassJson> classes { get; }
#pragma warning restore IDE1006 // Naming Styles

        public AssemblyJson(Assembly assembly, int index)
		{
			Assembly = assembly;
			name = assembly.Name;
			shortName = assembly.ShortName;
			classes = assembly.Classes.Select((@class, classIndex) => new ClassJson(@class, classIndex,index)).ToList();
			this.index = index;
		}

		[JsonConstructor]
		public AssemblyJson(
			string name,
			string shortName,
			List<ClassJson> classes
        )
        {
			this.name = name;
			this.shortName = shortName;
			this.classes = classes;
        }
	}

}
