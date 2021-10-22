using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NUnit.Framework;
using Tgs.LogApi;
using Tgs.LogApi.MessageQueue;
using Tgs.LogApi.Repositories.Entities;
using Tgs.Logging;
using Tgs.MessageQueue;

namespace LogApi.IntegrationTests
{
	[TestFixture]
	public class ApiLogTests
	{
		[Test]
		public async Task Send_ApiLogEvent_By_TgsLog()
		{
			var tcs = new TaskCompletionSource();
			var fakeTgsRepository = Substitute.For<ITgsRepository>();

			using var testHost = TestSiteHelper.GetLogApiTestHost(services =>
			{
				services
					.AddSingleton(sp => ActivatorUtilities.CreateInstance<ApiLogHandler>(sp))
					.AddSingleton(fakeTgsRepository);
			});

			_ = testHost.StartAsync();

			await Task.Delay(1000);

			var handler = testHost.Services.GetRequiredService<ApiLogHandler>();

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

			fakeTgsRepository.SaveApiLogsAsync(Arg.Any<IEnumerable<ApiLogEvent>>())
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
}
