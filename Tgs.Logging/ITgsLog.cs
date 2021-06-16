using System;
using System.Runtime.CompilerServices;

namespace Tgs.Logging
{
	public interface ITgsLog
	{
		Guid TrackingId { get; set; }

		void LogAdmin(AdminLogEvent adminEvent);

		void LogApi(ApiLogEvent apiCall);

		void LogDebug(
			string message,
			Exception? exception = null,
			[CallerFilePath] string file = "",
			[CallerMemberName] string member = "",
			[CallerLineNumber] int line = 0);

		void LogError(
			string message,
			Exception? exception = null,
			[CallerFilePath] string file = "",
			[CallerMemberName] string member = "",
			[CallerLineNumber] int line = 0);

		void LogFatal(
			string message,
			Exception? exception = null,
			[CallerFilePath] string file = "",
			[CallerMemberName] string member = "",
			[CallerLineNumber] int line = 0);

		void LogInfo(
			string message,
			Exception? exception = null,
			[CallerFilePath] string file = "",
			[CallerMemberName] string member = "",
			[CallerLineNumber] int line = 0);

		void LogTrackingData(string trackingData);

		void LogWarning(
			string message,
			Exception? exception = null,
			[CallerFilePath] string file = "",
			[CallerMemberName] string member = "",
			[CallerLineNumber] int line = 0);

		ITgsLog SetLogger(string name);
	}
}
