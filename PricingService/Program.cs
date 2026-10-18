using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace PricingService;

public static class Program
{
    public static void Main(string[] args)
    {
        CreateWebHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateWebHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webHost => webHost.UseStartup<Startup>());
    }
}
