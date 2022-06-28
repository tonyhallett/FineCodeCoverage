using ReflectObject;
using System.Reflection;

namespace FineCodeCoverage.Impl
{
    public class Container : ReflectObjectProperties
    {
        public Container(object toReflect) : base(toReflect) { }
        public Container() { } // tests

        [ReflectFlags(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)]
        public string ProjectName { get; set; }

        [ReflectFlags(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)]
        public string Source { get; set; }

        [ReflectFlags(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)]
        public object TargetPlatform { get; set; }

        [ReflectFlags(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)]
        public object TargetFramework { get; set; }

        [ReflectFlags(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)]
        public ContainerData ProjectData { get; set; }
    }
}
