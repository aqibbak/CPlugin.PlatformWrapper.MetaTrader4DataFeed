using System;
using System.Runtime.InteropServices;

namespace CPlugin.PlatformWrapper.MetaTrader4DataFeed.Delegates
{
    [UnmanagedFunctionPointer(CallingConvention.ThisCall, CharSet = CharSet.Ansi)]
    internal delegate int ReadDelegate(IntPtr ths, ref FeedData data);
}