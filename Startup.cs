using System;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;

namespace Slydr
{
    public class Startup
    {
        private Microsoft.Extensions.Logging.ILogger _log;
        private readonly IHostingEnvironment _hostingEnvironment;

        public IConfiguration Configuration { get; set; }

        public Startup(IHostingEnvironment hostingEnvironment)
        {
            if (hostingEnvironment == null) throw new ArgumentNullException(nameof(hostingEnvironment));
            _hostingEnvironment = hostingEnvironment;

            Configuration = new ConfigurationBuilder()
                .AddJsonFile("config.json")
                .AddUserSecrets()
                .Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddLogging();
            services.AddMvc().AddJsonOptions(o => o.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver());
        }

        public void Configure(IApplicationBuilder app)
        {
            ConfigureExceptionHandling(app);

            //_log = loggerFactory.CreateLogger("slydr");

            app.UseIISPlatformHandler();

            app.UseMvc();
        }

        private void ConfigureExceptionHandling(IApplicationBuilder app)
        {
            if (_hostingEnvironment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler(
                    subApp => subApp.Run(async context => await context.Response.WriteAsync("An error occurred :(")));
            }
        }
    }
}