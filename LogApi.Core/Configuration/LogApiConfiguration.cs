using System;
using Microsoft.Extensions.DependencyInjection;

namespace Tgs.LogApi.Configuration
{
	public class LogApiConfiguration
	{
		public IServiceCollection Services { get; }

		public LogApiConfiguration(IServiceCollection services)
		{
			Services = services ?? throw new ArgumentNullException(nameof(services));
		}

		public LogApiConfiguration AddTgsRepository<TRepository>()
			where TRepository : class, ITgsRepository
		{
			Services.AddTransient<ITgsRepository, TRepository>();

			return this;
		}
	}
}
