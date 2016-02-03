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
         
        public void ConfigureServices(IServiceCollection services)
        {
            
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseIISPlatformHandler();

            app.Run(async (context) => {
               
               var gitHubRepositoryOwner = "eaardal";
               var gitHubRepositoryName = "mdtest";
               
               var header = new ProductHeaderValue(ProductIdentifier);
               var credentialStore = new InMemoryCredentialStore(new Credentials(ApiToken));
               var github = new GitHubClient(header, credentialStore);
               
               var repository = await github.Repository.Get(gitHubRepositoryOwner, gitHubRepositoryName);
               
               var content = await github.Repository.Content.GetAllContents(gitHubRepositoryOwner, gitHubRepositoryName, "/");
              
               var markdownFiles = content.Where(c => c.Name.EndsWith(".md"));
               
               var slides = markdownFiles.Where(md => md.Name.StartsWith("slide"));
               
               foreach (var slide in slides) 
               {
                   var slideContent = await Download(slide.DownloadUrl);
                    
                   var slideMarkdown = new Markdown().Transform(slideContent);
                   
                   await context.Response.WriteAsync(slideMarkdown);
               }                              
            });
        }
        
        private async Task Log(HttpContext context, string message)
        {
            await context.Response.WriteAsync(message + "\n");
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
