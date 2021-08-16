using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading.Tasks;
using MessagePack;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Tgs.LogApi;
using Tgs.LogApi.MessageQueue;
using Tgs.Logging;

namespace LogApi.UnitTests
{
	[TestFixture]
	public class MessageQueueHandlerTests
	{
		[Test]
		public async Task Handle_ApiLog_Message()
		{
			var testSubject = new ApiLogSubject();
			var options = new LogApiOptions();

			var sut = new ApiLogHandler(
				testSubject,
				new OptionsWrapper<LogApiOptions>(options));

			var message = new ApiLogEvent();

			var receiveSubject = new ReplaySubject<ApiLogEvent>();

			using var sub = testSubject.Subscribe(receiveSubject);

			await sut.HandleAsync(MessagePackSerializer.Serialize(
				message,
				options.SerializerOptions));

			receiveSubject.OnCompleted();

			var count = await receiveSubject.Count().ToTask();

			Assert.AreEqual(1, count);
		}

		[Test]
		public async Task Handle_AdminLog_Message()
		{
			var testSubject = new AdminLogSubject();
			var options = new LogApiOptions();

			var sut = new AdminLogHandler(
				testSubject,
				new OptionsWrapper<LogApiOptions>(options));

			var message = new AdminLogEvent();

			var receiveSubject = new ReplaySubject<AdminLogEvent>();

			using var sub = testSubject.Subscribe(receiveSubject);

			await sut.HandleAsync(MessagePackSerializer.Serialize(
				message,
				options.SerializerOptions));

			receiveSubject.OnCompleted();

			var count = await receiveSubject.Count().ToTask();

			Assert.AreEqual(1, count);
		}

		[Test]
		public async Task Handle_LogRecord_Message()
		{
			var testSubject = new LogRecordSubject();
			var options = new LogApiOptions();

			var sut = new LogRecordHandler(
				testSubject,
				new OptionsWrapper<LogApiOptions>(options));

			var message = new LogRecordEvent();

			var receiveSubject = new ReplaySubject<LogRecordEvent>();

			using var sub = testSubject.Subscribe(receiveSubject);

			await sut.HandleAsync(MessagePackSerializer.Serialize(
				message,
				options.SerializerOptions));

			receiveSubject.OnCompleted();

			var count = await receiveSubject.Count().ToTask();

			Assert.AreEqual(1, count);
		}
	}
}
