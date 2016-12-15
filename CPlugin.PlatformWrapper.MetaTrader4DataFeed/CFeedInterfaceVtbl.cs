using System;

namespace CPlugin.PlatformWrapper.MetaTrader4DataFeed
{
    internal struct CFeedInterfaceVtbl
    {
        public IntPtr Connect;
        public IntPtr Close;
        public IntPtr SetSymbols;
        public IntPtr Read;
        public IntPtr Journal;
    }
}