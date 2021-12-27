using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;

namespace Tgs.LogApi.ApplicationServices
{
	internal class SaveLogRecordBatchCommandService : ICommandService<SaveLogRecordBatchCommand>
	{
		private readonly ITgsRepository _tgsRepository;

		public SaveLogRecordBatchCommandService(ITgsRepository tgsRepository)
		{
			_tgsRepository = tgsRepository ?? throw new ArgumentNullException(nameof(tgsRepository));
		}

		public ValueTask ExecuteAsync(SaveLogRecordBatchCommand command, CancellationToken cancellationToken = default)
			=> _tgsRepository.SaveLogRecordsAsync(command.LogEvents);
	}
}
