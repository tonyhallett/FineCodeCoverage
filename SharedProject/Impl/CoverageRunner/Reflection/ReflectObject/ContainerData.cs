using System;
using System.Reflection;
using System.Threading.Tasks;
using ReflectObject;

namespace FineCodeCoverage.Impl
{
    public class ContainerData : ReflectObjectProperties
    {
        public ContainerData(object toReflect) : base(toReflect) { }
        public ContainerData() { } // tests
        [ReflectFlags(BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.NonPublic)]
        public string ProjectFilePath { get; set; }
        [ReflectFlags(BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.NonPublic)]
        public Guid Id { get; set; }

        [ReflectFlags(BindingFlags.Instance | BindingFlags.NonPublic)]
        public Func<string, string, Task<string>> GetBuildPropertyAsync { get; protected set; }
    }
}
