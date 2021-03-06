﻿using System;
using System.ComponentModel;
using System.Diagnostics;

namespace CPlugin.PlatformWrapper.MetaTrader4DataFeed
{
    /// <summary>
    /// </summary>
    public class Quote
    {
        
        
        private readonly double _originAsk;

        
        
        private readonly double _originBid;

        
        
        private double _ask;

        
        
        private double _askSize;

        
        
        private string _bank;

        
        
        private double _bid;

        
        
        private double _bidSize;

        
        
        private object _context;

        
        
        private bool _isIndicative;

        
        
        private double _lastTrade;

        
        
        private int _mark;

        
        
        private string _quoteId;

        
        
        private DateTime _receivedTime = DateTime.Now;

        
        
        private string _source;

        
        
        private DateTime _sourceTime = DateTime.MinValue;

        
        
        private string _symbol;

        
        
        private int _volume;

        /// <summary>
        /// 
        /// </summary>
        public Quote()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public Quote(string symbol, double bid, double ask)
        {
            _bid = bid;
            _ask = ask;
            _symbol = symbol;
            _originBid = bid;
            _originAsk = ask;
        }

        /// <summary>
        /// 
        /// </summary>
        public Quote(string source, string bank, string symbol, double bid, double ask, int volume)
        {
            _bid = bid;
            _ask = ask;
            _bank = bank;
            _symbol = symbol;
            _source = source;
            _volume = volume;
            _originBid = bid;
            _originAsk = ask;
        }

        /// <summary>
        /// 
        /// </summary>
        public string Symbol
        {
            get { return _symbol; }
            set { _symbol = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public string Source
        {
            get { return _source; }
            set { _source = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public string Bank
        {
            get { return _bank; }
            set { _bank = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public int Mark
        {
            get { return _mark; }
            set { _mark = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public int Volume
        {
            get { return _volume; }
            set { _volume = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public double Bid
        {
            get { return _bid; }
            set { _bid = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public double Ask
        {
            get { return _ask; }
            set { _ask = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public double LastTrade
        {
            get { return _lastTrade; }
            set { _lastTrade = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public double OriginBid => _originBid;

        /// <summary>
        /// 
        /// </summary>
        public double OriginAsk => _originAsk;

        /// <summary>
        /// 
        /// </summary>
        public DateTime SourceTime
        {
            get { return _sourceTime; }
            set { _sourceTime = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public DateTime ReceivedTime
        {
            get { return _receivedTime; }
            set { _receivedTime = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public object Context
        {
            get { return _context; }
            set { _context = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public string QuoteId
        {
            get { return _quoteId; }
            set { _quoteId = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public double BidSize
        {
            get { return _bidSize; }
            set { _bidSize = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public double AskSize
        {
            get { return _askSize; }
            set { _askSize = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsIndicative
        {
            get { return _isIndicative; }
            set { _isIndicative = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public Quote Clone()
        {
            return MemberwiseClone() as Quote;
        }
    }
}