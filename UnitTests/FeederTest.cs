using System;
using System.Diagnostics;
using System.Threading;
using CPlugin.PlatformWrapper.MetaTrader4DataFeed;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class FeederTest
    {
        [TestMethod]
        public void GetQuotesDuringNextFewSeconds()
        {
            var feed = new Feeder()
            {
                Name = "test feed",
                Path = @"F:\temp\mt4\mt4feeder.feed",
                Server = "10.200.0.3:10443",
                Login = 1002,
                Password = "RtxD4bo_",
                ReconnectErrorsLimit = 1,
                ReconnectTimeout = 1,
                ReconnectRetryTimeout = 1
            };

            "Init".ToConsole();

            // enable if you need only one symbol
            //feed.AdviseSymbol("EURUSD");

            int quotesReceived = 0;
            feed.Started += (sender, args) => { "Started".ToConsole(); };
            feed.ChangeStatus += (sender, args) => { $"Change Status: {args.Status}".ToConsole(); };
            feed.Paused += (sender, args) => { "Paused".ToConsole(); };
            feed.QuoteReceived += (sender, args) => {
                $"Quote Received: {args.Quote.Symbol} {args.Quote.Bid}/{args.Quote.Ask}".ToConsole();
                ++quotesReceived;

                // receive first N quotes then leave
                if (quotesReceived < 5)
                    return;

                "Stop".ToConsole();
                feed.Stop();
            };
            feed.Stopped += (sender, args) => { "Stopped".ToConsole(); };

            "Start".ToConsole();
            var started = DateTime.Now;
            feed.Start();

            // wait until N ticks came
            // but not too infinite time
            while (quotesReceived < 5 && DateTime.Now - started < TimeSpan.FromSeconds(60))
            {
                //"Sleep...".ToConsole();
                Thread.Sleep(TimeSpan.FromSeconds(1));
                //"Sleep done".ToConsole();
            }

            "Stop".ToConsole();
            feed.Stop();
        }
    }

    public static class Extensions
    {
        public static void ToConsole(this string message)
        {
            Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] {message}");
        }
    }
}