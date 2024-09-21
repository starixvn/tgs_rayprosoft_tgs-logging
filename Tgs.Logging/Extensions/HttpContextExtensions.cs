using Microsoft.AspNetCore.Http;

namespace Tgs.Logging.Extensions
{
    public static class HttpContextExtensions
    {
        public static void AddTrackingId(this HttpContext context, Guid guid)
            => context.Items.Add("TrackingId", guid);

        public static void AddRequestContent(this HttpContext context, string requestContent)
            => context.Items.Add("RequestContent", requestContent);

        public static void AddResponseContent(this HttpContext context, string responseContent)
            => context.Items.Add("RequestContent", responseContent);
    }
}
