using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

//using System.Reflection.Metadata;

#if NET451
using Microsoft.Win32.SafeHandles;
#endif

namespace CPlugin.PlatformWrapper.MetaTrader4DataFeed
{
    internal sealed class SafeLibraryHandle
#if NET451
        : SafeHandleZeroOrMinusOneIsInvalid
#endif
    {
        private SafeLibraryHandle()
#if NET451
            : base(true)
#endif
        {
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr LoadLibrary(string path);


#if NET451
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected
            override
            bool ReleaseHandle()
        {
            return FreeLibrary(handle);
        }
#endif


        [EditorBrowsable(EditorBrowsableState.Never)]
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FreeLibrary(IntPtr module);
    }
}