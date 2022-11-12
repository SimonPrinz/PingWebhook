using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PingWebhook
{
	public class NtfyClient : IDisposable
	{
		private readonly HttpClient _Client;

		public NtfyClient(string pBaseUrl, string? pBasicAuthString = null)
		{
			_Client = new HttpClient
			{
				BaseAddress = new Uri(pBaseUrl),
			};
			if (pBasicAuthString != null)
				_Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
					"Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(pBasicAuthString)));
		}

		public async Task PublishAsync(string pTopic, string pBody, string? pTitle = null, ePriority? pPriority = null, string[]? pTags = null, CancellationToken pCancellationToken = default)
		{
			using HttpRequestMessage lRequest = new(HttpMethod.Post, pTopic)
			{
				Content = new StringContent(pBody, Encoding.UTF8, "text/plain"),
			};
			if (pTitle != null) lRequest.Headers.Add("Title", pTitle);
			if (pPriority != null) lRequest.Headers.Add("Priority", pPriority.Value.ToString("G").ToLower());
			if (pTags is { Length: > 0 }) lRequest.Headers.Add("Tags", string.Join(',', pTags));

			using HttpResponseMessage lResponse = await _Client.SendAsync(lRequest, pCancellationToken);
			lResponse.EnsureSuccessStatusCode();
		}

		public void Dispose()
		{
			_Client.Dispose();
		}

		public enum ePriority
		{
			Min,
			Low,
			Default,
			High,
			Max,
		}
	}
}