using System;
using System.Runtime.InteropServices;

namespace CPlugin.PlatformWrapper.MetaTrader4DataFeed
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct FeedData
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2560)]
        public byte[] Ticks;

        public int TicksCount;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string NewsTime;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string Subject;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string Category;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string Keywords;

        public IntPtr Body;
        public int BodyLen;
        public int BodyMaxlen;
        public int Result;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string ResultString;

        public int Feeder;
        public int Mode;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 68)]
        public string Reserved;
    }
}