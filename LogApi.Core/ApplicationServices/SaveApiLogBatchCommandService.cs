using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;

namespace Tgs.LogApi.ApplicationServices
{
	internal class SaveApiLogBatchCommandService : ICommandService<SaveApiLogBatchCommand>
	{
		private readonly ITgsRepository _tgsRepository;

		public SaveApiLogBatchCommandService(ITgsRepository tgsRepository)
		{
			_tgsRepository = tgsRepository ?? throw new ArgumentNullException(nameof(tgsRepository));
		}

		public ValueTask ExecuteAsync(
			SaveApiLogBatchCommand command,
			CancellationToken cancellationToken = default)
			=> _tgsRepository.SaveApiLogsAsync(command.LogEvents);
	}
}
