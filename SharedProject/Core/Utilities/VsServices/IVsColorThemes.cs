using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
//using System.Runtime.InteropServices.CustomMarshalers;
//using System.Collections;

namespace Microsoft.Internal.VisualStudio.Shell.Interop
{
    [Guid("98192AFE-75B9-4347-82EC-FF312C1995D8")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    public interface IVsColorThemes
    {
        [MethodImpl(MethodImplOptions.InternalCall)]
        [return: MarshalAs(UnmanagedType.Interface)]
        IVsColorTheme GetThemeFromId([In] Guid ThemeId);

        [DispId(0)]
        IVsColorTheme this[[In] int index] { [MethodImpl(MethodImplOptions.InternalCall)][return: MarshalAs(UnmanagedType.Interface)] get; }

        int Count { [MethodImpl(MethodImplOptions.InternalCall)] get; }

        //[MethodImpl(MethodImplOptions.InternalCall)]
        //[return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(EnumeratorToEnumVariantMarshaler))]
        //IEnumerator GetEnumerator();
    }
}
