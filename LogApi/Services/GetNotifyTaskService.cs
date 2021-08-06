using System.Threading.Tasks;
using Tgs.NotifyService.Client;
using TGS.Apis.LogApi.Clients;
using TGS.Core.DataServices.Tgs.Models;
using TGS.NotifyApi.Messages;

namespace TGS.Apis.LogApi.Services
{
	public interface IGetNotifyTaskService
	{
		ValueTask<NotifyResponse> GetNotifyTaskAsync(AdminLog log);
		ValueTask<NotifyResponse> GetNotifyTaskAsync(ApiLog log);
	}

	public class GetNotifyTaskService : IGetNotifyTaskService
	{
		private readonly INotifyClient _alertClient;

		public GetNotifyTaskService(INotifyClient alertClient)
		{
			_alertClient = alertClient;
		}

		ValueTask<NotifyResponse> IGetNotifyTaskService.GetNotifyTaskAsync(AdminLog log)
		{
			var request = new NotifyLogRequest
			{
				Type = typeof(AdminLog).Name,
				Id = log.Id,
				Path = log.Path,
				Method = log.Method,
				Prefix = log.Method,
				Exception = log.Exception,
				Stacktrace = log.StackTrace,
				ServerName = log.ServerName,
				ResponseCode = log.ResponseCode,
				RecordedOn = log.RecordedOn
			};

			return _alertClient.SendAsync(request);
		}

		ValueTask<NotifyResponse> IGetNotifyTaskService.GetNotifyTaskAsync(ApiLog log)
		{
			var request = new NotifyLogRequest
			{
				Type = typeof(ApiLog).Name,
				Id = log.Id,
				Path = log.Path,
				Method = log.Method,
				Prefix = log.Method,
				ServerName = log.ServerName,
				ResponseCode = log.ResponseCode,
				RecordedOn = log.RecordedOn
			};

			return _alertClient.SendAsync(request);
		}
	}
}