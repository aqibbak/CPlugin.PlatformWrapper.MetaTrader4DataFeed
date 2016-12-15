namespace CPlugin.PlatformWrapper.MetaTrader4DataFeed.Common
{
    /// <summary>
    /// 
    /// </summary>
    public interface INamedObject
    {
        /// <summary>
        /// 
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        string Description { get; set; }
    }
}