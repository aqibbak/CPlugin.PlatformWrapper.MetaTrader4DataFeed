using System.Runtime.InteropServices;

namespace CPlugin.PlatformWrapper.MetaTrader4DataFeed
{
    internal struct FeedDescription
    {
        public int Version;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string Name;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string Copyright;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string Web;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string Email;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string Server;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string Username;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string Userpass;

        public int Modes;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 512)]
        public string Description;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 62)]
        public int[] Reserved;
    }
}