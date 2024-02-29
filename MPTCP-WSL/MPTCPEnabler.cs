using Microsoft.Extensions.Logging;

namespace MPTCP_WSL;


/// <summary>
/// Main class of the application that allow to enable MPTCP inside WSL2
/// </summary>
public class MPTCPEnabler
{
    private readonly HyperVManager _hyperVManager;
    private readonly NetworkMonitor _networkMonitor;
    private readonly WslAttacher _wslAttacher;
    private NetworkConfig _config;

    /// <summary>
    /// Create an instance of MPTCP class 
    /// </summary>
    /// <param name="refreshDelay">delay in ms between each update of the network interfaces</param>
    public MPTCPEnabler(ILogger logger,CancellationToken token,int refreshDelay,NetworkConfig config)
    {
        _config = config;
        _hyperVManager = new HyperVManager(logger);
        _networkMonitor = new NetworkMonitor(logger,token,refreshDelay);
        _wslAttacher = new WslAttacher(logger,_config,token);
        _networkMonitor.OnUpdate += _config.NetworkMonitorOnOnUpdate;
        _networkMonitor.OnUpdate += async (sender, collectionUpdateEvent) =>
        {
            if (collectionUpdateEvent.Type == EventType.Addition)
                await _hyperVManager.CreateHyperVSwitch(collectionUpdateEvent.NetworkInfo);
            Console.WriteLine(collectionUpdateEvent.NetworkInfo.InterfaceName);
        };
        _hyperVManager.OnAdd += _wslAttacher.AddEvent;
    }
    
}