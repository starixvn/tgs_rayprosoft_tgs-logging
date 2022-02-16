using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using NUnit.Framework;
using Tgs.LogApi;
using Tgs.LogApi.MessageQueue;
using Tgs.LogApi.Repositories.Entities;
using Tgs.Logging;
using Tgs.MessageQueue;

namespace LogApi.IntegrationTests;

[TestFixture]
public class LogRecordTests
{
    [Test]
    public async Task Send_LogRecordEvent_By_Logger()
    {
        var tcs = new TaskCompletionSource();
        var fakeTgsRepository = Substitute.For<ITgsRepository>();

        using var testWebApp = new TestLogApiApplication(builder => builder.ConfigureServices(
            services => services
                .AddSingleton(sp => ActivatorUtilities.CreateInstance<LogRecordHandler>(sp))
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

        var handler = testWebApp.Services.GetRequiredService<LogRecordHandler>();

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

                handler.HandleAsync(data).GetAwaiter().GetResult();

                return ValueTask.CompletedTask;
            });

        _ = fakeTgsRepository.SaveLogRecordsAsync(Arg.Any<IEnumerable<LogRecordEvent>>())
            .Returns(callInfo =>
            {
                tcs.SetResult();
                return ValueTask.CompletedTask;
            });

        var logRecordProcessor = new TgsLogBackgroundProcessor<LogRecordEvent>(
            "LogRecordEvent",
            fakeMessageSender);
        var fakeOptionsMonitor = Substitute.For<IOptionsMonitor<TgsLoggerOptions>>();

        var loggerProvider = new TgsLoggerProvider(
            logRecordProcessor,
            fakeOptionsMonitor);

        var sut = loggerProvider.CreateLogger("test");

        sut.LogInformation("test");

        await tcs.Task.ConfigureTimeout(TimeSpan.FromSeconds(3));
    }

    [Test]
    public async Task Send_LogRecordEvent_By_TgsLog()
    {
        var tcs = new TaskCompletionSource();
        var fakeTgsRepository = Substitute.For<ITgsRepository>();

        using var testWebApp = new TestLogApiApplication(builder => builder.ConfigureServices(
            services => services
                .AddSingleton(sp => ActivatorUtilities.CreateInstance<LogRecordHandler>(sp))
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

        var handler = testWebApp.Services.GetRequiredService<LogRecordHandler>();

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

                handler.HandleAsync(data).GetAwaiter().GetResult();

                return ValueTask.CompletedTask;
            });

        _ = fakeTgsRepository.SaveLogRecordsAsync(Arg.Any<IEnumerable<LogRecordEvent>>())
            .Returns(callInfo =>
            {
                tcs.SetResult();
                return ValueTask.CompletedTask;
            });

        var logRecordProcessor = new TgsLogBackgroundProcessor<LogRecordEvent>(
            "LogRecordEvent",
            fakeMessageSender);
        var adminLogProcessor = new TgsLogBackgroundProcessor<AdminLogEvent>(
            "AdminLogEvent",
            fakeMessageSender);
        var apiLogProcessor = new TgsLogBackgroundProcessor<ApiLogEvent>(
            "ApiLogEvent",
            fakeMessageSender);

        var sut = new TgsLog(
            logRecordProcessor,
            adminLogProcessor,
            apiLogProcessor,
            NullLogger<TgsLog>.Instance);

        _ = sut.SetLogger("test");

        sut.LogInfo("test", null, "file", "member", 0);

        await tcs.Task.ConfigureTimeout(TimeSpan.FromSeconds(3));
    }
}
