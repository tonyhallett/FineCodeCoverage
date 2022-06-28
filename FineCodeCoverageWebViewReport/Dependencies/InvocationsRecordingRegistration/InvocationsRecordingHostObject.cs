using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace FineCodeCoverageWebViewReport.InvocationsRecordingRegistration
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public abstract class InvocationsRecordingHostObject
    {
        private readonly List<Invocation> invocations = new List<Invocation>();
        public void AddInvocation(string name, params object[] arguments)
        {
            invocations.Add(new Invocation { Name = name, Arguments = arguments.ToList() });
        }

#pragma warning disable IDE1006 // Naming Styles
        public string getInvocations()
#pragma warning restore IDE1006 // Naming Styles
        {
            return JsonConvert.SerializeObject(invocations);
        }
    }
}
