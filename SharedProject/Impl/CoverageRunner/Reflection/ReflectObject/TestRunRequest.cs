using ReflectObject;
using System.Reflection;

namespace FineCodeCoverage.Impl
{
    public class TestRunRequest : ReflectObjectProperties
    {
        public TestRunRequest(object toReflect) : base(toReflect) { }
        public TestRunRequest() { } // tests

        [ReflectFlags(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)]
        public TestConfiguration Configuration { get; set; }

        [ReflectFlags(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)]
        public TestRunResponse Response { get; set; }

        [ReflectFlags(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)]
        public long TotalTests { get; set; }

    }
}
