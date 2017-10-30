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

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            try
            {
                if (!String.IsNullOrWhiteSpace(e?.Data?.ToString()))
                {
                    _logger.LogDebug($"{sender}:\n\t{e.Data}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{sender}:\n\tFailed to log. Asynchronous process finished before asynchrous logs. Usually caused by debugging.", ex);
            }
        }

        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            try
            {
                if (!String.IsNullOrWhiteSpace(e?.Data?.ToString()))
                {
                    _logger.LogWarning($"{sender}:\n\t{e.Data}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to log. Asynchronous process finished before asynchrous logs. Usually caused by debugging.", ex);
            }
        }

        public void Dispose()
        {
            process?.Dispose();
            process = null;
        }
    }
}
