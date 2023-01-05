using System;
using System.Runtime.InteropServices;

namespace Microsoft.Internal.VisualStudio.Shell.Interop
{
    public struct ColorName
    {
        public Guid Category;
        [MarshalAs(UnmanagedType.BStr)]
        public string Name;
    }
}
