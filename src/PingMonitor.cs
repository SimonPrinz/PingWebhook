namespace PingWebhook;

public class PingMonitor : IDisposable
{
    private readonly Options _options;
    private readonly NtfyClient _client;

    public PingMonitor(Options options)
    {
        _options = options;
        _client = new NtfyClient(_options.NtfyBaseUrl, _options.NtfyAuthentication);
    }

    public async Task RunAsync()
    {
        IPStatus status = IPStatus.Unknown;
        while (true)
        {
            LogVerbose($"Checking status of {_options.IpAddress} with a timeout of {_options.CheckTimeout}ms.");
            try
            {
                using Ping ping = new();
                PingReply reply = await ping.SendPingAsync(_options.IpAddress, _options.CheckTimeout);
                LogVerbose($"Received status {reply.Status:G} in {reply.RoundtripTime:D}ms.");
                if (status != reply.Status)
                {
                    LogVerbose($"Previous status was {status:G}, sending update.");
                    status = reply.Status;
                    await SendUpdateAsync(status);
                }
            }
            catch (Exception exception)
            {
                LogVerbose($"An error occurred while pinging: {exception.Message}");
                if (status != IPStatus.Unknown)
                {
                    LogVerbose($"Previous status was {status:G}, sending update.");
                    status = IPStatus.Unknown;
                    await SendUpdateAsync(status);
                }
            }

            LogVerbose($"Waiting {_options.CheckInterval}ms before checking again.");
            Thread.Sleep(_options.CheckInterval);
        }
    }

    private void LogVerbose(string message)
    {
        if (_options.Verbose)
            Console.WriteLine(message);
    }

    private string Title => _options.Title ?? _options.IpAddress;

    private async Task SendUpdateAsync(IPStatus status)
    {
        string body;
        NtfyClient.Priority priority = NtfyClient.Priority.Default;
        List<string> tags = new();

        switch (status)
        {
            case IPStatus.Success:
                body = $"{Title} is online!";
                tags.Add("white_check_mark");
                break;
            case IPStatus.TimedOut:
                body = $"{Title} is offline!";
                tags.Add("exclamation");
                break;
            case IPStatus.TimeExceeded:
                body = $"{Title} took too long to respond!";
                tags.Add("hourglass_flowing_sand");
                break;
            case IPStatus.Unknown:
                body = $"{Title} has an unknown state!";
                tags.Add("scream");
                break;
            default:
                body = $"Unknown status {status:G}, please add to {nameof(SendUpdateAsync)}.";
                tags.Add("question");
                break;
        }

        await _client.PublishAsync(
            _options.NtfyTopic,
            body,
            $"PingWebhook: {Title}",
            priority,
            tags.ToArray());
    }

    public void Dispose()
    {
        _client.Dispose();
    }
}