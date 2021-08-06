using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using TGS.NotifyApi.Messages;

namespace Tgs.NotifyService.Client
{
	internal class NotifyClient : INotifyClient
	{
		private readonly HttpClient _client;
		private readonly NotifyClientOptions _options;

		public NotifyClient(HttpClient client, IOptions<NotifyClientOptions> optionsAccessor)
		{
			if (optionsAccessor is null)
				throw new ArgumentNullException(nameof(optionsAccessor));

			_client = client ?? throw new ArgumentNullException(nameof(client));
			_options = optionsAccessor.Value;
		}

		public async ValueTask<NotifyResponse?> SendAsync(NotifyLogRequest request)
		{
			var requestContent = new StringContent(JsonSerializer.Serialize(request));

			requestContent.Headers.ContentType = new MediaTypeHeaderValue(MediaTypeNames.Application.Json);

			var response = await _client
				.PostAsync(_options.Path, requestContent)
				.ConfigureAwait(false);

			var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

			return JsonSerializer.Deserialize<NotifyResponse?>(json);
		}
	}
}
