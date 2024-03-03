using MPTCP_WSL;

namespace mptcp_enabler;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private const int RefreshDelay = 5000;
    private MPTCPEnabler mptcpEnabler;
    private WSLAwaker awaker;
    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        NetworkConfig config = NetworkConfig.LoadConfigFromFile();
        WslConfigManager wslConfigManager = new WslConfigManager(config);
        mptcpEnabler = new MPTCPEnabler(_logger, stoppingToken, RefreshDelay, config);
        awaker = new WSLAwaker(_logger,config);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        awaker.Stop();
        return base.StopAsync(cancellationToken);
    }
}