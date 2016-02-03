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
using Microsoft.Extensions.DependencyInjection;
using Octokit;
using Octokit.Internal;

namespace Slydr
{
    public class Startup
    {
        private const string ApiToken = "079b22171cd1b3ae67bbab5af21be5880edf05b8";
        private const string ProductIdentifier = "Gittablog";

        public void Configure(IApplicationBuilder app)
        {
            app.UseIISPlatformHandler();
            
            app.Map("/slide", SlideRoute);
            
            app.Run(async (context) =>
            {
                var gitHub = CreateGitHubClient();
                var repositoryContents = await GetRepositoryContent(gitHub);

                var nrOfSlides = repositoryContents.Count(c => c.Name.EndsWith(".md") && c.Name.StartsWith("slide"));

                await Write(context, "<p>Found " + nrOfSlides + " slides</p>");
                await Write(context, "<a href='http://localhost:5000/slide/?nr=1'>Start</a>");
            });
        }

        private void SlideRoute(IApplicationBuilder app)
        {
            app.Run(async (context) =>
            {
                var slideNr = GetCurrentSlideNr(context);
                var slideHtml = await GetSlide(slideNr);

                await Write(context, slideHtml);

                await context.Response.WriteAsync("<a href='http://localhost:5000/slide/?nr=" + (slideNr + 1) + "'>Next slide</a>");
            });
        }

        private int GetCurrentSlideNr(HttpContext context)
        {
            return context.Request.Query.ContainsKey("nr") ? int.Parse(context.Request.Query.First(q => q.Key == "nr").Value.First()) : 1;
        }

        private async Task<string> GetSlide(int slideNr)
        {
            var gitHub = CreateGitHubClient();
            var content = await GetRepositoryContent(gitHub);

            var markdownFiles = content.Where(c => c.Name.EndsWith(".md"));

            var slide = markdownFiles.Where(md => md.Name.StartsWith("slide") && md.Name.Contains("0" + slideNr)).FirstOrDefault();

            if (slide != null)
            {
                var slideMarkdown = await Download(slide.DownloadUrl);
                return new Markdown().Transform(slideMarkdown);
            }
            return string.Empty;
        }

        private GitHubClient CreateGitHubClient()
        {
            var header = new ProductHeaderValue(ProductIdentifier);
            var credentialStore = new InMemoryCredentialStore(new Credentials(ApiToken));
            return new GitHubClient(header, credentialStore);
        }

        private async Task<IReadOnlyList<RepositoryContent>> GetRepositoryContent(GitHubClient github)
        {
            const string gitHubRepositoryOwner = "eaardal";
            const string gitHubRepositoryName = "mdtest";

            var repository = await github.Repository.Get(gitHubRepositoryOwner, gitHubRepositoryName);
            return await github.Repository.Content.GetAllContents(gitHubRepositoryOwner, gitHubRepositoryName, "/");
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
                var sr = new StreamReader(stream);
                return sr.ReadToEnd();
            }
        }

        public static void Main(string[] args) => Microsoft.AspNet.Hosting.WebApplication.Run<Startup>(args);
    }
}