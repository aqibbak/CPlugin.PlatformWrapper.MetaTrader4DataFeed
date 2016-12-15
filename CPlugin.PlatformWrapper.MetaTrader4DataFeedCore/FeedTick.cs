using System.Runtime.InteropServices;

namespace CPlugin.PlatformWrapper.MetaTrader4DataFeed
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct FeedTick
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
        public string Symbol;

        public int Ctm;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string Bank;

        public double Bid;
        public double Ask;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 12)]
        public string Reserved;
    }
}