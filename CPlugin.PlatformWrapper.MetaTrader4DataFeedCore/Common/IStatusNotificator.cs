using System;

namespace CPlugin.PlatformWrapper.MetaTrader4DataFeed.Common
{
    /// <summary>
    /// 
    /// </summary>
    public interface IStatusNotificator
    {
        /// <summary>
        /// 
        /// </summary>
        event EventHandler<StatusMessageEventArgs> ChangeStatus;
    }
}