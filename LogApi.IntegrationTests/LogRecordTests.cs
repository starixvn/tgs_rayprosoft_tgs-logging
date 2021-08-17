using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using NUnit.Framework;
using Tgs.LogApi;
using Tgs.LogApi.MessageQueue;
using Tgs.Logging;
using Tgs.MessageQueue;

namespace LogApi.IntegrationTests
{
    [TestFixture]
    public class LogRecordTests
    {
        [Test]
        public async Task Send_LogRecordEvent_By_Logger()
        {
            var tcs = new TaskCompletionSource();
            var fakeTgsRepository = Substitute.For<ITgsRepository>();

            using var testHost = TestSiteHelper.GetLogApiTestHost(services =>
            {
                services
                    .AddSingleton(sp => ActivatorUtilities.CreateInstance<LogRecordHandler>(sp))
                    .AddSingleton(fakeTgsRepository);
            });

            _ = testHost.StartAsync();

            await Task.Delay(1000);

            var handler = testHost.Services.GetRequiredService<LogRecordHandler>();

            var fakeMessageSender = Substitute.For<IMessageSender>();

            fakeMessageSender
                .PublishAsync(Arg.Any<string>(), Arg.Any<byte[]>())
                .Returns(callInfo =>
                {
                    var data = callInfo.Arg<byte[]>();

                    handler.HandleAsync(data).GetAwaiter().GetResult();

                    return ValueTask.CompletedTask;
                });

            fakeTgsRepository.SaveLogRecordsAsync(Arg.Any<IEnumerable<LogRecordEvent>>())
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

            using var testHost = TestSiteHelper.GetLogApiTestHost(services =>
            {
                services
                    .AddSingleton(sp => ActivatorUtilities.CreateInstance<LogRecordHandler>(sp))
                    .AddSingleton(fakeTgsRepository);
            });

            _ = testHost.StartAsync();

            await Task.Delay(1000);

            var handler = testHost.Services.GetRequiredService<LogRecordHandler>();

            var fakeMessageSender = Substitute.For<IMessageSender>();

            fakeMessageSender
                .PublishAsync(Arg.Any<string>(), Arg.Any<byte[]>())
                .Returns(callInfo =>
                {
                    var data = callInfo.Arg<byte[]>();

                    handler.HandleAsync(data).GetAwaiter().GetResult();

                    return ValueTask.CompletedTask;
                });

            fakeTgsRepository.SaveLogRecordsAsync(Arg.Any<IEnumerable<LogRecordEvent>>())
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
                apiLogProcessor);

            sut.SetLogger("test");

            sut.LogInfo("test", null, "file", "member", 0);

            await tcs.Task.ConfigureTimeout(TimeSpan.FromSeconds(3));
        }
    }
}
