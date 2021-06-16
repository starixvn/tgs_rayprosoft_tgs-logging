using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Tgs.Logging
{
	[ProviderAlias("TgsLog")]
	internal class TgsLoggerProvider : ILoggerProvider
	{
		private readonly ConcurrentDictionary<string, TgsLogger> _loggers = new();
		private readonly TgsLogBackgroundProcessor<LogRecordEvent> _tgsLogBackgroundProcessor;

		public TgsLoggerProvider(TgsLogBackgroundProcessor<LogRecordEvent> tgsLogBackgroundProcessor)
		{
			_tgsLogBackgroundProcessor = tgsLogBackgroundProcessor ?? throw new System.ArgumentNullException(nameof(tgsLogBackgroundProcessor));
		}

		public ILogger CreateLogger(string categoryName)
			=> _loggers.GetOrAdd(
				categoryName,
				name => new TgsLogger(categoryName, _tgsLogBackgroundProcessor));

		public void Dispose() => _loggers.Clear();
	}
}
