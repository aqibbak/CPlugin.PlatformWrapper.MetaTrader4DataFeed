using System;
using System.Threading;
using CPlugin.PlatformWrapper.MetaTrader4DataFeed;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class FeederTest
    {
        [TestMethod]
        public void GetQuotesDuring5Seconds()
        {
            var feed = new Feeder()
            {
                Path = @"F:\temp\mt4\mt4feeder.feed",
                Server = "222.222.0.1:1443",
                Login = 100,
                Password = "KQy5lay"
            };

            "Init".ToConsole();

            feed.AdviseSymbol("EURUSD");

            feed.Started += (sender, args) => { "Started".ToConsole(); };
            feed.ChangeStatus += (sender, args) => { $"Change Status: {args.Status}".ToConsole(); };
            feed.Paused += (sender, args) => { "Paused".ToConsole(); };
            feed.QuoteReceived += (sender, args) => { $"Quote Received: {args.Quote.Symbol} {args.Quote.Bid}/{args.Quote.Bid}".ToConsole(); };
            feed.Stopped += (sender, args) => { "Stopped".ToConsole(); };

            "Start".ToConsole();
            feed.Start();

            "Sleep...".ToConsole();
            Thread.Sleep(TimeSpan.FromSeconds(5));
            "Sleep done".ToConsole();

            "Stop".ToConsole();
            feed.Stop();
        }
    }

    public static class Extensions
    {
        public static void ToConsole(this string message)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] {message}");
        }
    }
}