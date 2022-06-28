using System.Collections.Generic;
using System.Reflection;
using ReflectObject;

namespace FineCodeCoverage.Impl
{
    public class TestConfiguration : ReflectObjectProperties
    {
        public TestConfiguration(object toReflect) : base(toReflect) { }
        public TestConfiguration() { } // tests

        [ReflectFlags(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)]
        public object UserRunSettings { get; set; }

        [ReflectFlags(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)]
        public IEnumerable<Container> Containers { get; set; }

        [ReflectFlags(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)]
        public string SolutionDirectory { get; set; }
    }
}
