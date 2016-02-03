using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using MarkdownSharp;
using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.Configuration;
using Octokit;

namespace Slydr
{
    public class HomeController : Controller
    {        
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Slide()
        {
            return null;
        }
    }
/*
    class Temp : Controller
    {
        private async Task Default()
        {
            try
            {
                var repositoryOwner = GetRepositoryOwner();
                var repositoryName = GetRepositoryName();
                var gitHub = CreateGitHubClient();
                var repositoryContents = await GetRepositoryContent(gitHub, repositoryOwner, repositoryName);

                var nrOfSlides = repositoryContents.Count(c => c.Name.EndsWith(".md") && c.Name.StartsWith("slide"));

                var url = CreateNextSlideUrl(1);
                //await context.Response.WriteAsync("<a href='" + url + "'>Start</a>");
            }
            catch (Exception)
            {
                //context.Response.Clear();
                //await context.Response.WriteAsync("Some shit went wrong :(");
            }
        }

        private async Task SlideRoute()
        {
            try
            {
                var slideNr = GetCurrentSlideNr();
                var slideHtml = await GetSlide(slideNr);

                //await Write(context, slideHtml);

                var url = CreateNextSlideUrl(slideNr + 1);
                //await context.Response.WriteAsync("<a href='" + url + "'>Next slide</a>");
            }
            catch (Exception)
            {

            }
        }

        private string CreateNextSlideUrl(int slideNr)
        {
            var repositoryOwner = GetRepositoryOwner();
            var repositoryName = GetRepositoryName();
            return "http://localhost:5000/slide/?nr=" + slideNr + "&owner=" + repositoryOwner + "&repo=" + repositoryName;
        }

        private int GetCurrentSlideNr()
        {
            return HttpContext.Request.Query.ContainsKey("nr")
                ? int.Parse(HttpContext.Request.Query["nr"])
                : 1;
        }

        private async Task<string> GetSlide(int slideNr)
        {
            var repositoryOwner = GetRepositoryOwner();
            var repositoryName = GetRepositoryName();
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

        private string GetRepositoryName()
        {
            if (!HttpContext.Request.Query.ContainsKey("repo"))
            {
                throw new Exception("No repository name given as query string");
            }
            return HttpContext.Request.Query["repo"];
        }

        private string GetRepositoryOwner()
        {
            if (!HttpContext.Request.Query.ContainsKey("owner"))
            {
                throw new Exception("No repository owner given as query string");
            }
            return HttpContext.Request.Query["owner"];
        }

        private GitHubClient CreateGitHubClient()
        {
            var configuration = (IConfiguration)HttpContext.RequestServices.GetService(typeof(IConfiguration));
            var productIdentifier = configuration["github.productIdentifier"];
            var header = new ProductHeaderValue(productIdentifier);
            return new GitHubClient(header);
        }

        private async Task<IReadOnlyList<RepositoryContent>> GetRepositoryContent(GitHubClient github, string repositoryOwner, string repositoryName)
        {
            var repository = await github.Repository.Get(repositoryOwner, repositoryName);
            return await github.Repository.Content.GetAllContents(repositoryOwner, repositoryName, "/");
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

    }
    */
}