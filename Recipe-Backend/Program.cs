using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using System.Diagnostics;
using System;
using System.Threading;
using Microsoft.Extensions.Logging;

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
            var mongoPath = Environment.GetEnvironmentVariable("MONGOD");
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
                    RestoreBackup(mongoPath);
                    logger.LogDebug("mongod.exe started");
                    host.Run();
                    logger.LogDebug("webAPI started");
                    exeProcess.WaitForExit();

                    using (Process exeProcess2 = Process.Start(mongod))
                    {
                        SaveBackup(mongoPath);
                        exeProcess2.Kill();
                    }
                    logger.LogDebug("mongod.exe finished");
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "mongod.exe error");
                Console.Out.WriteLine("Caught this" + e);
            }
        }

        private static void SaveBackup(string mongoPath)
        {
            ProcessStartInfo mongodump = new ProcessStartInfo
            {
                CreateNoWindow = false,
                UseShellExecute = false,
                FileName = $@"{mongoPath}mongoexport.exe",
                WindowStyle = ProcessWindowStyle.Maximized,
                Arguments = @"--db RecipesDb --collection Recipe --out ../recipesBackup.json"
            };

            try
            {
                // Start the process with the info we specified.
                // Call WaitForExit and then the using statement will close.
                using (Process exeProcess = Process.Start(mongodump))
                {
                    exeProcess.WaitForExit();
                }
            }
            catch (Exception e)
            {
                Console.Out.WriteLine("Failed Backup:" + e);
            }
        }

        private static void RestoreBackup(string mongoPath)
        {
            ProcessStartInfo mongodump = new ProcessStartInfo
            {
                CreateNoWindow = false,
                UseShellExecute = false,
                FileName = $@"{mongoPath}mongoimport.exe",
                WindowStyle = ProcessWindowStyle.Maximized,
                Arguments = @"--db RecipesDb --collection Recipe --type json --mode upsert --file ../recipesBackup.json"
            };

            try
            {
                // Start the process with the info we specified.
                // Call WaitForExit and then the using statement will close.
                using (Process exeProcess = Process.Start(mongodump))
                {
                    exeProcess.WaitForExit();
                }
            }
            catch (Exception e)
            {
                Console.Out.WriteLine("Failed Restore:" + e);
            }
        }
    }
}