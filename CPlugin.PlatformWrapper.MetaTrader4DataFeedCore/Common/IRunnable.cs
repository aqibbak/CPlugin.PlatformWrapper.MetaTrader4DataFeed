using System;

namespace CPlugin.PlatformWrapper.MetaTrader4DataFeed.Common
{
    /// <summary>
    /// 
    /// </summary>
    public interface IRunnable
    {
        /// <summary>
        /// 
        /// </summary>
        bool CanPaused { get; }

        /// <summary>
        /// 
        /// </summary>
        bool CanResumed { get; }

        /// <summary>
        /// 
        /// </summary>
        RunningStatus Status { get; }

        /// <summary>
        /// 
        /// </summary>
        void Start();

        /// <summary>
        /// 
        /// </summary>
        void Stop(TimeSpan? ts);

        /// <summary>
        /// 
        /// </summary>
        void Pause();

        /// <summary>
        /// 
        /// </summary>
        void Resume();
    }
}