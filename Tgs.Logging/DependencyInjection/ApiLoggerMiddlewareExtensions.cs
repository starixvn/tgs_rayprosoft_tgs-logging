using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Tgs.Logging.DependencyInjection
{
    public static class ApiLoggerMiddlewareExtensions
    {
        public static IApplicationBuilder UseTgsApiLoggerMiddleware(
            this IApplicationBuilder builder)
        {
            var options = builder.ApplicationServices.GetService<IOptions<TgsApiLoggerOptions>>()!.Value;
            var allowPaths = options.AllowPaths;
            var ignorePaths = options.IgnorePaths;

            return builder.UseWhen((context) =>
            {
                var isSpecific = allowPaths.Any();
                return isSpecific ? allowPaths.Any(path => context.Request.Path.StartsWithSegments(path)) : ignorePaths.All(path => !context.Request.Path.StartsWithSegments(path));
            }, (appBuilder) => appBuilder.UseMiddleware<TgsApiLoggerMiddleware>());
        }
    }
}
