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
        //[EditorBrowsable(EditorBrowsableState.Never)]
        //[DebuggerBrowsable(DebuggerBrowsableState.Never)]
        //private const int StackSize = 65536;

        [EditorBrowsable(EditorBrowsableState.Never)]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        readonly object _stateLock = new object();

        private bool _running;

        [EditorBrowsable(EditorBrowsableState.Never)]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private IntPtr _hglobal;


        [EditorBrowsable(EditorBrowsableState.Never)]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly Type TypeOfRawQuote = typeof(FeedTick);

        [EditorBrowsable(EditorBrowsableState.Never)]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly int LengthOfRawQuote = Marshal.SizeOf<FeedTick>();

        [EditorBrowsable(EditorBrowsableState.Never)]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly List<string> _activeSymbols = new List<string>();

        [EditorBrowsable(EditorBrowsableState.Never)]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly AutoResetEvent _completeShutdownEvent = new AutoResetEvent(false);

        [EditorBrowsable(EditorBrowsableState.Never)]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly ManualResetEvent _completionEvent = new ManualResetEvent(true);

        //[EditorBrowsable(EditorBrowsableState.Never)]
        //[DebuggerBrowsable(DebuggerBrowsableState.Never)]
        //private readonly AutoResetEvent _initCompletionEvent = new AutoResetEvent(false);

        [EditorBrowsable(EditorBrowsableState.Never)]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly ManualResetEvent _initShutdownEvent = new ManualResetEvent(false);

        [EditorBrowsable(EditorBrowsableState.Never)]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly FeederStatistics _statistics = new FeederStatistics();

        [EditorBrowsable(EditorBrowsableState.Never)]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private CloseDelegate _closeCall;

        [EditorBrowsable(EditorBrowsableState.Never)]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ConnectDelegate _connectCall;

        [EditorBrowsable(EditorBrowsableState.Never)]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private IntPtr _feederInstance = IntPtr.Zero;

        [EditorBrowsable(EditorBrowsableState.Never)]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Timer _timer;

        //[EditorBrowsable(EditorBrowsableState.Never)]
        //[DebuggerBrowsable(DebuggerBrowsableState.Never)]
        //private Thread _helperThread;

        [EditorBrowsable(EditorBrowsableState.Never)]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private DsCreate _nativeCreateCall;

        [EditorBrowsable(EditorBrowsableState.Never)]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private DsDestroy _nativeDestroyCall;

        [EditorBrowsable(EditorBrowsableState.Never)]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private IntPtr _nativeModule;

        [EditorBrowsable(EditorBrowsableState.Never)]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private DsVersion _nativeVersionCall;

        [EditorBrowsable(EditorBrowsableState.Never)]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ReadDelegate _readCall;

        [EditorBrowsable(EditorBrowsableState.Never)]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private SetSymbolsDelegate _setSymbolsCall;

        [EditorBrowsable(EditorBrowsableState.Never)]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Settings _settings = new Settings();

        [EditorBrowsable(EditorBrowsableState.Never)]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private RunningStatus _status = RunningStatus.Stopped;

        static Feeder()
        {
            InitWinSock();
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
        /// 
        /// </summary>
        public int ReconnectRetryTimeout
        {
            get { return _settings.ReconnectRetryTimeout; }
            set { _settings.ReconnectRetryTimeout = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public int ReconnectTimeout
        {
            get { return _settings.ReconnectTimeout; }
            set { _settings.ReconnectTimeout = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public int ReadErrorsLimit
        {
            get { return _settings.ReadErrorsLimit; }
            set { _settings.ReadErrorsLimit = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public int ReconnectErrorsLimit
        {
            get { return _settings.ReconnectErrorsLimit; }
            set { _settings.ReconnectErrorsLimit = value; }
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
        /// 
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
        public void Pause()
        {
            if (_status != RunningStatus.Running)
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
            if (_status != RunningStatus.Paused)
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
            if (_status != RunningStatus.Stopped)
                return;

            if (_settings.Path == "")
                throw new Exception($"[{_settings.Name}]: 'Path' property not set");
            if (_settings.Server == "")
                throw new Exception($"[{_settings.Name}]: 'Server' property not set");

            _completionEvent.WaitOne();
            try
            {
                _completionEvent.Reset();
                _statistics.Lock();
                _statistics.Reset();
                _statistics.Unlock();
                LoadNativeModule();
                StartHelper();
                _status = RunningStatus.Running;
                StartedEvent?.Invoke(this, null);
            }
            finally
            {
                _completionEvent.Set();
            }
        }

        /// <summary>
        ///     Stod feeder.
        ///     Block current thread not more than 'ts' until feed beig fully stopped.
        /// </summary>
        public void Stop(TimeSpan? ts = null)
        {
            if (_status == RunningStatus.Stopped)
                return;

            _completionEvent.WaitOne();
            try
            {
                _completionEvent.Reset();
                StopHelper(ts);
                UnloadNativeModule();
            }
            finally
            {
                _status = RunningStatus.Stopped;
                _completionEvent.Set();
                StoppedEvent?.Invoke(this, null);
            }
        }

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
        ///     Active symbols. To enable/disable specific symbol call AdviseSymbol/UnadviseSymbol.
        ///     By default - all symbols are active if non e were specified.
        /// </summary>
        public ReadOnlyCollection<string> ActiveSymbols => new ReadOnlyCollection<string>(_activeSymbols);

        //public void AdviseSymbol(string symbol, string[] fields)
        //{
        //    AdviseSymbol(symbol);
        //}

        /// <summary>
        ///     Enable receiving quotes for specified symbol.
        /// </summary>
        /// <param name="symbol"></param>
        public void AdviseSymbol(string symbol)
        {
            lock (_activeSymbols)
            {
                if (_activeSymbols.Contains(symbol))
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
            lock (_activeSymbols)
            {
                if (!_activeSymbols.Contains(symbol))
                    return;

                _activeSymbols.Remove(symbol);
            }
        }

        [DllImport("WS2_32.DLL")]
        private static extern int WSAStartup(int wVersionRequested, IntPtr lpWsaData);

        [DllImport("WS2_32.DLL")]
        private static extern int WSACleanup();

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
            catch (Exception)
            {
                return -1;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        //[DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private event EventHandler StartedEvent;

        [EditorBrowsable(EditorBrowsableState.Never)]
        //[DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private event EventHandler StoppedEvent;

        [EditorBrowsable(EditorBrowsableState.Never)]
        //[DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private event EventHandler PausedEvent;

        [EditorBrowsable(EditorBrowsableState.Never)]
        //[DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private event EventHandler<StatusMessageEventArgs> StatusEvent;

        [EditorBrowsable(EditorBrowsableState.Never)]
        //[DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private event EventHandler<ReceiveQuoteEventArgs> ReceivedQuoteEvent;


        [EditorBrowsable(EditorBrowsableState.Never)]
        [DllImport("kernel32.dll")]
        private static extern void RtlMoveMemory(ref IntPtr dst, IntPtr src, int size);


        [EditorBrowsable(EditorBrowsableState.Never)]
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetProcAddress(IntPtr module, string procName);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        public ConfigurationResult ApplySettings(Settings settings)
        {
            var configurationResult = ConfigurationResult.NotChanged;
            //if(settings is Mt4FeederSettings)
            //{
            //var mt4FeederSettings = (Mt4FeederSettings) settings;
            if (settings.Path != _settings.Path)
                configurationResult = _status == RunningStatus.Stopped
                                          ? ConfigurationResult.SuccessfulApplied
                                          : ConfigurationResult.NeedReactivation;
            if ((settings.Name != _settings.Name) && (configurationResult == ConfigurationResult.NotChanged))
                configurationResult = ConfigurationResult.SuccessfulApplied;
            if (settings.Server != _settings.Server)
                configurationResult = _status == RunningStatus.Stopped
                                          ? ConfigurationResult.SuccessfulApplied
                                          : ConfigurationResult.NeedReactivation;
            if (settings.Login != _settings.Login)
                configurationResult = _status == RunningStatus.Stopped
                                          ? ConfigurationResult.SuccessfulApplied
                                          : ConfigurationResult.NeedReactivation;
            if (settings.Password != _settings.Password)
                configurationResult = _status == RunningStatus.Stopped
                                          ? ConfigurationResult.SuccessfulApplied
                                          : ConfigurationResult.NeedReactivation;
            if ((settings.Description != _settings.Description) &&
                (configurationResult == ConfigurationResult.NotChanged))
                configurationResult = ConfigurationResult.SuccessfulApplied;
            if ((settings.ReadErrorsLimit != _settings.ReadErrorsLimit) &&
                (configurationResult == ConfigurationResult.NotChanged))
                configurationResult = ConfigurationResult.SuccessfulApplied;
            if ((settings.ReconnectErrorsLimit != _settings.ReconnectErrorsLimit) &&
                (configurationResult == ConfigurationResult.NotChanged))
                configurationResult = ConfigurationResult.SuccessfulApplied;
            if ((settings.ReconnectRetryTimeout != _settings.ReconnectRetryTimeout) &&
                (configurationResult == ConfigurationResult.NotChanged))
                configurationResult = ConfigurationResult.SuccessfulApplied;
            if ((settings.ReconnectTimeout != _settings.ReconnectTimeout) &&
                (configurationResult == ConfigurationResult.NotChanged))
                configurationResult = ConfigurationResult.SuccessfulApplied;
            if (configurationResult != ConfigurationResult.NotChanged)
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

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr LoadLibrary(string path);

        [EditorBrowsable(EditorBrowsableState.Never)]
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FreeLibrary(IntPtr module);


        [EditorBrowsable(EditorBrowsableState.Never)]
        private void LoadNativeModule()
        {
            _nativeModule = LoadLibrary(_settings.Path);
            if (_nativeModule == null)
            {
                throw new Exception($"[{_settings.Name}]: no load '{_settings.Path}'");
            }

            var procAddress1 = GetProcAddress(_nativeModule, "DsCreate");
            var procAddress2 = GetProcAddress(_nativeModule, "DsDestroy");
            var procAddress3 = GetProcAddress(_nativeModule, "DsVersion");
            if ((procAddress1 == IntPtr.Zero) || (procAddress2 == IntPtr.Zero) ||
                (procAddress3 == IntPtr.Zero))
            {
                FreeLibrary(_nativeModule);
                _nativeModule = IntPtr.Zero;
                //_nativeModule = null;
                throw new Exception($"[{_settings.Name}]: no query API");
            }

            _nativeCreateCall =
                (DsCreate) Marshal.GetDelegateForFunctionPointer<DsCreate>(procAddress1);
            _nativeDestroyCall =
                (DsDestroy) Marshal.GetDelegateForFunctionPointer<DsDestroy>(procAddress1);
            _nativeVersionCall =
                (DsVersion) Marshal.GetDelegateForFunctionPointer<DsVersion>(procAddress3);
        }


        [EditorBrowsable(EditorBrowsableState.Never)]
        private void UnloadNativeModule()
        {
            if (_nativeModule == null)
                return;

            FreeLibrary(_nativeModule);
            _nativeModule = IntPtr.Zero;
        }


        [EditorBrowsable(EditorBrowsableState.Never)]
        private void StartHelper()
        {
            lock (_stateLock)
            {
                _timer = new Timer(OnTick, null, Timeout.Infinite, 1);
                //_helperThread = new Thread(HelperCallback /*, 65536*/);
                //_helperThread.IsBackground = true;
                //_helperThread.Name = "[feeder]: " + _settings.Name;
                //_initCompletionEvent.Reset();
                //_helperThread.Start();
                //_initCompletionEvent.WaitOne();
            }
        }


        [EditorBrowsable(EditorBrowsableState.Never)]
        private void StopHelper(TimeSpan? ts = null)
        {
            lock (_stateLock)
            {
                while (_running)
                    Monitor.Wait(_stateLock);

                _timer?.Dispose();

                try
                {
                    ReleaseInstance();
                    if (_hglobal != IntPtr.Zero)
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
        }


        [EditorBrowsable(EditorBrowsableState.Never)]
        private void InitInstance()
        {
            _feederInstance = _nativeCreateCall();
            if (_feederInstance == IntPtr.Zero)
                throw new Exception($"[{_settings.Name}]: DsCreate failed.");

            var dst = IntPtr.Zero;
            RtlMoveMemory(ref dst, _feederInstance, IntPtr.Size);
            var cfeedInterfaceVtbl = Marshal.PtrToStructure<CFeedInterfaceVtbl>(dst);
            _connectCall = Marshal.GetDelegateForFunctionPointer<ConnectDelegate>(cfeedInterfaceVtbl.Connect);
            _closeCall = Marshal.GetDelegateForFunctionPointer<CloseDelegate>(cfeedInterfaceVtbl.Close);
            _setSymbolsCall = Marshal.GetDelegateForFunctionPointer<SetSymbolsDelegate>
                (cfeedInterfaceVtbl.SetSymbols);
            _readCall = Marshal.GetDelegateForFunctionPointer<ReadDelegate>(cfeedInterfaceVtbl.Read);
        }


        [EditorBrowsable(EditorBrowsableState.Never)]
        private void ReleaseInstance()
        {
            try
            {
                if (!(_feederInstance != IntPtr.Zero) || (_closeCall == null))
                    return;

                _closeCall(_feederInstance);
                _nativeDestroyCall(_feederInstance);
            }
            catch
            {
            }
            finally
            {
                _feederInstance = IntPtr.Zero;
            }
        }


        [EditorBrowsable(EditorBrowsableState.Never)]
        private void _HelperCallback(object context)
        {
            var num1 = 0;
            var flag = false;
            var num2 = IntPtr.Zero;
            var millisecondsTimeout = 1;
            //_initCompletionEvent.Set();
            _initShutdownEvent.Reset();
            _completeShutdownEvent.Reset();
            StatusEvent?.Invoke
                (this, new StatusMessageEventArgs($"[{_settings.Name}]: helper thread started.", null, null));
            _hglobal = Marshal.AllocHGlobal(65536);
            try
            {
                while (!_initShutdownEvent.WaitOne(millisecondsTimeout /*, true*/))
                {
                    if (_feederInstance == IntPtr.Zero)
                    {
                        InitInstance();
                        num1 = 0;
                        flag = false;
                    }
                    if (!flag)
                        try
                        {
                            _statistics.Lock();
                            ++_statistics.TotalConnections;
                            _statistics.Unlock();
                            if (
                                _connectCall
                                    (_feederInstance,
                                     _settings.Server,
                                     _settings.Login.ToString(),
                                     _settings.Password) == 0)
                                throw new Exception("No connection to MT4");

                            num1 = 0;
                            flag = true;
                            millisecondsTimeout = 1;
                            StatusEvent?.Invoke
                                (this,
                                 new StatusMessageEventArgs
                                     (
                                      $"[{_settings.Name}]: connected to {_settings.Server}",
                                      null,
                                      null));
                        }
                        catch
                        {
                            _statistics.Lock();
                            ++_statistics.ErrorsConnections;
                            _statistics.Unlock();
                            StatusEvent?.Invoke
                                (this,
                                 new StatusMessageEventArgs
                                     (
                                      $"[{_settings.Name}]: connection failed to {_settings.Server}",
                                      null,
                                      null));
                            ++num1;
                            if (num1 >= _settings.ReconnectErrorsLimit)
                            {
                                ReleaseInstance();
                                millisecondsTimeout = _settings.ReconnectTimeout*1000;
                                num1 = 0;
                                continue;
                            }

                            millisecondsTimeout = _settings.ReconnectRetryTimeout*1000;
                            continue;
                        }

                    var feedData = new FeedData
                    {
                        Body = _hglobal,
                        BodyMaxlen = 65536
                    };
                    int num3;
                    try
                    {
                        num3 = _readCall(_feederInstance, ref feedData);
                    }
                    catch
                    {
                        num3 = 0;
                    }
                    if ((num3 == 0) || (feedData.Result != 0) || (feedData.ResultString != ""))
                    {
                        _statistics.Lock();
                        ++_statistics.ReceivedErrors;
                        _statistics.Unlock();
                        ++num1;
                        //if (Thread.CurrentThread.Priority != ThreadPriority.Normal)
                        //    Thread.CurrentThread.Priority = ThreadPriority.Normal;
                        if (num1 > _settings.ReadErrorsLimit)
                        {
                            StatusEvent?.Invoke
                                (this,
                                 new StatusMessageEventArgs
                                     (
                                      string.Format
                                          ("[{0}]: too many reading errors - auto reconnect has been scheduled.",
                                           _settings.Name,
                                           _settings.Server),
                                      null,
                                      null));
                            try
                            {
                                _closeCall(_feederInstance);
                                ReleaseInstance();
                            }
                            catch
                            {
                            }
                            finally
                            {
                                num1 = 0;
                                flag = false;
                            }
                        }
                    }
                    else if ((feedData.TicksCount > 0) && (feedData.TicksCount <= 32))
                    {
                        _statistics.Lock();
                        _statistics.LastQuoteTime = DateTime.Now;
                        _statistics.QuotesReceivedTotal += feedData.TicksCount;
                        _statistics.Unlock();
                        num1 = 0;
                        //if (Thread.CurrentThread.Priority != ThreadPriority.AboveNormal)
                        //    Thread.CurrentThread.Priority = ThreadPriority.AboveNormal;
                        if ((ReceivedQuoteEvent != null) && (_status == RunningStatus.Running))
                            ProcessingQuotes(ref feedData);
                    }
                }
            }
            //catch (ThreadAbortException ex)
            //{
            //}
            catch (Exception ex)
            {
                if (StatusEvent == null)
                    return;

                StatusEvent
                    (this,
                     new StatusMessageEventArgs
                         ($"[{_settings.Name}]: feeder error. See inner exception.", ex, null));
            }
            finally
            {
                ReleaseInstance();
                if (_hglobal != IntPtr.Zero)
                    Marshal.FreeHGlobal(_hglobal);
                StatusEvent?.Invoke
                    (this,
                     new StatusMessageEventArgs($"[{_settings.Name}]: helper thread stopped.", null, null));
                _completeShutdownEvent.Set();
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        private void OnTick(object o)
        {
            try
            {
                lock (_stateLock)
                {
                    if (_running)
                        Monitor.Wait(_stateLock);

                    _running = true;

                    var num1 = 0;
                    var flag = false;
                    var num2 = IntPtr.Zero;
                    var millisecondsTimeout = 1;
                    var hglobal = Marshal.AllocHGlobal(65536);

                    //while (!_initShutdownEvent.WaitOne(millisecondsTimeout/*, true*/))
                    //{
                    if (_feederInstance == IntPtr.Zero)
                    {
                        InitInstance();
                        num1 = 0;
                        flag = false;
                    }
                    if (!flag)
                        try
                        {
                            _statistics.Lock();
                            ++_statistics.TotalConnections;
                            _statistics.Unlock();
                            if (
                                _connectCall
                                    (_feederInstance,
                                     _settings.Server,
                                     _settings.Login.ToString(),
                                     _settings.Password) == 0)
                                throw new Exception("No connection to MT4");

                            num1 = 0;
                            flag = true;
                            millisecondsTimeout = 1;
                            StatusEvent?.Invoke
                                (this,
                                 new StatusMessageEventArgs
                                     (
                                      $"[{_settings.Name}]: connected to {_settings.Server}",
                                      null,
                                      null));
                        }
                        catch
                        {
                            _statistics.Lock();
                            ++_statistics.ErrorsConnections;
                            _statistics.Unlock();
                            StatusEvent?.Invoke
                                (this,
                                 new StatusMessageEventArgs
                                     (
                                      $"[{_settings.Name}]: connection failed to {_settings.Server}",
                                      null,
                                      null));
                            ++num1;
                            if (num1 >= _settings.ReconnectErrorsLimit)
                            {
                                ReleaseInstance();
                                millisecondsTimeout = _settings.ReconnectTimeout*1000;
                                num1 = 0;
                                return;
                            }

                            millisecondsTimeout = _settings.ReconnectRetryTimeout*1000;
                            return;
                        }

                    var feedData = new FeedData
                    {
                        Body = hglobal,
                        BodyMaxlen = 65536
                    };
                    int num3;
                    try
                    {
                        num3 = _readCall(_feederInstance, ref feedData);
                    }
                    catch
                    {
                        num3 = 0;
                    }
                    if ((num3 == 0) || (feedData.Result != 0) || (feedData.ResultString != ""))
                    {
                        _statistics.Lock();
                        ++_statistics.ReceivedErrors;
                        _statistics.Unlock();
                        ++num1;
                        //if (Thread.CurrentThread.Priority != ThreadPriority.Normal)
                        //    Thread.CurrentThread.Priority = ThreadPriority.Normal;
                        if (num1 > _settings.ReadErrorsLimit)
                        {
                            StatusEvent?.Invoke
                                (this,
                                 new StatusMessageEventArgs
                                     (
                                      string.Format
                                          ("[{0}]: too many reading errors - auto reconnect has been scheduled.",
                                           _settings.Name,
                                           _settings.Server),
                                      null,
                                      null));
                            try
                            {
                                _closeCall(_feederInstance);
                                ReleaseInstance();
                            }
                            catch
                            {
                            }
                            finally
                            {
                                num1 = 0;
                                flag = false;
                            }
                        }
                    }
                    else if ((feedData.TicksCount > 0) && (feedData.TicksCount <= 32))
                    {
                        _statistics.Lock();
                        _statistics.LastQuoteTime = DateTime.Now;
                        _statistics.QuotesReceivedTotal += feedData.TicksCount;
                        _statistics.Unlock();
                        num1 = 0;
                        //if (Thread.CurrentThread.Priority != ThreadPriority.AboveNormal)
                        //    Thread.CurrentThread.Priority = ThreadPriority.AboveNormal;
                        if ((ReceivedQuoteEvent != null) && (_status == RunningStatus.Running))
                            ProcessingQuotes(ref feedData);
                    }
                }
            }
            finally
            {
                lock (_stateLock)
                {
                    _running = false;
                    Monitor.PulseAll(_stateLock);
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        private void ProcessingQuotes(ref FeedData context)
        {
            var num1 = IntPtr.Zero;
            try
            {
                num1 = Marshal.AllocHGlobal(context.Ticks.Length);
                Marshal.Copy(context.Ticks, 0, num1, LengthOfRawQuote*context.TicksCount);
                var num2 = 0;
                for (; num2 < context.TicksCount; ++num2)
                {
                    var feedTick = Marshal.PtrToStructure<FeedTick>(new IntPtr(num2*LengthOfRawQuote + num1.ToInt32()));
                    lock (_activeSymbols)
                    {
                        if ((_activeSymbols == null) || !_activeSymbols.Any() ||
                            _activeSymbols.Contains(feedTick.Symbol))
                        {
                            _statistics.Lock();
                            ++_statistics.QuotesReceived;
                            _statistics.Unlock();
                            var local4 = new Quote
                                (_settings.Name, feedTick.Bank, feedTick.Symbol, feedTick.Bid, feedTick.Ask, 0);
                            try
                            {
                                ReceivedQuoteEvent(this, new ReceiveQuoteEventArgs(local4));
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
                if (num1 != IntPtr.Zero)
                    Marshal.FreeHGlobal(num1);
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        private struct WsaData
        {
            private readonly ushort wVersion;
            private readonly ushort wHighVersion;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256 + 1)]
            public readonly string szDescription;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128 + 1)]
            public readonly string szSystemStatus;

            private readonly ushort iMaxSockets;
            private readonly ushort iMaxUdpDg;
            private readonly IntPtr lpVendorInfo;
            //char FAR* lpVendorInfo;
        }
    }
}