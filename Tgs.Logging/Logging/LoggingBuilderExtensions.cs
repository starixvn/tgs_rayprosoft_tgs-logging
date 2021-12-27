using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging.Configuration;
using Tgs.Logging;
using Tgs.MessageQueue;

namespace Microsoft.Extensions.Logging
{
	public static class LoggingBuilderExtensions
	{
		public static ILoggingBuilder AddTgsLog(this ILoggingBuilder builder)
		{
			builder.Services.TryAddEnumerable(
				ServiceDescriptor.Singleton<ILoggerProvider, TgsLoggerProvider>());

			LoggerProviderOptions.RegisterProviderOptions<TgsLoggerOptions, TgsLoggerProvider>(builder.Services);

			builder.Services
				.AddSingleton(
					sp => new TgsLogBackgroundProcessor<LogRecordEvent>(
						"LogRecordEvent",
						sp.GetRequiredService<IMessageSender>()))
				.AddSingleton(
					sp => new TgsLogBackgroundProcessor<AdminLogEvent>(
						"AdminLogEvent",
						sp.GetRequiredService<IMessageSender>()))
				.AddSingleton(
					sp => new TgsLogBackgroundProcessor<ApiLogEvent>(
						"ApiLogEvent",
						sp.GetRequiredService<IMessageSender>()));


			builder.Services
				.AddTransient<ITgsLog, TgsLog>();

			return builder;
		}
	}
}
