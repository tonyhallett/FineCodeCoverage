using ReflectObject;

namespace FineCodeCoverage.Impl
{
    public class Container : ReflectObjectProperties
    {
        public Container(object toReflect) : base(toReflect) { }
        public Container() { } // tests
        public string ProjectName { get; set; }
        public string Source { get; set; }
        public object TargetPlatform { get; set; }

        // this is a public enum FrameworkVersion
        //[ReflectFlags(BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.NonPublic)]
        public object TargetFramework { get; set; }
        public ContainerData ProjectData { get; set; }
    }
}
