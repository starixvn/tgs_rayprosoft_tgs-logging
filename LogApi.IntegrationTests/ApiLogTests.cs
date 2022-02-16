using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NUnit.Framework;
using Tgs.LogApi;
using Tgs.LogApi.MessageQueue;
using Tgs.LogApi.Repositories.Entities;
using Tgs.Logging;
using Tgs.MessageQueue;

namespace LogApi.IntegrationTests;

[TestFixture]
public class ApiLogTests
{
    [Test]
    public async Task Send_ApiLogEvent_By_TgsLog()
    {
        var tcs = new TaskCompletionSource();
        var fakeTgsRepository = Substitute.For<ITgsRepository>();

        using var testWebApp = new TestLogApiApplication(builder => builder.ConfigureServices(
            services => services
                .AddSingleton(sp => ActivatorUtilities.CreateInstance<ApiLogHandler>(sp))
                .AddSingleton(fakeTgsRepository)
                .Configure<LogApiOptions>(options =>
                {
                    options.LogLevel = Tgs.Logging.TgsLogLevel.All;
                    options.UnrestrictedLoggers = Array.Empty<string>();
                })
                .AddFakeNatsMessageQueue()
                .AddSingleton(sp =>
                {
                    var dbContextOptionsBuilder = new DbContextOptionsBuilder<TgsContext>();
                    _ = dbContextOptionsBuilder
                        .UseInMemoryDatabase(Guid.NewGuid().ToString())
                        .ConfigureWarnings(x => x.Ignore(
                            InMemoryEventId.TransactionIgnoredWarning));

                    return new TgsContext(dbContextOptionsBuilder.Options);
                })));

        var handler = testWebApp.Services.GetRequiredService<ApiLogHandler>();

        var fakeMessageSender = Substitute.For<IMessageSender>();

        _ = fakeMessageSender
            .PublishAsync(
                Arg.Any<string>(),
                Arg.Any<ReadOnlyMemory<byte>>(),
                Arg.Any<IEnumerable<MessageHeaderValue>>(),
                Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var data = callInfo.Arg<ReadOnlyMemory<byte>>();

                return new ValueTask(Task.Run(async () =>
                {
                    await handler.HandleAsync(data);
                }));
                ;
            });

        _ = fakeTgsRepository.SaveApiLogsAsync(Arg.Any<IEnumerable<ApiLogEvent>>())
            .Returns(callInfo =>
            {
                tcs.SetResult();
                return ValueTask.CompletedTask;
            });

        var apiLogProcessor = new TgsLogBackgroundProcessor<LogRecordEvent>(
            "LogRecordEvent",
            fakeMessageSender);
        var adminLogProcessor = new TgsLogBackgroundProcessor<AdminLogEvent>(
            "AdminLogEvent",
            fakeMessageSender);
        var logRecordProcessor = new TgsLogBackgroundProcessor<ApiLogEvent>(
            "ApiLogEvent",
            fakeMessageSender);

        var sut = new TgsLog(
            apiLogProcessor,
            adminLogProcessor,
            logRecordProcessor,
            NullLogger<TgsLog>.Instance);

        sut.LogApi(new ApiLogEvent());

        await tcs.Task.ConfigureTimeout(TimeSpan.FromSeconds(3));
    }
}
