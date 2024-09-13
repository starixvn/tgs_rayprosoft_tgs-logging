using System;

namespace Tgs.NotifyService.Client
{
    public class NotifyClientOptions
    {
        public Uri BaseUrl { get; set; } = default!;

        public string Path { get; set; } = default!;

        public bool Enabled { get; set; } = false;
    }
}
