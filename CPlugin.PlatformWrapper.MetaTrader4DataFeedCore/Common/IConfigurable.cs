namespace CPlugin.PlatformWrapper.MetaTrader4DataFeed.Common
{
    /// <summary>
    /// 
    /// </summary>
    public interface IConfigurable
    {
        /// <summary>
        /// 
        /// </summary>
        object Settings { get; }

        /// <summary>
        /// 
        /// </summary>
        bool CanResetSettings { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        ConfigurationResult ApplySettings(object settings);

        /// <summary>
        /// 
        /// </summary>
        void ResetSettings();
    }
}