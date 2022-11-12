using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace PingWebhook
{
	internal class Program : IDisposable
	{
		public static void Main(string[] pArgs)
		{
			Parser.Default.ParseArguments<Options>(pArgs)
				.WithParsedAsync(pOptions =>
				{
					using Program lProgram = new(pOptions);
					return lProgram.Run();
				})
				.GetAwaiter().GetResult();
		}

		public class Options
		{
			[Option("ntfyBaseUrl", Required = true)]
			public string NtfyBaseUrl { get; set; } = null!;

			[Option("ntfyTopic", Required = true)]
			public string NtfyTopic { get; set; } = null!;

			[Option("ntfyAuthentication", Required = false)]
			public string? NtfyAuthentication { get; set; }
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
			await _Client.PublishAsync(_Options.NtfyTopic, "Hello, World!");
		}

		public void Dispose()
		{
			_Client.Dispose();
		}
	}
}