using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Microsoft.Internal.VisualStudio.Shell.Interop
{
    [Guid("EAB552CF-7858-4F05-8435-62DB6DF60684")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    public interface IVsColorThemeService
    {
        [MethodImpl(MethodImplOptions.InternalCall)]
        void NotifyExternalThemeChanged();

        [MethodImpl(MethodImplOptions.InternalCall)]
        [return: ComAliasName("VsShell.VS_RGBA")]
        uint GetCurrentVsColorValue([In] int vsSysColor);

        [MethodImpl(MethodImplOptions.InternalCall)]
        [return: ComAliasName("VsShell.VS_RGBA")]
        uint GetCurrentColorValue([ComAliasName("OLE.REFGUID"), In] ref Guid rguidColorCategory, [ComAliasName("OLE.LPCOLESTR"), MarshalAs(UnmanagedType.LPWStr), In] string pszColorName, [ComAliasName("VsShell.THEMEDCOLORTYPE"), In] uint dwColorType);

        [MethodImpl(MethodImplOptions.InternalCall)]
        [return: ComAliasName("OLE.DWORD")]
        uint GetCurrentEncodedColor([ComAliasName("OLE.REFGUID"), In] ref Guid rguidColorCategory, [ComAliasName("OLE.LPCOLESTR"), MarshalAs(UnmanagedType.LPWStr), In] string pszColorName, [ComAliasName("VsShell.THEMEDCOLORTYPE"), In] uint dwColorType);

        IVsColorThemes Themes { [MethodImpl(MethodImplOptions.InternalCall)][return: MarshalAs(UnmanagedType.Interface)] get; }

        IVsColorNames ColorNames { [MethodImpl(MethodImplOptions.InternalCall)][return: MarshalAs(UnmanagedType.Interface)] get; }

        IVsColorTheme CurrentTheme { [MethodImpl(MethodImplOptions.InternalCall)][return: MarshalAs(UnmanagedType.Interface)] get; }
    }
}
