using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace CPlugin.PlatformWrapper.MetaTrader4DataFeed.Delegates
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    internal delegate void CloseDelegate(IntPtr ths);
}