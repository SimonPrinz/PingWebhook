namespace PingWebhook;

public class Options
{
    [Option("ntfyBaseUrl", Required = true)]
    public string NtfyBaseUrl { get; set; } = null!;

    [Option("ntfyTopic", Required = true)]
    public string NtfyTopic { get; set; } = null!;

    [Option("ntfyAuthentication", Required = false)]
    public string? NtfyAuthentication { get; set; }

    [Option("checkInterval", Default = 5000)]
    public int CheckInterval { get; set; }

    [Option("checkTimeout", Default = 3000)]
    public int CheckTimeout { get; set; }

    [Option("title", Required = false)]
    public string? Title { get; set; }

    [Option('v', "verbose", Required = false, Default = false)]
    public bool Verbose { get; set; }

    [Value(0, Required = true)]
    public string IpAddress { get; set; } = null!;
}
