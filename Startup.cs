using System;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using Serilog;
using Serilog.Events;

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

            Log.Logger = new LoggerConfiguration()
                .WriteTo.RollingFile("Logs\\slydr-{Date}-verbose.log", LogEventLevel.Verbose)
                .WriteTo.RollingFile("Logs\\slydr-{Date}-warnings.log", LogEventLevel.Warning)
                .WriteTo.Console(LogEventLevel.Verbose)
                .CreateLogger();

            Configuration = new ConfigurationBuilder()
                .AddJsonFile("config.json")
                .AddUserSecrets()
                .Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging();
            services.AddMvc().AddJsonOptions(o => o.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver());
        }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddSerilog();
            _log = loggerFactory.CreateLogger("slydr");

            app.UseIISPlatformHandler();

            app.UseMvc();
        }
        
        public static void Main(string[] args)
        {
            Microsoft.AspNet.Hosting.WebApplication.Run<Startup>(args);
        }
    }
}