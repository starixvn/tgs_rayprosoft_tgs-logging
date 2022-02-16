using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;

namespace Tgs.LogApi.ApplicationServices;

internal class SaveAdminLogBatchCommandService : ICommandService<SaveAdminLogBatchCommand>
{
    private readonly ITgsRepository _tgsRepository;

    public SaveAdminLogBatchCommandService(ITgsRepository tgsRepository)
    {
        _tgsRepository = tgsRepository ?? throw new ArgumentNullException(nameof(tgsRepository));
    }

    public ValueTask ExecuteAsync(SaveAdminLogBatchCommand command, CancellationToken cancellationToken = default)
        => _tgsRepository.SaveAdminLogsAsync(command.LogEvents);
}
