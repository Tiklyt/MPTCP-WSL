﻿using System.Diagnostics;
using Microsoft.Extensions.Logging;

public class WSLAwaker
{
    private Process awakerProcess;
    private NetworkConfig _config;
    private ILogger _logger;
    public WSLAwaker(ILogger logger,NetworkConfig config)
    {
        _config = config;
        this._logger = logger;
        if (_config.KeepWSL2Awake)
        {
            Task.Run(() =>
            {
                try
                {
                    awakerProcess = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "wsl",
                            Arguments = "sleep infinity",
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        }
                    };
                    awakerProcess.Start();
                }
                catch (Exception e)
                {
                    _logger.LogError(e,"Error while starting the WSL2 Awaker");
                }
            });
        }
    }
    
    public void Stop()
    {
        if (_config.KeepWSL2Awake)
        {
            awakerProcess.Kill();
        }
    }
}