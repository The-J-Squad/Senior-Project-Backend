using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RecipeBackend.Models;
using System;
using System.Diagnostics;

namespace RecipeBackend.Processes
{
    public class MongoImport : IDisposable
    {
        private Process process;
        private readonly Settings _settings;
        private readonly ILogger<MongoImport> _logger;

        public MongoImport(IOptions<Settings> settings, ILogger<MongoImport> logger)
        {
            _settings = settings.Value;
            _logger = logger;

            process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    CreateNoWindow = false,
                    UseShellExecute = false,
                    FileName = $@"{_settings.MongoPath}mongoimport.exe",
                    Arguments = $@"--db {_settings.Database} --collection {_settings.Collection} --type json --mode upsert --file ../recipesBackup.json",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };

            process.ErrorDataReceived += Process_ErrorDataReceived;
            process.OutputDataReceived += Process_OutputDataReceived;
            process.Start();
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();
        }

        public void WaitForExit() => process?.WaitForExit();

        public void Kill() => process?.Kill();

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(e?.Data?.ToString()))
            {
                _logger.LogInformation($"{sender}:\n\t{e.Data}");
            }
        }

        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(e?.Data?.ToString()))
            {
                _logger.LogWarning($"{sender}:\n\t{e.Data}");
            }
        }

        public void Dispose()
        {
            process?.CancelErrorRead();
            process?.CancelOutputRead();
            process?.Dispose();
            process = null;
        }
    }
}
