using System.Reflection;
using ReflectObject;

namespace FineCodeCoverage.Impl
{
    public class TestRunResponse : ReflectObjectProperties
    {
        public TestRunResponse(object toReflect) : base(toReflect) { }
        public TestRunResponse() { } // tests

        [ReflectFlags(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)]
        public long FailedTests { get; set; }

        [ReflectFlags(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)]
        public long PassedTests { get; set; }

        [ReflectFlags(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)]
        public long SkippedTests { get; set; }

        [ReflectFlags(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)]
        public long TotalTests { get; set; }

        [ReflectFlags(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)]
        public bool IsAborted { get; set; }

    }
}