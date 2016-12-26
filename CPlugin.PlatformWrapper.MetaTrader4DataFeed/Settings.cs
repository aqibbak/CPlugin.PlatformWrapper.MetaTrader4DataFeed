using System.ComponentModel;
using System.Diagnostics;

namespace CPlugin.PlatformWrapper.MetaTrader4DataFeed
{
    /// <summary>
    ///     DataFeed settings
    /// </summary>
#if NET451
    [DefaultProperty("Name")]
#endif
    public class Settings
    {
        /// <summary>
        /// 
        /// </summary>
        private const int READ_ERRORS_LIMIT = 20;

        /// <summary>
        /// 
        /// </summary>
        private const int RECONNECT_ERRORS_LIMIT = 5;

        /// <summary>
        /// 
        /// </summary>
        private const int RECONNECT_TIMEOUT = 60;

        /// <summary>
        /// 
        /// </summary>
        private const int RECONNECT_RETRY_TIMEOUT = 2;

        /// <summary>
        /// default constructor
        /// </summary>
        public Settings()
        {
            ReadErrorsLimit = READ_ERRORS_LIMIT;
            ReconnectRetryTimeout = RECONNECT_RETRY_TIMEOUT;
            ReconnectTimeout = RECONNECT_TIMEOUT;
            ReconnectErrorsLimit = RECONNECT_ERRORS_LIMIT;
        }


        private int _readErrorsLimit = 20;

        
        
        private int _reconnectErrorsLimit = 5;

        
        
        private int _reconnectRetryTimeout = 2;

        
        
        private int _reconnectTimeout = 60;


        /// <summary>
        /// Feeder name
        /// </summary>
        [Category("Common")]
        public string Name { get; set; } = "";

        /// <summary>
        /// Feeder description
        /// </summary>
        [Category("Common")]
        [Description("Feeder description")]
        public string Description { get; set; } = "";

        /// <summary>
        /// Path to *.feed DLL
        /// </summary>
        [Category("Connection")]
        [Description("Path to *.feed DLL")]
        public string Path { get; set; } = "";

        /// <summary>
        /// Server
        /// </summary>
        [Description("Server")]
        [Category("Connection")]
        public string Server { get; set; } = "";

        /// <summary>
        /// Login for connection
        /// </summary>
        [Category("Connection")]
        [Description("Login for connection")]
        public int Login { get; set; }

        /// <summary>
        /// Password for connection
        /// </summary>
#if NET451
        [PasswordPropertyText(true)]
#endif
        [Category("Connection")]
        [Description("Password for connection")]
        public string Password { get; set; } = "";

        /// <summary>
        /// Idle timeout before reconnect, in sec.
        /// </summary>
        [DisplayName("Reconnect Idle Timeout")]
        [Description("Idle timeout before reconnect, in sec.")]
        [RefreshProperties(RefreshProperties.All)]
        [Category("Timeouts")]
        public int ReconnectRetryTimeout
        {
            get { return _reconnectRetryTimeout; }
            set
            {
                if (value <= 0)
                    return;

                _reconnectRetryTimeout = value;
            }
        }

        /// <summary>
        /// Sleep after several failed attempts, in sec.
        /// </summary>
        [RefreshProperties(RefreshProperties.All)]
        [Description("Sleep after several failed attempts, in sec.")]
        [Category("Timeouts")]
        [DisplayName("Reconnect Sleep Timeout")]
        public int ReconnectTimeout
        {
            get { return _reconnectTimeout; }
            set
            {
                if (value <= 0)
                    return;

                _reconnectTimeout = value;
            }
        }

        /// <summary>
        /// Read errors for force reconnect
        /// </summary>
        [Description("Read errors for force reconnect")]
        [Category("Timeouts")]
        [RefreshProperties(RefreshProperties.All)]
        [DisplayName("Maximum Read Errors")]
        public int ReadErrorsLimit
        {
            get { return _readErrorsLimit; }
            set
            {
                if (value <= 0)
                    return;

                _readErrorsLimit = value;
            }
        }

        /// <summary>
        /// Reconnect errors before sleep
        /// </summary>
        [Category("Timeouts")]
        [Description("Reconnect errors before sleep")]
        [RefreshProperties(RefreshProperties.All)]
        [DisplayName("Reconnect Attempts")]
        public int ReconnectErrorsLimit
        {
            get { return _reconnectErrorsLimit; }
            set
            {
                if (value <= 0)
                    return;

                _reconnectErrorsLimit = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override string ToString()
        {
            return Name == "" ? base.ToString() : Name;
        }
    }
}