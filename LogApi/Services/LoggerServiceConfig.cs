using System;
using Microsoft.Extensions.Hosting;
using TGS.Core.Infrastructures.Log.Enums;

namespace TGS.Apis.LogApi.Services
{
    public class LoggerServiceConfig
    {
        public TimeSpan BatchInsertWaitingSeconds { set; get; }
        public int BatchInsertCounts { set; get; }
        public TgsLogLevel LogLevel { set; get; }
        public string[] UnrestrictedLoggers { set; get; }
        public IHostEnvironment HostEnvironment { set; get; }
    }
}
