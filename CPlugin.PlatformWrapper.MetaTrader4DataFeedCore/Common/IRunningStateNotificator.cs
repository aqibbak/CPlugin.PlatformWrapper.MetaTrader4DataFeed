using System;

namespace CPlugin.PlatformWrapper.MetaTrader4DataFeed.Common
{
    /// <summary>
    /// 
    /// </summary>
    public interface IRunningStateNotificator
    {
        /// <summary>
        /// 
        /// </summary>
        event EventHandler Started;

        /// <summary>
        /// 
        /// </summary>
        event EventHandler Stopped;

        /// <summary>
        /// 
        /// </summary>
        event EventHandler Paused;
    }
}