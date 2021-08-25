using System.Threading.Tasks;
using TGS.NotifyApi.Messages;

namespace Tgs.NotifyService.Client
{
	public interface INotifyClient
	{
		ValueTask<NotifyResponse?> SendAsync(NotifyLogRequest request);
	}
}
