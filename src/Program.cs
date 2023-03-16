namespace PingWebhook;

internal class Program
{
    public static async Task<int> Main(string[] args)
    {
        var options = Parser.Default.ParseArguments<Options>(args);
        if (options.Errors.Any())
            return 1;

        using var monitor = new PingMonitor(options.Value);
        await monitor.RunAsync();
        return 0;
    }
}