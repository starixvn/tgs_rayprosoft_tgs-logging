using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NUnit.Framework;
using Tgs.LogApi.Repositories;
using Tgs.LogApi.Repositories.Entities;
using Tgs.Logging;
using Tgs.NotifyService.Client;
using TGS.NotifyApi.Messages;

namespace LogApi.Repositories.UnitTests
{
    [TestFixture]
    public class TgsRepositoryTests
    {
        [Test]
        public async Task SaveAdminLogs_If_Status_Code_Greater_Or_Equal_500_Will_Send_Notify()
        {
            var dbContextOptionsBuilder = new DbContextOptionsBuilder<TgsContext>();
            dbContextOptionsBuilder
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning));

            var dbContext = new TgsContext(dbContextOptionsBuilder.Options);

            var fakeNotifyClient = Substitute.For<INotifyClient>();
            var logger = NullLogger<TgsRepository>.Instance;

            var sut = new TgsRepository(dbContext, fakeNotifyClient, logger);

            var batch = new[] {
                new AdminLogEvent()
                {
                    ResponseCode = 500
                },
                new AdminLogEvent()
                {
                    ResponseCode = 501
                },
                new AdminLogEvent()
                {
                    ResponseCode = 200
                }
            };

            await sut.SaveAdminLogsAsync(batch);

            _ = fakeNotifyClient.Received(2)
                .SendAsync(Arg.Any<NotifyLogRequest>());
        }

        [Test]
        public async Task SaveApiLogs_If_Status_Code_Greater_Or_Equal_500_Will_Send_Notify()
        {
            var dbContextOptionsBuilder = new DbContextOptionsBuilder<TgsContext>();
            dbContextOptionsBuilder
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning));

            var dbContext = new TgsContext(dbContextOptionsBuilder.Options);

            var fakeNotifyClient = Substitute.For<INotifyClient>();
            var logger = NullLogger<TgsRepository>.Instance;

            var sut = new TgsRepository(dbContext, fakeNotifyClient, logger);

            var batch = new[] {
                new ApiLogEvent()
                {
                    ResponseCode = 500
                },
                new ApiLogEvent()
                {
                    ResponseCode = 501
                },
                new ApiLogEvent()
                {
                    ResponseCode = 200
                }
            };

            await sut.SaveApiLogsAsync(batch);

            _ = fakeNotifyClient.Received(2)
                .SendAsync(Arg.Any<NotifyLogRequest>());
        }
    }
}
