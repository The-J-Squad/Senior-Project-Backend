using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using System.Diagnostics;
using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using RecipeBackend.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Bson;

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
                        .Build();

            ILogger<Program> logger = (ILogger<Program>)host.Services.GetService(typeof(ILogger<Program>));
            Settings settings = ((IOptions<Settings>)host.Services.GetService(typeof(IOptions<Settings>)))?.Value;
            var client = new MongoClient(settings.ConnectionString);
            var mongoPath = Environment.GetEnvironmentVariable("MONGO") ?? "";
            // Use ProcessStartInfo class
            ProcessStartInfo mongod = new ProcessStartInfo
            {
                CreateNoWindow = false,
                UseShellExecute = false,
                FileName = $@"{mongoPath}mongod.exe",
                WindowStyle = ProcessWindowStyle.Maximized
            };

            try
            {
                // Start the process with the info we specified.
                // Call WaitForExit and then the using statement will close.
                using (Process exeProcess = Process.Start(mongod))
                {
                    client.GetDatabase(settings.Database).GetCollection<Recipe>(settings.Collection);
                    RestoreBackup(mongoPath, logger, settings);
                    logger.LogInformation("mongod.exe started");
                    host.Run();
                    logger.LogInformation("webAPI started");
                    exeProcess.WaitForExit();
                    logger.LogInformation("Program finished. webAPI and mongod.exe finished");
                    using (Process exeProcess2 = Process.Start(mongod))
                    {
                        logger.LogInformation("mongod.exe started again for restore");
                        SaveBackup(mongoPath, logger, settings);
                        
                        if (client != null)
                        {
                            var adminDatabase = client.GetDatabase("admin");
                            var cmd = new BsonDocument("shutdown", 1);
                            try
                            {
                                adminDatabase.RunCommand<BsonDocument>(cmd); //This throws an exception. No way around it.
                            }
                            catch (Exception e)
                            {
                                if (!(e.InnerException is IOException) && e.InnerException.Message == ("Unable to read data from the transport connection: An existing connection was forcibly closed by the remote host."))
                                {
                                    throw; //rethrow if we weren't expecting it.
                                }
                                else
                                {
                                    logger.LogInformation("mongod.exe finished from restore");
                                }
                            }
                        }
                        exeProcess2.Kill();
                    }
                    logger.LogInformation("mongod.exe finished");
                }
            }
            catch (Exception e)
            {
                    logger.LogError(e, "mongod.exe error");
                    Console.Out.WriteLine("Caught this" + e);
            }
        }

        private static void SaveBackup(string mongoPath, ILogger<Program> logger, Settings settings)
        {
            ProcessStartInfo mongodump = new ProcessStartInfo
            {
                CreateNoWindow = false,
                UseShellExecute = false,
                FileName = $@"{mongoPath}mongoexport.exe",
                WindowStyle = ProcessWindowStyle.Maximized,
                Arguments = $@"--db {settings.Database} --collection {settings.Collection} --out ../recipesBackup.json"
            };

            try
            {
                // Start the process with the info we specified.
                // Call WaitForExit and then the using statement will close.
                using (Process exeProcess = Process.Start(mongodump))
                {
                    logger.LogInformation("mongoexport.exe started");
                    exeProcess.WaitForExit();
                    logger.LogInformation("mongoexport.exe finished");
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "mongoexport.exe error");
                Console.Out.WriteLine("Failed Backup:" + e);
            }
        }

        private static void RestoreBackup(string mongoPath, ILogger<Program> logger, Settings settings)
        {
            ProcessStartInfo mongodump = new ProcessStartInfo
            {
                CreateNoWindow = false,
                UseShellExecute = false,
                FileName = $@"{mongoPath}mongoimport.exe",
                WindowStyle = ProcessWindowStyle.Maximized,
                Arguments = $@"--db {settings.Database} --collection {settings.Collection} --type json --mode upsert --file ../recipesBackup.json"
            };

            try
            {
                // Start the process with the info we specified.
                // Call WaitForExit and then the using statement will close.
                using (Process exeProcess = Process.Start(mongodump))
                {
                    logger.LogInformation("mongoimport.exe started");
                    exeProcess.WaitForExit();
                    logger.LogInformation("mongoimport.exe started");
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "mongoimport.exe error");
                Console.Out.WriteLine("Failed Restore:" + e);
            }
        }
    }
}