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

            ILogger<Program> logger = (ILogger<Program>)host.Services.GetService(typeof(ILogger<Program>));
            IOptions<Settings> options = ((IOptions<Settings>)host.Services.GetService(typeof(IOptions<Settings>)));
            Settings settings = options?.Value;
            var client = new MongoClient(settings.ConnectionString);

            try
            {
                using (Mongod mongod = new Mongod(options, (ILogger<Mongod>)host.Services.GetService(typeof(ILogger<Mongod>))))
                {
                    logger.LogInformation("mongod.exe started");
                    client.GetDatabase(settings.Database).GetCollection<Recipe>(settings.Collection);

                    using (MongoImport mongoImport = new MongoImport(options, (ILogger<MongoImport>)host.Services.GetService(typeof(ILogger<MongoImport>))))
                    {
                        logger.LogInformation("mongoimport.exe started");
                        mongoImport.WaitForExit();
                        logger.LogInformation("mongoimport.exe finished");
                    }

                    //This will run when Ctrl+C is pressed but before the other processes are ended.
                    Console.CancelKeyPress += new ConsoleCancelEventHandler((sender, e) =>
                    {
                        using (MongoExport mongoExport = new MongoExport(options, (ILogger<MongoExport>)host.Services.GetService(typeof(ILogger<MongoExport>))))
                        {
                            logger.LogInformation("mongoexport.exe started");
                            mongoExport.WaitForExit();
                            logger.LogInformation("mongoexport.exe finished");
                        }
                    });

                    logger.LogInformation("webAPI starting");
                    host.Run();

                    mongod.WaitForExit();
                    logger.LogInformation("WebAPI and mongod.exe finished"); //This won't appear in file logs. I don't know why.
                }
            }
            catch (Exception e)
            {
                logger.LogCritical("Program Crash: ", e); //This won't appear in file logs. I don't know why.
            }
        }
    }
}