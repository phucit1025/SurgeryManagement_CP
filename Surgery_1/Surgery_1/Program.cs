using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Surgery_1.Controllers;

namespace Surgery_1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();

            //var config = new ConfigurationBuilder()
            //    .AddCommandLine(args)
            //    .Build();
            //var configuration = new ConfigurationBuilder()
            //    .AddCommandLine(args)
            //    .Build();

            //var hostUrl = configuration["hosturl"];
            //if (string.IsNullOrEmpty(hostUrl))
            //    hostUrl = $"http://{CommonUtil.GetLocalIPAddress()}:5000";

            //var host = new WebHostBuilder()
            //    .UseKestrel()
            //    .UseUrls($"http://+:50794", $"https://+:44372")   // <!-- this 
            //    .UseContentRoot(Directory.GetCurrentDirectory())
            //    .UseIISIntegration()
            //    .UseStartup<Startup>()
            //    .UseConfiguration(config)
            //    .Build();

            //host.Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
