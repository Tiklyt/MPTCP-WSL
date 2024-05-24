using MPTCP_WSL;

namespace mptcp_enabler;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private const int RefreshDelay = 5000;
    private MPTCPEnabler _mptcpEnabler;
    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        NetworkConfig config = NetworkConfig.LoadConfigFromFile();
        WslConfigManager wslConfigManager = new WslConfigManager(config);
        _mptcpEnabler = new MPTCPEnabler(_logger, stoppingToken, RefreshDelay, config);
        _mptcpEnabler.Start();
    }
}