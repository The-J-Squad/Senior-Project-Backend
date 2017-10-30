using System;
using System.Diagnostics;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Bson;
using RecipeBackend.Models;
using RecipeBackend.Processes;

namespace RecipeBackend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            

                var host = new WebHostBuilder()
                        .UseKestrel()
                        .UseContentRoot(Directory.GetCurrentDirectory())
                        .UseIISIntegration()
                        .UseStartup<Startup>()
                        .UseApplicationInsights()
                        .ConfigureLogging(logging => logging.SetMinimumLevel(LogLevel.Trace))
                        .Build();



            host.Run();
        }
    }
}