using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CPlugin.PlatformWrapper.MetaTrader4DataFeed;

namespace Debug
{
    class Program
    {
        public static void Main(string[] cmdLineArgs)
        {
            Feeder _feed = new Feeder()
            {
                Name = "FxPro Demo5",
                Path = @"F:\temp\mt4\mt4feeder.feed",
                Server = "demo5-london.fxpro.com:443",
                Login = 8035138,
                Password = "b7Bx9Vd5",
                ReconnectErrorsLimit = 1,
                ReconnectTimeout = 1,
                ReconnectRetryTimeout = 1,
                ReadErrorsLimit = 1
            };

            "Init".ToConsole();

            // enable if you need only one symbol
            //feed.AdviseSymbol("EURUSD");

            int quotesReceived = 0;
            _feed.Started += (sender, args) => { "Started".ToConsole(); };
            _feed.ChangeStatus += (sender, args) => { $"Change Status: {args.Status}".ToConsole(); };
            _feed.Paused += (sender, args) => { "Paused".ToConsole(); };
            _feed.QuoteReceived += (sender, args) => {

                if(quotesReceived % 1000 == 0)
                    $"quotesReceived: {quotesReceived}".ToConsole();

                ++quotesReceived;

                // receive first N quotes then leave
                if (quotesReceived < 10000)
                    return;

                "Stop".ToConsole();
                _feed.Stop();
            };
            _feed.Stopped += (sender, args) => { "Stopped".ToConsole(); };

            "Start".ToConsole();
            var started = DateTime.Now;
            _feed.Start();

            // wait until N ticks came
            // but not too infinite time
            while (quotesReceived < 10000 && DateTime.Now - started < TimeSpan.FromMinutes(3))
            {
                //"Sleep...".ToConsole();
                Thread.Sleep(TimeSpan.FromSeconds(1));
                //"Sleep done".ToConsole();
            }

            "Stop".ToConsole();
            _feed.Stop();
        }
    }

    public static class Extensions
    {
        public static void ToConsole(this string message)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] {message}");
            System.Diagnostics.Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] {message}");
        }
    }
}
