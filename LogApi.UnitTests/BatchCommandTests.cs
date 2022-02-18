using System.Collections.Generic;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Tgs.LogApi;
using Tgs.LogApi.ApplicationServices;
using Tgs.Logging;

namespace LogApi.UnitTests;

public class BatchCommandTests
{
    [Test]
    public async Task SaveAdminLogBatch_Should_Call_Repository()
    {
        var fakeTgsRepository = Substitute.For<ITgsRepository>();

        var sut = new SaveAdminLogBatchCommandService(fakeTgsRepository);

        var command = new SaveAdminLogBatchCommand(new[] {
                new AdminLogEvent(),
                new AdminLogEvent()
            });

        await sut.ExecuteAsync(command);

        _ = fakeTgsRepository.Received(1)
            .SaveAdminLogsAsync(Arg.Any<IEnumerable<AdminLogEvent>>());
    }

    [Test]
    public async Task SaveApiLogBatch_Should_Call_Repository()
    {
        var fakeTgsRepository = Substitute.For<ITgsRepository>();

        var sut = new SaveApiLogBatchCommandService(fakeTgsRepository);

        var command = new SaveApiLogBatchCommand(new[] {
                new ApiLogEvent(),
                new ApiLogEvent()
            });

        await sut.ExecuteAsync(command);

        _ = fakeTgsRepository.Received(1)
            .SaveApiLogsAsync(Arg.Any<IEnumerable<ApiLogEvent>>());
    }

    [Test]
    public async Task SaveLogRecordBatch_Should_Call_RepositoryAsync()
    {
        var fakeTgsRepository = Substitute.For<ITgsRepository>();

        var sut = new SaveLogRecordBatchCommandService(fakeTgsRepository);

        var command = new SaveLogRecordBatchCommand(new[] {
                new LogRecordEvent(),
                new LogRecordEvent()
            });

        await sut.ExecuteAsync(command);

        _ = fakeTgsRepository.Received(1)
            .SaveLogRecordsAsync(Arg.Any<IEnumerable<LogRecordEvent>>());
    }
}
