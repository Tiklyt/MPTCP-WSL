using System.Diagnostics;
using System.Management;
using System.Security.Principal;
using MTCP_WSL2;

namespace mptcp_enabler;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private const int RefreshDelay = 5000;
    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    private void WaitForServiceInWSL2()
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "bash.exe",
                Arguments = "-c 'ls'",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,// Run as administrator
            }
        };

        process.OutputDataReceived += (sender, e) =>
        {
            if (e.Data != null)
            {
                Console.WriteLine(e.Data);
                _logger.LogInformation($"Output received: {e.Data}");
            }
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (e.Data != null)
            {
                _logger.LogError($"Error received: {e.Data}");
            }
        };

        process.Exited += (sender, e) =>
        {
            _logger.LogInformation($"Process exited with code: {process.ExitCode}");
        };

        try
        {
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            _logger.LogInformation($"Current user: {Environment.UserName}");
            process.WaitForExit();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Exception: {ex.Message}");
        }

    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        //WaitForServiceInWSL2();
        NetworkConfig config = NetworkConfig.LoadConfigFromFile();
        WslConfigManager wslConfigManager = new WslConfigManager(config);  
        var mtcpEnabler = new MPTCPEnabler(_logger,stoppingToken,RefreshDelay,config);
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(RefreshDelay, stoppingToken);
        }
    }
}