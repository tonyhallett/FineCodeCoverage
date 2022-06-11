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
        public TestRunResponse Response { get; set; }

        public long TotalTests { get; set; }

    }
}
