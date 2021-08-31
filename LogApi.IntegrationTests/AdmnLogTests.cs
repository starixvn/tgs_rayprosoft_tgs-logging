using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NUnit.Framework;
using Tgs.LogApi;
using Tgs.LogApi.MessageQueue;
using Tgs.Logging;
using Tgs.MessageQueue;

namespace LogApi.IntegrationTests
{
    [TestFixture]
    public class AdmnLogTests
    {
        [Test]
        public async Task Send_AdminLogEvent_By_TgsLog()
        {
            var tcs = new TaskCompletionSource();
            var fakeTgsRepository = Substitute.For<ITgsRepository>();

            using var testHost = TestSiteHelper.GetLogApiTestHost(services =>
            {
                services
                    .AddSingleton(sp => ActivatorUtilities.CreateInstance<AdminLogHandler>(sp))
                    .AddSingleton(fakeTgsRepository);
            });

            _ = testHost.StartAsync();

            await Task.Delay(1000);

            var handler = testHost.Services.GetRequiredService<AdminLogHandler>();

            var fakeMessageSender = Substitute.For<IMessageSender>();

            fakeMessageSender
                .PublishAsync(
                    Arg.Any<string>(),
                    Arg.Any<byte[]>(),
                    Arg.Any<IEnumerable<MessageHeaderValue>>())
                .Returns(callInfo =>
                {
                    var data = callInfo.Arg<byte[]>();

                    return new ValueTask(Task.Run(async () =>
                    {
                        await handler.HandleAsync(data);
                    }));
                    ;
                });

            fakeTgsRepository.SaveAdminLogsAsync(Arg.Any<IEnumerable<AdminLogEvent>>())
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
                logRecordProcessor);

            sut.LogAdmin(new AdminLogEvent());

            await tcs.Task.ConfigureTimeout(TimeSpan.FromSeconds(3));
        }
    }
}
