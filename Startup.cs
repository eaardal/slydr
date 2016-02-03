using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MarkdownSharp;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Octokit;
using Octokit.Internal;
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
        }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddSerilog();
            _log = loggerFactory.CreateLogger("slydr");

            app.UseIISPlatformHandler();

            app.Map("/slide", SlideRoute);

            app.Run(async (context) =>
            {
                try
                {
                    var repositoryOwner = GetRepositoryOwner(context);
                    var repositoryName = GetRepositoryName(context);
                    var gitHub = CreateGitHubClient();
                    var repositoryContents = await GetRepositoryContent(gitHub, repositoryOwner, repositoryName);

                    var nrOfSlides = repositoryContents.Count(c => c.Name.EndsWith(".md") && c.Name.StartsWith("slide"));

                    await Write(context, "<p>Found " + nrOfSlides + " slides</p>");
                    
                    var url = CreateNextSlideUrl(1, context);
                    await context.Response.WriteAsync("<a href='" + url + "'>Start</a>");
                }
                catch (Exception)
                {
                    context.Response.Clear();
                    await context.Response.WriteAsync("Some shit went wrong :(");
                }
            });
        }

        private void SlideRoute(IApplicationBuilder app)
        {
            app.Run(async (context) =>
            {
                try
                {
                    var slideNr = GetCurrentSlideNr(context);
                    var slideHtml = await GetSlide(slideNr, context);

                    await Write(context, slideHtml);

                    var url = CreateNextSlideUrl(slideNr + 1, context);
                    await context.Response.WriteAsync("<a href='" + url + "'>Next slide</a>");
                }
                catch (Exception)
                {
                    context.Response.Clear();
                    await context.Response.WriteAsync("Some shit went wrong :(");
                }
            });
        }

        private string CreateNextSlideUrl(int slideNr, HttpContext context)
        {
            var repositoryOwner = GetRepositoryOwner(context);
            var repositoryName = GetRepositoryName(context);
            return "http://localhost:5000/slide/?nr=" + slideNr + "&owner=" + repositoryOwner + "&repo=" + repositoryName;
        }

        private int GetCurrentSlideNr(HttpContext context)
        {
            return context.Request.Query.ContainsKey("nr") ? int.Parse(context.Request.Query.First(q => q.Key == "nr").Value.First()) : 1;
        }

        private async Task<string> GetSlide(int slideNr, HttpContext context)
        {
            var repositoryOwner = GetRepositoryOwner(context);
            var repositoryName = GetRepositoryName(context);
            var gitHub = CreateGitHubClient();
            var content = await GetRepositoryContent(gitHub, repositoryOwner, repositoryName);

            var markdownFiles = content.Where(c => c.Name.EndsWith(".md"));

            var slide = markdownFiles.Where(md => md.Name.StartsWith("slide") && md.Name.Contains("0" + slideNr)).FirstOrDefault();

            if (slide != null)
            {
                var slideMarkdown = await Download(slide.DownloadUrl);
                return new Markdown().Transform(slideMarkdown);
            }
            return string.Empty;
        }

        private string GetRepositoryName(HttpContext context)
        {
            if (!context.Request.Query.ContainsKey("repo"))
            {
                throw new Exception("No repository name given as query string");
            }
            return context.Request.Query["repo"];
        }

        private string GetRepositoryOwner(HttpContext context)
        {
            if (!context.Request.Query.ContainsKey("owner"))
            {
                throw new Exception("No repository owner given as query string");
            }
            return context.Request.Query["owner"];
        }

        private GitHubClient CreateGitHubClient()
        {
            var productIdentifier = Configuration["github.productIdentifier"];
            var header = new ProductHeaderValue(productIdentifier);
            return new GitHubClient(header);
        }

        private async Task<IReadOnlyList<RepositoryContent>> GetRepositoryContent(GitHubClient github, string repositoryOwner, string repositoryName)
        {
            var repository = await github.Repository.Get(repositoryOwner, repositoryName);
            return await github.Repository.Content.GetAllContents(repositoryOwner, repositoryName, "/");
        }

        private async Task Write(HttpContext context, string message)
        {
            await context.Response.WriteAsync(message + "<br/>");
        }

        private async Task<string> Download(Uri url)
        {
            var webClient = new WebClient();
            using (var stream = new MemoryStream(await webClient.DownloadDataTaskAsync(url)))
            {
                stream.Position = 0;
                var reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }
        }

        public static void Main(string[] args)
        {
            Microsoft.AspNet.Hosting.WebApplication.Run<Startup>(args);
        }
    }
}