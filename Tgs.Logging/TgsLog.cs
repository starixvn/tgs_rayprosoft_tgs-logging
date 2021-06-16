using System;
using System.Threading;

namespace Tgs.Logging
{
	internal class TgsLog : ITgsLog
	{
		private readonly AsyncLocal<Guid> _asyncLocalTrackingId = new AsyncLocal<Guid>();
		private readonly TgsLogBackgroundProcessor<LogRecordEvent> _logRecordEventProcessor;
		private readonly TgsLogBackgroundProcessor<AdminLogEvent> _adminEventProcessor;
		private readonly TgsLogBackgroundProcessor<ApiLogEvent> _apiLogEventProcessor;
		private string _loggerName = string.Empty;

		public Guid TrackingId
		{
			get => _asyncLocalTrackingId.Value;

			set
			{
				var slot = _asyncLocalTrackingId.Value;
				if (slot == Guid.Empty)
					_asyncLocalTrackingId.Value = value;
			}
		}

		public TgsLog(
			TgsLogBackgroundProcessor<LogRecordEvent> logRecordEventProcessor,
			TgsLogBackgroundProcessor<AdminLogEvent> adminEventProcessor,
			TgsLogBackgroundProcessor<ApiLogEvent> apiLogEventProcessor)
		{
			_logRecordEventProcessor = logRecordEventProcessor ?? throw new ArgumentNullException(nameof(logRecordEventProcessor));
			_adminEventProcessor = adminEventProcessor ?? throw new ArgumentNullException(nameof(adminEventProcessor));
			_apiLogEventProcessor = apiLogEventProcessor ?? throw new ArgumentNullException(nameof(apiLogEventProcessor));
		}

		public void LogAdmin(AdminLogEvent adminEvent)
		{
			adminEvent.TrackingId = TrackingId;
			_adminEventProcessor.AddRecord(adminEvent);
		}

		public void LogApi(ApiLogEvent apiLogEvent)
		{
			apiLogEvent.TrackingId = TrackingId;
			_apiLogEventProcessor.AddRecord(apiLogEvent);
		}

		public void LogDebug(
			string message,
			Exception? exception,
			string file,
			string member,
			int line)
			=> SendLogRecord(
				TgsLogLevel.Debug,
				message,
				exception,
				file,
				member,
				line);

		public void LogError(
			string message,
			Exception? exception,
			string file,
			string member,
			int line)
			=> SendLogRecord(
				TgsLogLevel.Error,
				message,
				exception,
				file,
				member,
				line);

		public void LogFatal(
			string message,
			Exception? exception,
			string file,
			string member,
			int line)
			=> SendLogRecord(
				TgsLogLevel.Fatal,
				message,
				exception,
				file,
				member,
				line);

		public void LogInfo(
			string message,
			Exception? exception,
			string file,
			string member,
			int line)
			=> SendLogRecord(
				TgsLogLevel.Info,
				message,
				exception,
				file,
				member,
				line);

		public void LogTrackingData(string trackingData)
			=> _logRecordEventProcessor.AddRecord(new LogRecordEvent
			{
				TrackingId = TrackingId,
				RecordedOn = DateTimeOffset.UtcNow,
				ServerName = Environment.MachineName,
				Level = TgsLogLevel.Info,
				Logger = trackingData
			});

		public void LogWarning(
			string message,
			Exception? exception,
			string file,
			string member,
			int line)
			=> SendLogRecord(
				TgsLogLevel.Warning,
				message,
				exception,
				file,
				member,
				line);

		public ITgsLog SetLogger(string name)
		{
			_loggerName = name;
			return this;
		}

		private void SendLogRecord(
			TgsLogLevel level,
			string message,
			Exception? exception,
			string callerFile,
			string callerMember,
			int lineNumber)
		{
			_logRecordEventProcessor.AddRecord(new LogRecordEvent
			{
				TrackingId = TrackingId,
				RecordedOn = DateTimeOffset.UtcNow,
				ServerName = Environment.MachineName,
				Level = level,
				Logger = _loggerName,
				Message = message,
				Exception = exception?.ToString(),
				File = callerFile,
				Member = callerMember,
				Line = lineNumber
			});
		}
	}
}
