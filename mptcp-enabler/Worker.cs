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

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        
        NetworkConfig config = NetworkConfig.LoadConfigFromFile();
        WslConfigManager wslConfigManager = new WslConfigManager(config);
        var mtcpEnabler = new MPTCPEnabler(_logger,stoppingToken,RefreshDelay,config);
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(RefreshDelay, stoppingToken);
        }
    }
}