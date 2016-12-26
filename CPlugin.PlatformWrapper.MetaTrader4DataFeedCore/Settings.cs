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
        public const int READ_ERRORS_LIMIT = 20;

        /// <summary>
        /// 
        /// </summary>
        public const int RECONNECT_ERRORS_LIMIT = 5;

        /// <summary>
        /// 
        /// </summary>
        public const int RECONNECT_TIMEOUT = 60;

        /// <summary>
        /// 
        /// </summary>
        public const int RECONNECT_RETRY_TIMEOUT = 2;

        
        
        private string _description = "";

        
        
        private int _login;

        
        
        private string _name = "";

        
        
        private string _password = "";

        
        
        private string _path = "";

        
        
        private int _readErrorsLimit = 20;

        
        
        private int _reconnectErrorsLimit = 5;

        
        
        private int _reconnectRetryTimeout = 2;

        
        
        private int _reconnectTimeout = 60;

        
        
        private string _server = "";

        /// <summary>
        /// 
        /// </summary>
        [Category("Common")]
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        [Category("Common")]
        [Description("Feeder description")]
        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        [Category("Connection")]
        [Description("Path to *.feed DLL")]
        public string Path
        {
            get { return _path; }
            set { _path = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        [Description("Server")]
        [Category("Connection")]
        public string Server
        {
            get { return _server; }
            set { _server = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        [Category("Connection")]
        [Description("Login for connection")]
        public int Login
        {
            get { return _login; }
            set { _login = value; }
        }

        /// <summary>
        /// 
        /// </summary>
#if NET451
        [PasswordPropertyText(true)]
#endif
        [Category("Connection")]
        [Description("Password for connection")]
        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }

        /// <summary>
        /// 
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
        /// 
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
        /// 
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
        /// 
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
            if (_name == "")
                return base.ToString();

            return _name;
        }
    }
}