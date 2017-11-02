﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using RecipeBackend.Models;
using System;
using System.Diagnostics;
using System.Threading;

namespace RecipeBackend.Processes
{
    public class MongoImport : IDisposable
    {
        private Process process;

        private Mongod _mongod;
        private readonly Settings _settings;
        private readonly ILogger<MongoImport> _logger;

        public MongoImport(Mongod mongod, IOptions<Settings> settings, ILogger<MongoImport> logger)
        {
            _mongod = mongod;
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

        public void WaitForExit()
        {
            process?.WaitForExit();

            var client = new MongoClient(_settings.ConnectionString);
            var adminDatabase = client.GetDatabase("admin");
            var cmd = new BsonDocument("shutdown", 1);

            try {
                adminDatabase.RunCommand<BsonDocument>(cmd); //This throws an exception. No way around it. We will ignore it.
                Thread.Sleep(1000);
                _logger.LogWarning("Expected exception not yet caught");
            } catch {
                _logger.LogDebug("Expected exception caught");
            } 

            _mongod?.WaitForExit();
        }

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
            _mongod?.Dispose();
            _mongod = null;
            process?.Dispose();
            process = null;
        }
    }
}
