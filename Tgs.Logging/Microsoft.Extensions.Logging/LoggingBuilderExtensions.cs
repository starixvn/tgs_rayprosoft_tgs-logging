using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Tgs.Logging;

namespace Microsoft.Extensions.Logging
{
	public static class LoggingBuilderExtensions
	{
		public static ILoggingBuilder AddTgsLog(this ILoggingBuilder builder)
		{
			builder.Services.TryAddEnumerable(
				ServiceDescriptor.Singleton<ILoggerProvider, TgsLoggerProvider>());

			builder.Services
				.Add(ServiceDescriptor.Singleton(typeof(TgsLogBackgroundProcessor<>)));

			builder.Services
				.AddTransient<ITgsLog, TgsLog>();

			return builder;
		}
	}
}
