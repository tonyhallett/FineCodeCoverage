using Newtonsoft.Json;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using System.Collections.Generic;
using System.Linq;

namespace FineCodeCoverage.Output.JsSerialization
{
	public class AssemblyJson
	{
		[JsonIgnore]
		public int index { get; }

		[JsonIgnore]
		public Assembly Assembly { get; }

		public string name { get; }
		
		public string shortName { get; }
		
		public List<ClassJson> classes { get; }

		public AssemblyJson(Assembly assembly, int index)
		{
			Assembly = assembly;
			name = assembly.Name;
			shortName = assembly.ShortName;
			classes = assembly.Classes.Select((@class, classIndex) => new ClassJson(@class, classIndex)).ToList();
			this.index = index;
		}
	}

}
