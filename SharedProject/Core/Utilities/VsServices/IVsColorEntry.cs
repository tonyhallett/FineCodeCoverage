using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Microsoft.Internal.VisualStudio.Shell.Interop
{
    public interface IVsColorEntry
    {
        [ComAliasName("Microsoft.Internal.VisualStudio.Shell.Interop.ColorName")]
        ColorName ColorName { [MethodImpl(MethodImplOptions.InternalCall)][return: ComAliasName("Microsoft.Internal.VisualStudio.Shell.Interop.ColorName")] get; }

        [ComAliasName("TextManager.BYTE")]
        byte BackgroundType { [MethodImpl(MethodImplOptions.InternalCall)][return: ComAliasName("TextManager.BYTE")] get; }

        [ComAliasName("TextManager.BYTE")]
        byte ForegroundType { [MethodImpl(MethodImplOptions.InternalCall)][return: ComAliasName("TextManager.BYTE")] get; }

        [ComAliasName("VsShell.VS_RGBA")]
        uint Background { [MethodImpl(MethodImplOptions.InternalCall)][return: ComAliasName("VsShell.VS_RGBA")] get; }

        [ComAliasName("VsShell.VS_RGBA")]
        uint Foreground { [MethodImpl(MethodImplOptions.InternalCall)][return: ComAliasName("VsShell.VS_RGBA")] get; }

        [ComAliasName("OLE.UINT")]
        uint BackgroundSource { [MethodImpl(MethodImplOptions.InternalCall)][return: ComAliasName("OLE.UINT")] get; }

        [ComAliasName("OLE.UINT")]
        uint ForegroundSource { [MethodImpl(MethodImplOptions.InternalCall)][return: ComAliasName("OLE.UINT")] get; }
    }
}
