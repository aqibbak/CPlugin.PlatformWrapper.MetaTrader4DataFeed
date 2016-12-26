using System;
using System.ComponentModel;
using System.Diagnostics;

namespace CPlugin.PlatformWrapper.MetaTrader4DataFeed.QuoteSources
{
    /// <summary>
    /// 
    /// </summary>
    public class ReceiveQuoteEventArgs : EventArgs
    {
        
        
        private readonly Quote _quote;

        /// <summary>
        /// 
        /// </summary>
        public ReceiveQuoteEventArgs(Quote quote)
        {
            _quote = quote;
        }

        /// <summary>
        /// 
        /// </summary>
        public Quote Quote => _quote;
    }
}