using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;

public class Program
{
    public static void Main(string[] args)
    {
        var host = new WebHostBuilder()
                .UseDefaultConfiguration()
                .UseStartup<Slydr.Startup>()
                .Build();

        host.Run();
    }
}