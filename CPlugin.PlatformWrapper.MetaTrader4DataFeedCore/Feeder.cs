using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using CPlugin.PlatformWrapper.MetaTrader4DataFeed.Common;
using CPlugin.PlatformWrapper.MetaTrader4DataFeed.Delegates;
using CPlugin.PlatformWrapper.MetaTrader4DataFeed.QuoteSources;

//using System.Reflection.Metadata;

namespace CPlugin.PlatformWrapper.MetaTrader4DataFeed
{
    /// <summary>
    ///     .NET Wrapper over MetaQuotes MetaTrader4 DataFeed (for example, mt4feeder.feed)
    /// </summary>
    public class Feeder :
#if NET451
        MarshalByRefObject,
#endif
        IFeeder,
        IStatusNotificator,
        IRunningStateNotificator,
        ISymbolObserver,
        IDisposable
    {
        #region public fields

        /// <summary>
        /// Handle paused event here
        /// </summary>
        public event EventHandler Paused
        {
            add { PausedEvent += value; }
            remove
            {
                if (PausedEvent == null)
                    return;

                PausedEvent -= value;
            }
        }

        /// <summary>
        /// Handle started event here
        /// </summary>
        public event EventHandler Started
        {
            add { StartedEvent += value; }
            remove
            {
                if (StartedEvent == null)
                    return;

                StartedEvent -= value;
            }
        }

        /// <summary>
        /// Handle stopped event here
        /// </summary>
        public event EventHandler Stopped
        {
            add { StoppedEvent += value; }
            remove
            {
                if (StoppedEvent == null)
                    return;

                StoppedEvent -= value;
            }
        }

        /// <summary>
        /// When status changed you get this event
        /// </summary>
        public event EventHandler<StatusMessageEventArgs> ChangeStatus
        {
            add { StatusEvent += value; }
            remove
            {
                if (StatusEvent == null)
                    return;

                StatusEvent -= value;
            }
        }

        /// <summary>
        /// Handle quote received event here
        /// </summary>
        public event EventHandler<ReceiveQuoteEventArgs> QuoteReceived
        {
            add { ReceivedQuoteEvent += value; }
            remove
            {
                if (ReceivedQuoteEvent == null)
                    return;

                ReceivedQuoteEvent -= value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool CanResetSettings => false;

        /// <summary>
        /// 
        /// </summary>
        public object Settings => _settings;

        /// <summary>
        ///     Full path to 'mt4feeder.feed' file including its name
        /// </summary>
        public string Path
        {
            get { return _settings.Path; }
            set { _settings.Path = value; }
        }

        /// <summary>
        ///     MT4 server address
        /// </summary>
        public string Server
        {
            get { return _settings.Server; }
            set { _settings.Server = value; }
        }

        /// <summary>
        ///     Client account
        /// </summary>
        public int Login
        {
            get { return _settings.Login; }
            set { _settings.Login = value; }
        }

        /// <summary>
        ///     Client password
        /// </summary>
        public string Password
        {
            get { return _settings.Password; }
            set { _settings.Password = value; }
        }

        /// <summary>
        /// Idle timeout before reconnect, in sec.
        /// </summary>
        public int ReconnectRetryTimeout
        {
            get { return _settings.ReconnectRetryTimeout; }
            set { _settings.ReconnectRetryTimeout = value; }
        }

        /// <summary>
        /// Sleep after several failed attempts, in sec.
        /// </summary>
        public int ReconnectTimeout
        {
            get { return _settings.ReconnectTimeout; }
            set { _settings.ReconnectTimeout = value; }
        }

        /// <summary>
        /// Read errors for force reconnect
        /// </summary>
        public int ReadErrorsLimit
        {
            get { return _settings.ReadErrorsLimit; }
            set { _settings.ReadErrorsLimit = value; }
        }

        /// <summary>
        /// Reconnect errors before sleep
        /// </summary>
        public int ReconnectErrorsLimit
        {
            get { return _settings.ReconnectErrorsLimit; }
            set { _settings.ReconnectErrorsLimit = value; }
        }

        /// <summary>
        /// Get Vendor name
        /// </summary>
        public string VendorInfo => "Quote Feed for MT4 Platform";

        //public Version Version => GetType().Assembly.GetName().Version;
        /// <summary>
        /// Get current assembly version
        /// </summary>
        public Version Version => typeof(Feeder).GetTypeInfo().Assembly.GetName().Version;

        /// <summary>
        /// Get statistics
        /// </summary>
        public FeederStatistics Statistics
        {
            get
            {
                _statistics.Lock();
                var feederStatistics = _statistics;
                _statistics.Unlock();
                return feederStatistics;
            }
        }

        /// <summary>
        /// Human readable feeder name. Used for journaling etc.
        /// </summary>
        public string Name
        {
            get { return _settings.Name; }
            set { _settings.Name = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public string Description
        {
            get { return _settings.Description; }
            set { _settings.Description = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool CanPaused => _status == RunningStatus.Running;

        /// <summary>
        /// 
        /// </summary>
        public bool CanResumed => _status == RunningStatus.Paused;

        /// <summary>
        /// 
        /// </summary>
        public RunningStatus Status => _status;

        /// <summary>
        ///     Active symbols. To enable/disable specific symbol call AdviseSymbol/UnadviseSymbol.
        ///     By default - all symbols are active if non e were specified.
        /// </summary>
        public ReadOnlyCollection<string> ActiveSymbols => new ReadOnlyCollection<string>(_activeSymbols);

        #endregion

        #region private fields

        private event EventHandler StartedEvent;


        //
        private event EventHandler StoppedEvent;


        //
        private event EventHandler PausedEvent;


        //
        private event EventHandler<StatusMessageEventArgs> StatusEvent;


        //
        private event EventHandler<ReceiveQuoteEventArgs> ReceivedQuoteEvent;


        private readonly object _stateLock = new object();

        private bool _running;


        private IntPtr _hglobal;

        private int _failedConnectAttempts;
        private bool _isConnected;

        private const int MaxBuffSize = 65536;
        private const int MillisecondsInSecond = 1000;


        private static readonly Type TypeOfRawQuote = typeof(FeedTick);


        private static readonly int LengthOfRawQuote = Marshal.SizeOf<FeedTick>();


        private readonly List<string> _activeSymbols = new List<string>();


        private readonly AutoResetEvent _completeShutdownEvent = new AutoResetEvent(false);


        private readonly ManualResetEvent _completionEvent = new ManualResetEvent(true);

        //
        //
        //private readonly AutoResetEvent _initCompletionEvent = new AutoResetEvent(false);


        private readonly ManualResetEvent _initShutdownEvent = new ManualResetEvent(false);


        private readonly FeederStatistics _statistics = new FeederStatistics();


        private CloseDelegate _closeCall;


        private ConnectDelegate _connectCall;


        private IntPtr _feederInstance = IntPtr.Zero;


        private Timer _timer;

        private int _timerTimeout = 1;

        /// <summary>
        /// update timer settings
        /// </summary>
        private int TimerTimeout
        {
            get { return _timerTimeout; }
            set
            {
                _timerTimeout = value;
                _timer?.Change(value, -1);
            }
        }

        //
        //
        //private Thread _helperThread;


        private DsCreate _nativeCreateCall;


        private DsDestroy _nativeDestroyCall;


        private IntPtr _nativeModule;


        private DsVersion _nativeVersionCall;


        private ReadDelegate _readCall;


        private SetSymbolsDelegate _setSymbolsCall;


        private Settings _settings = new Settings();


        private RunningStatus _status = RunningStatus.Stopped;

        #endregion

        static Feeder()
        {
            InitWinSock();
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public Feeder()
        {
            _timer = new Timer
                (state =>
                {
                    try
                    {
                        OnTick();
                    }
                    catch
                    {
                    }
                    _timer?.Change(TimerTimeout, Timeout.Infinite);
                },
                 null,
                 Timeout.Infinite,
                 Timeout.Infinite);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            try
            {
                Stop();
            }
            catch
            {
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Pause()
        {
            if(_status != RunningStatus.Running)
                return;

            _completionEvent.WaitOne();
            try
            {
                _completionEvent.Reset();
                _status = RunningStatus.Paused;
            }
            finally
            {
                _completionEvent.Set();
                PausedEvent?.Invoke(this, null);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Resume()
        {
            if(_status != RunningStatus.Paused)
                return;

            _completionEvent.WaitOne();
            try
            {
                _completionEvent.Reset();
                _status = RunningStatus.Running;
            }
            finally
            {
                _completionEvent.Set();
                StartedEvent?.Invoke(this, null);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Start()
        {
            if(_status != RunningStatus.Stopped)
                return;

            if(_settings.Path == "")
                throw new Exception($"[{_settings.Name}]: 'Path' property not set");
            if(_settings.Server == "")
                throw new Exception($"[{_settings.Name}]: 'Server' property not set");

            _completionEvent.WaitOne();
            try
            {
                _completionEvent.Reset();
                _statistics.Lock();
                _statistics.Reset();
                _statistics.Unlock();
                LoadNativeModule();

                if(_feederInstance == IntPtr.Zero)
                {
                    InitInstance();
                    _failedConnectAttempts = 0;
                    //flag = false;
                }

                lock(_stateLock)
                {
                    _timer.Change(0, Timeout.Infinite);
                }
                _status = RunningStatus.Running;
                StartedEvent?.Invoke(this, null);
            }
            finally
            {
                _completionEvent.Set();
            }
        }

        /// <summary>
        ///     Stop feeder and block current thread until it's getting fully stopped
        /// </summary>
        /// <param name="ts">Maximum time to await</param>
        public void Stop(TimeSpan? ts = null)
        {
            if(_status == RunningStatus.Stopped)
                return;

            _status = RunningStatus.Stopped;


            _completionEvent.WaitOne();
            try
            {
                _completionEvent.Reset();

                lock(_stateLock)
                {
                    //while(_running)
                    //Monitor.Wait(_stateLock);

                    _timer.Change(-1, -1);

                    try
                    {
                        Disconnect();
                        if(_hglobal != IntPtr.Zero)
                            Marshal.FreeHGlobal(_hglobal);
                        _completeShutdownEvent.Set();

                        _initShutdownEvent.Set();
                        //if (_helperThread.IsAlive)
                        //{
                        //                    if (
                        //                        !_helperThread.Join
                        //                            (ts.HasValue
                        //                                 ? (int) ts.Value.TotalMilliseconds
                        //                                 : (int) TimeSpan.FromSeconds(3).TotalMilliseconds))
                        //                    {
                        //#if NET451
                        //                        _helperThread.Abort();
                        //#endif
                        //                    }
                        _completeShutdownEvent.WaitOne();
                        //}
                    }
                    finally
                    {
                        _timer = null;
                    }
                }

                UnloadNativeModule();
            }
            finally
            {
                _completionEvent.Set();
                StoppedEvent?.Invoke(this, null);
            }
        }

        /// <summary>
        ///     Enable receiving quotes for specified symbol.
        /// </summary>
        /// <param name="symbol"></param>
        public void AdviseSymbol(string symbol)
        {
            lock(_activeSymbols)
            {
                if(_activeSymbols.Contains(symbol))
                    throw new Exception("Duplicate symbol: " + symbol);

                _activeSymbols.Add(symbol);
            }
        }

        /// <summary>
        ///     Stop receiving quotes for specified symbol.
        /// </summary>
        /// <param name="symbol"></param>
        public void UnadviseSymbol(string symbol)
        {
            lock(_activeSymbols)
            {
                if(!_activeSymbols.Contains(symbol))
                    return;

                _activeSymbols.Remove(symbol);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        public ConfigurationResult ApplySettings(Settings settings)
        {
            var configurationResult = ConfigurationResult.NotChanged;
            if(settings.Path != _settings.Path)
                configurationResult = _status == RunningStatus.Stopped
                                          ? ConfigurationResult.SuccessfulApplied
                                          : ConfigurationResult.NeedReactivation;
            if((settings.Name != _settings.Name) && (configurationResult == ConfigurationResult.NotChanged))
                configurationResult = ConfigurationResult.SuccessfulApplied;
            if(settings.Server != _settings.Server)
                configurationResult = _status == RunningStatus.Stopped
                                          ? ConfigurationResult.SuccessfulApplied
                                          : ConfigurationResult.NeedReactivation;
            if(settings.Login != _settings.Login)
                configurationResult = _status == RunningStatus.Stopped
                                          ? ConfigurationResult.SuccessfulApplied
                                          : ConfigurationResult.NeedReactivation;
            if(settings.Password != _settings.Password)
                configurationResult = _status == RunningStatus.Stopped
                                          ? ConfigurationResult.SuccessfulApplied
                                          : ConfigurationResult.NeedReactivation;
            if((settings.Description != _settings.Description) &&
               (configurationResult == ConfigurationResult.NotChanged))
                configurationResult = ConfigurationResult.SuccessfulApplied;
            if((settings.ReadErrorsLimit != _settings.ReadErrorsLimit) &&
               (configurationResult == ConfigurationResult.NotChanged))
                configurationResult = ConfigurationResult.SuccessfulApplied;
            if((settings.ReconnectErrorsLimit != _settings.ReconnectErrorsLimit) &&
               (configurationResult == ConfigurationResult.NotChanged))
                configurationResult = ConfigurationResult.SuccessfulApplied;
            if((settings.ReconnectRetryTimeout != _settings.ReconnectRetryTimeout) &&
               (configurationResult == ConfigurationResult.NotChanged))
                configurationResult = ConfigurationResult.SuccessfulApplied;
            if((settings.ReconnectTimeout != _settings.ReconnectTimeout) &&
               (configurationResult == ConfigurationResult.NotChanged))
                configurationResult = ConfigurationResult.SuccessfulApplied;
            if(configurationResult != ConfigurationResult.NotChanged)
                _settings = settings;
            //}
            return configurationResult;
        }

        /// <summary>
        /// 
        /// </summary>
        public void ResetSettings()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #region private methods
        private void InitInstance()
        {
            _feederInstance = _nativeCreateCall();
            if(_feederInstance == IntPtr.Zero)
                throw new Exception($"[{_settings.Name}]: DsCreate failed.");

            //var feedVersion = _nativeVersionCall();

            var dst = IntPtr.Zero;
            RtlMoveMemory(ref dst, _feederInstance, IntPtr.Size);
            var cfeedInterfaceVtbl = Marshal.PtrToStructure<CFeedInterfaceVtbl>(dst);
            _connectCall = Marshal.GetDelegateForFunctionPointer<ConnectDelegate>(cfeedInterfaceVtbl.Connect);
            _closeCall = Marshal.GetDelegateForFunctionPointer<CloseDelegate>(cfeedInterfaceVtbl.Close);
            _setSymbolsCall = Marshal.GetDelegateForFunctionPointer<SetSymbolsDelegate>
                (cfeedInterfaceVtbl.SetSymbols);
            _readCall = Marshal.GetDelegateForFunctionPointer<ReadDelegate>(cfeedInterfaceVtbl.Read);
        }

        private void Disconnect()
        {
            try
            {
                if(!(_feederInstance != IntPtr.Zero) || (_closeCall == null))
                    return;

                _closeCall(_feederInstance);
                //_nativeDestroyCall(_feederInstance);
            }
            catch
            {
            }
            finally
            {
                //_feederInstance = IntPtr.Zero;
            }
        }

        private bool Connect()
        {
            try
            {
                _statistics.Lock();
                ++_statistics.TotalConnections;
                _statistics.Unlock();

                if(_feederInstance == IntPtr.Zero)
                    InitInstance();

                if(_connectCall
                        (_feederInstance,
                         _settings.Server,
                         _settings.Login.ToString(),
                         _settings.Password) == 0)
                    throw new Exception("No connection to MT4");

                _failedConnectAttempts = 0;
                TimerTimeout = 1;
                StatusEvent?.Invoke
                    (this,
                     new StatusMessageEventArgs
                         ($"[{_settings.Name}]: connected to {_settings.Server}",
                          null,
                          null));

                return true;
            }
            catch
            {
                _statistics.Lock();
                ++_statistics.ErrorsConnections;
                _statistics.Unlock();
                StatusEvent?.Invoke
                    (this,
                     new StatusMessageEventArgs
                         ($"[{_settings.Name}]: connection failed to {_settings.Server}",
                          null,
                          null));
                ++_failedConnectAttempts;
                if(_failedConnectAttempts >= _settings.ReconnectErrorsLimit)
                {
                    Disconnect();
                    TimerTimeout = _settings.ReconnectTimeout * MillisecondsInSecond;
                    _failedConnectAttempts = 0;
                    return false;
                }

                TimerTimeout = _settings.ReconnectRetryTimeout * MillisecondsInSecond;
            }

            return false;
        }

        private void OnTick()
        {
            lock(_stateLock)
            {
                //if(_running)
                //    Monitor.Wait(_stateLock);

                //_running = true;

                var hglobal = Marshal.AllocHGlobal(MaxBuffSize);

                if(_isConnected == false)
                    if((_isConnected = Connect()) == false)
                        return;

                var feedData = new FeedData
                {
                    Body = hglobal,
                    BodyMaxlen = MaxBuffSize
                };

                int readResult;
                try
                {
                    readResult = _readCall(_feederInstance, ref feedData);
                }
                catch
                {
                    readResult = 0;
                }

                if((readResult == 0) || (feedData.Result != 0) || (feedData.ResultString != ""))
                {
                    _statistics.Lock();
                    ++_statistics.ReceivedErrors;
                    _statistics.Unlock();

                    ++_failedConnectAttempts;

                    if(_failedConnectAttempts > _settings.ReadErrorsLimit)
                    {
                        StatusEvent?.Invoke
                            (this,
                             new StatusMessageEventArgs
                                 ($"[{_settings.Name}]: too many reading errors - auto reconnect has been scheduled.",
                                  null,
                                  null));

                        Disconnect();
                        TimerTimeout = _settings.ReconnectTimeout * MillisecondsInSecond;
                        _failedConnectAttempts = 0;
                        _isConnected = false;
                    }

                    return;
                }

                if((feedData.TicksCount <= 0) || (feedData.TicksCount > 32))
                    return;

                _statistics.Lock();
                _statistics.LastQuoteTime = DateTime.Now;
                _statistics.QuotesReceivedTotal += feedData.TicksCount;
                _statistics.Unlock();

                _failedConnectAttempts = 0;

                if((ReceivedQuoteEvent != null) && (_status == RunningStatus.Running))
                    ProceedQuotes(ref feedData);
            }
        }

        private void ProceedQuotes(ref FeedData context)
        {
            var ptrTicks = IntPtr.Zero;
            try
            {
                ptrTicks = Marshal.AllocHGlobal(context.Ticks.Length);
                Marshal.Copy(context.Ticks, 0, ptrTicks, LengthOfRawQuote * context.TicksCount);
                for(var i = 0; i < context.TicksCount; ++i)
                {
                    var feedTick = Marshal.PtrToStructure<FeedTick>
                        (new IntPtr(i*LengthOfRawQuote + ptrTicks.ToInt32()));
                    lock(_activeSymbols)
                    {
                        if((_activeSymbols == null) || !_activeSymbols.Any() ||
                           _activeSymbols.Contains(feedTick.Symbol))
                        {
                            _statistics.Lock();
                            ++_statistics.QuotesReceived;
                            _statistics.Unlock();
                            var quote = new Quote
                                (_settings.Name, feedTick.Bank, feedTick.Symbol, feedTick.Bid, feedTick.Ask, 0);
                            try
                            {
                                ReceivedQuoteEvent?.Invoke(this, new ReceiveQuoteEventArgs(quote));
                            }
                            catch
                            {
                            }
                        }
                    }
                }
            }
            finally
            {
                if(ptrTicks != IntPtr.Zero)
                    Marshal.FreeHGlobal(ptrTicks);
            }
        }

        //public void AdviseSymbol(string symbol, string[] fields)
        //{
        //    AdviseSymbol(symbol);
        //}

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr LoadLibrary(string path);


        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FreeLibrary(IntPtr module);


        private void LoadNativeModule()
        {
            _nativeModule = LoadLibrary(_settings.Path);
            if(_nativeModule == null)
                throw new Exception($"[{_settings.Name}]: no load '{_settings.Path}'");

            var procAddress1 = GetProcAddress(_nativeModule, "DsCreate");
            var procAddress2 = GetProcAddress(_nativeModule, "DsDestroy");
            var procAddress3 = GetProcAddress(_nativeModule, "DsVersion");
            if((procAddress1 == IntPtr.Zero) || (procAddress2 == IntPtr.Zero) ||
                (procAddress3 == IntPtr.Zero))
            {
                FreeLibrary(_nativeModule);
                _nativeModule = IntPtr.Zero;
                //_nativeModule = null;
                throw new Exception($"[{_settings.Name}]: no query API");
            }

            _nativeCreateCall =
                Marshal.GetDelegateForFunctionPointer<DsCreate>(procAddress1);
            _nativeDestroyCall =
                Marshal.GetDelegateForFunctionPointer<DsDestroy>(procAddress1);
            _nativeVersionCall =
                Marshal.GetDelegateForFunctionPointer<DsVersion>(procAddress3);
        }


        private void UnloadNativeModule()
        {
            if(_nativeModule == null)
                return;

            FreeLibrary(_nativeModule);
            _nativeModule = IntPtr.Zero;
        }

        /// <summary>
        ///     The WSAStartup function initiates use of WS2_32.DLL by a process.
        /// </summary>
        /// <returns>The WSAStartup function returns zero if successful.</returns>
        private static int InitWinSock()
        {
            try
            {
                var pData = Marshal.AllocHGlobal(Marshal.SizeOf<WsaData>());
                var err = WSAStartup(0x0202, pData);
                //WSAData wsaData = (WSAData)Marshal.PtrToStructure(pData, typeof(WSAData));
                Marshal.FreeHGlobal(pData);
                return err;
            }
            catch(Exception)
            {
                return -1;
            }
        }

        [DllImport("WS2_32.DLL")]
        private static extern int WSAStartup(int wVersionRequested, IntPtr lpWsaData);

        [DllImport("WS2_32.DLL")]
        private static extern int WSACleanup();

        [DllImport("kernel32.dll")]
        private static extern void RtlMoveMemory(ref IntPtr dst, IntPtr src, int size);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetProcAddress(IntPtr module, string procName);

        #endregion
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    internal struct WsaData
    {
        public readonly ushort wVersion;
        public readonly ushort wHighVersion;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256 + 1)]
        public readonly string szDescription;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128 + 1)]
        public readonly string szSystemStatus;

        public readonly ushort iMaxSockets;
        public readonly ushort iMaxUdpDg;
        public readonly IntPtr lpVendorInfo;
        //char FAR* lpVendorInfo;
    }
}