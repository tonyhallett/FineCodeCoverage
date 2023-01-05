using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
//using System.Runtime.InteropServices.CustomMarshalers;
//using System.Collections;

namespace Microsoft.Internal.VisualStudio.Shell.Interop
{
    public interface IVsColorNames
    {
        [MethodImpl(MethodImplOptions.InternalCall)]
        [return: ComAliasName("Microsoft.Internal.VisualStudio.Shell.Interop.ColorName")]
        ColorName GetNameFromVsColor([In] int vsSysColor);

        [ComAliasName("Microsoft.Internal.VisualStudio.Shell.Interop.ColorName")]
        [DispId(0)]
        ColorName this[[In] int index] { [MethodImpl(MethodImplOptions.InternalCall)][return: ComAliasName("Microsoft.Internal.VisualStudio.Shell.Interop.ColorName")] get; }

        int Count { [MethodImpl(MethodImplOptions.InternalCall)] get; }

        //[MethodImpl(MethodImplOptions.InternalCall)]
        //[return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(EnumeratorToEnumVariantMarshaler))]
        //IEnumerator GetEnumerator();
    }
}
