using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using TGS.Apis.LogApi;

namespace LogApi.IntegrationTests
{
    internal class TestLogApiApplication : WebApplicationFactory<Startup>
    {
        private readonly Action<IWebHostBuilder>? _webHostConfigure;

        public TestLogApiApplication(Action<IWebHostBuilder>? webHostConfigure = null)
        {
            _webHostConfigure = webHostConfigure;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
            => _webHostConfigure?.Invoke(builder);
    }
}
