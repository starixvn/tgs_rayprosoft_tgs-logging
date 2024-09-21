using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Tgs.Logging
{
    public class TgsApiLoggerMiddleware(RequestDelegate next, ITgsLog tgsLog, IOptions<TgsApiLoggerOptions> options)
    {

        private readonly TgsApiLoggerOptions _options = options.Value;
        public async Task InvokeAsync(HttpContext context)
        {
            var statusCode = 200;
            var timeStart = DateTimeOffset.UtcNow;
            var isManual = _options.IsManual;
            if (!isManual)
            {
                context.Request.EnableBuffering();
            }

            var response = string.Empty;
            try
            {
                await next(context);
                statusCode = context.Response.StatusCode;
                response = isManual ? context.Items["ResponseContent"]?.ToString() ?? string.Empty : await ReadStreamAsync(context.Response.Body);
            }
            catch (Exception ex)
            {
                statusCode = 500;
                response = ex.ToString();
                throw;
            }
            finally
            {
                var timeEnd = DateTimeOffset.UtcNow;
                string request = isManual ? context.Items["RequestContent"]?.ToString() ?? string.Empty : await ReadStreamAsync(context.Request.Body);

                _ = Guid.TryParse(context.Items["TrackingId"]?.ToString(), out var trackingId);
                var option = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

                var apiCall = new ApiLogEvent
                {
                    ExecutionTimeMs = ((int)timeEnd.Subtract(timeStart).TotalSeconds),
                    Ip = context.Connection?.RemoteIpAddress?.ToString() ?? string.Empty,
                    Method = context.Request.Method,
                    Path = context.Request.Path,
                    Prefix = context.Request.PathBase,
                    RecordedOn = DateTimeOffset.Now,
                    RequestContent = request,
                    RequestHeaders = JsonSerializer.Serialize(context.Request.Headers, option),
                    ResponseCode = (short)statusCode,
                    ResponseContent = response,
                    ResponseHeaders = JsonSerializer.Serialize(context.Response.Headers, option),
                    ServerName = Environment.MachineName,
                    TrackingId = trackingId,
                };

                tgsLog.LogApi(apiCall);
            }
        }

        private static async Task<string> ReadStreamAsync(Stream stream)
        {
            string request = string.Empty;

            if (stream.CanRead)
            {
                var requestReader = new StreamReader(stream);
                request = await requestReader.ReadToEndAsync();
            }

            return request;
        }
    }
}
