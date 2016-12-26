using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;

namespace CPlugin.PlatformWrapper.MetaTrader4DataFeed.QuoteSources
{
    /// <summary>
    /// 
    /// </summary>
    public class FeederStatistics
    {
        
        
        private readonly object _lockObject = new object();

        
        
        private long _errorsConnections;

        
        
        private long _helperRestarts;

        
        
        private DateTime _lastQuoteTime;

        
        
        private long _quotesReceived;

        
        
        private long _quotesReceivedTotal;

        
        
        private long _receivedErrors;

        
        
        private long _totalConnections;

        /// <summary>
        /// 
        /// </summary>
        public long TotalConnections
        {
            get { return _totalConnections; }
            set { _totalConnections = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public long ErrorsConnections
        {
            get { return _errorsConnections; }
            set { _errorsConnections = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public long QuotesReceived
        {
            get { return _quotesReceived; }
            set { _quotesReceived = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public long QuotesReceivedTotal
        {
            get { return _quotesReceivedTotal; }
            set { _quotesReceivedTotal = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public long ReceivedErrors
        {
            get { return _receivedErrors; }
            set { _receivedErrors = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public long HelperRestarts
        {
            get { return _helperRestarts; }
            set { _helperRestarts = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public DateTime LastQuoteTime
        {
            get { return _lastQuoteTime; }
            set { _lastQuoteTime = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public FeederStatistics Clone()
        {
            return (FeederStatistics) MemberwiseClone();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Reset()
        {
            _quotesReceived = 0L;
            _quotesReceivedTotal = 0L;
            _totalConnections = 0L;
            _errorsConnections = 0L;
            _receivedErrors = 0L;
            _helperRestarts = 0L;
            _lastQuoteTime = DateTime.MinValue;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Lock()
        {
            Monitor.Enter(_lockObject);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Unlock()
        {
            Monitor.Exit(_lockObject);
        }
    }
}