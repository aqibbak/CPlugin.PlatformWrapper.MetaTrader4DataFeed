using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace CPlugin.PlatformWrapper.MetaTrader4DataFeed.Delegates
{
    
    [UnmanagedFunctionPointer(CallingConvention.ThisCall, CharSet = CharSet.Ansi)]
    internal delegate int ConnectDelegate(IntPtr ths, string server, string login, string password);
}