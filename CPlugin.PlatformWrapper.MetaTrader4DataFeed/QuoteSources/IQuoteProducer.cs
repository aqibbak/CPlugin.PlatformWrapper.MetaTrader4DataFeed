using System;

namespace CPlugin.PlatformWrapper.MetaTrader4DataFeed.QuoteSources
{
    /// <summary>
    /// 
    /// </summary>
    public interface IQuoteProducer
    {
        /// <summary>
        /// 
        /// </summary>
        event EventHandler<ReceiveQuoteEventArgs> QuoteReceived;
    }
}