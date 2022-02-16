using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using Tgs.MessageQueue;

namespace Tgs.Logging.UnitTests;

[TestFixture]
public class TgsLoggerTests
{
    [Test]
    public async Task Log_Record_Should_Be_Sent()
    {
        var fakeMessageSender = Substitute.For<IMessageSender>();
        var sut = new TgsLogger(
            "Test",
            new TgsLogBackgroundProcessor<LogRecordEvent>(
                "subject",
                fakeMessageSender));

        var tcs = new TaskCompletionSource<bool>();
        fakeMessageSender
            .When(fake => fake.PublishAsync(Arg.Any<string>(), Arg.Any<ReadOnlyMemory<byte>>()))
            .Do(Callback.Always(callInfo =>
            {
                tcs.SetResult(true);
            }));

        sut.LogInformation("test");

        _ = await tcs.Task.ConfigureTimeout(TimeSpan.FromSeconds(3));

        Assert.Pass();
    }
}
