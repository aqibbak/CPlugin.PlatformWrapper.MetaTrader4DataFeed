using System;
using CPlugin.PlatformWrapper.MetaTrader4DataFeed.Common;

namespace CPlugin.PlatformWrapper.MetaTrader4DataFeed.QuoteSources
{
    /// <summary>
    /// 
    /// </summary>
    public interface IFeeder : INamedObject, IRunnable, IQuoteProducer
    {
        /// <summary>
        /// 
        /// </summary>
        string VendorInfo { get; }

        /// <summary>
        /// 
        /// </summary>
        Version Version { get; }

        /// <summary>
        /// 
        /// </summary>
        FeederStatistics Statistics { get; }
    }
}