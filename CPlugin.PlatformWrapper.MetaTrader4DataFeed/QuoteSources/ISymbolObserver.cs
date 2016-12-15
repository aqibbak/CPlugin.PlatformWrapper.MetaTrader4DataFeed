using System.Collections.ObjectModel;

namespace CPlugin.PlatformWrapper.MetaTrader4DataFeed.QuoteSources
{
    /// <summary>
    /// 
    /// </summary>
    public interface ISymbolObserver
    {
        /// <summary>
        /// 
        /// </summary>
        ReadOnlyCollection<string> ActiveSymbols { get; }

        /// <summary>
        /// 
        /// </summary>
        void AdviseSymbol(string symbol);

        /// <summary>
        /// 
        /// </summary>
        void UnadviseSymbol(string symbol);
    }
}