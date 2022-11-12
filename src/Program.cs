using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;

namespace PingWebhook
{
	internal class Program : IDisposable
	{
		public static int Main(string[] pArgs)
		{
			var lOptions = Parser.Default.ParseArguments<Options>(pArgs);
			if (lOptions.Errors.Any())
				return 1;
			using Program lProgram = new(lOptions.Value);
			lProgram.Run().GetAwaiter().GetResult();
			return 0;
		}

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

		private readonly Options _Options;
		private readonly NtfyClient _Client;

		private Program(Options pOptions)
		{
			_Options = pOptions;

			_Client = new NtfyClient(_Options.NtfyBaseUrl, _Options.NtfyAuthentication);
		}

		private async Task Run()
		{
			IPStatus lStatus = IPStatus.Unknown;
			while (true)
			{
				if (_Options.Verbose) Console.WriteLine($"Checking status of {_Options.IpAddress} with a timeout of {_Options.CheckTimeout}ms.");
				try
				{
					using Ping lPing = new();
					PingReply lReply = await lPing.SendPingAsync(_Options.IpAddress, _Options.CheckTimeout);
					if (_Options.Verbose) Console.WriteLine($"Received status {lReply.Status:G} in {lReply.RoundtripTime:D}ms.");
					if (lStatus != lReply.Status)
					{
						if (_Options.Verbose) Console.WriteLine($"Previous status was {lStatus:G}, sending update.");
						lStatus = lReply.Status;
						await SendUpdateAsync(lStatus);
					}
				}
				catch (Exception lException)
				{
					if (_Options.Verbose) Console.WriteLine($"An error occured while pinging: {lException.Message}");
					if (lStatus != IPStatus.Unknown)
					{
						if (_Options.Verbose) Console.WriteLine($"Previous status was {lStatus:G}, sending update.");
						lStatus = IPStatus.Unknown;
						await SendUpdateAsync(lStatus);
					}
				}

				if (_Options.Verbose) Console.WriteLine($"Waiting {_Options.CheckInterval}ms before checking again.");
				Thread.Sleep(_Options.CheckInterval);
			}
		}

		private string Title => _Options.Title ?? _Options.IpAddress;

		private async Task SendUpdateAsync(IPStatus pStatus)
		{
			string lBody;
			NtfyClient.ePriority lPriority = NtfyClient.ePriority.Default;
			List<string> lTags = new();
			
			switch (pStatus)
			{
				case IPStatus.Success:
					lBody = $"{Title} is online!";
					lTags.Add("white_check_mark");
					break;
				case IPStatus.TimedOut:
					lBody = $"{Title} is offline!";
					lTags.Add("exclamation");
					break;
				case IPStatus.TimeExceeded:
					lBody = $"{Title} took to long to respond!";
					lTags.Add("hourglass_flowing_sand");
					break;
				case IPStatus.Unknown:
					lBody = $"{Title} has an unknown state!";
					lTags.Add("scream");
					break;
				default:
					lBody = $"Unknown status {pStatus:G}, please add to {nameof(SendUpdateAsync)}.";
					lTags.Add("question");
					break;
			}
			
			await _Client.PublishAsync(
				_Options.NtfyTopic,
				lBody,
				$"PingWebhook: {Title}",
				lPriority,
				lTags.ToArray());
		}

		public void Dispose()
		{
			_Client.Dispose();
		}

		private enum eStatus
		{
			Online,
			Offline,
			Timeout,
		}
	}
}