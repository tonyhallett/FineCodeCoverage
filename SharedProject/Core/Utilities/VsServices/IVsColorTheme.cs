using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Microsoft.Internal.VisualStudio.Shell.Interop
{
    [Guid("413D8344-C0DB-4949-9DBC-69C12BADB6AC")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    public interface IVsColorTheme
    {
        [MethodImpl(MethodImplOptions.InternalCall)]
        void Apply();

        [DispId(0)]
        IVsColorEntry this[[ComAliasName("Microsoft.Internal.VisualStudio.Shell.Interop.ColorName"), In] ColorName Name] { [MethodImpl(MethodImplOptions.InternalCall)][return: MarshalAs(UnmanagedType.Interface)] get; }

        Guid ThemeId { [MethodImpl(MethodImplOptions.InternalCall)] get; }

        string Name { [MethodImpl(MethodImplOptions.InternalCall)][return: MarshalAs(UnmanagedType.BStr)] get; }

        bool IsUserVisible { [MethodImpl(MethodImplOptions.InternalCall)] get; }
    }
}
