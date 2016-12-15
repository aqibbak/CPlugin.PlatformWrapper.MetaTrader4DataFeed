# What is it?

This is a .NET wrapper over native MetaQuotes MetaTrader4  datafeed files like *mt4feeder.feed*.

By using it you can get real-time quotes stream from any MT4 server very easily (using only client account) in any .NET language like C# or VB.NET.

## Usage

```cs
// create new instance
var feed = new Feeder()
{
    Path = @"F:\temp\mt4\mt4feeder.feed",
    Server = "127.0.0.1:443",
    Login = 123,
    Password = "***"
};

// we need only EURUSD price
feed.AdviseSymbol("EURUSD");

feed.Started += (sender, args) => { "Started".ToConsole(); };
feed.ChangeStatus += (sender, args) => { $"Change Status: {args.Status}".ToConsole(); };
feed.Paused += (sender, args) => { "Paused".ToConsole(); };
feed.QuoteReceived += (sender, args) => { $"Quote Received: {args.Quote.Symbol} {args.Quote.Bid}/{args.Quote.Bid}".ToConsole(); };
feed.Stopped += (sender, args) => { "Stopped".ToConsole(); };

// start feed
feed.Start();

// simulate long running
Thread.Sleep(TimeSpan.FromSeconds(5));

// ok, leave
feed.Stop();

public static class Extensions
{
    public static void ToConsole(this string message) => Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] {message}");
}
```

# Installation
You can add reference to this project using any of this package:
`CPlugin.PlatformWrapper.MetaTrader4DataFeed` or 
`CPlugin.PlatformWrapper.MetaTrader4DataFeedCore`
