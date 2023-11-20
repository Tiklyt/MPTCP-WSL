using System.Management;
using Microsoft.Extensions.Logging;

namespace MTCP_WSL2;

/// <summary>
/// Monitor Physical Network Interfaces, basically it throw an event When there is deletion or an
/// addition of a physical NIC.
/// </summary>
public class NetworkMonitor
{

    private readonly HashSet<string> _interfacesNames = new();
    private readonly CancellationToken _token;
    private readonly ILogger _logger;

    /// <summary>
    /// Initialize a new instance of the NetworkMonitor class.
    /// </summary>
    /// <param name="refreshDelay">delay in ms between each update of the network interfaces</param>
    public NetworkMonitor(ILogger logger,CancellationToken token,int refreshDelay)
    {
        _logger = logger;
        _token = token;
        Loop(refreshDelay);
    }

    
    public event EventHandler<CollectionUpdateEvent> OnUpdate = null!;


    /// <summary>
    /// Create a task that indefinitely check for addition or deletion of Physical NIC.
    /// </summary>
    /// <param name="refreshDelay">The delay in milliseconds between each update</param>
    private void Loop(int refreshDelay)
    {
        Task.Run(async () =>
        {
            while (true)
            {
                UpdateNetworkInterfaces();
                await Task.Delay(refreshDelay);
            }
        },_token);
    }

    
    /// <summary>
    /// Add or delete into the hash set names of Physical NIC's, also raise an event containing
    /// information about which NIC have been added or deleted.
    /// </summary>
    private void UpdateNetworkInterfaces()
    {
        var updatedInterfacesName = GetAllNetworkInterfaces();
        foreach (var interfaceName in updatedInterfacesName.Where(interfaceName =>
                     !_interfacesNames.Contains(interfaceName)))
        {
            _interfacesNames.Add(interfaceName);
            OnUpdate?.Invoke(this, new CollectionUpdateEvent(EventType.Add, interfaceName));
        }

        foreach (var interfaceName in _interfacesNames.Where(interfaceName =>
                     !updatedInterfacesName.Contains(interfaceName)))
        {
            _interfacesNames.Remove(interfaceName);
            OnUpdate?.Invoke(this, new CollectionUpdateEvent(EventType.Del, interfaceName));
        }
    }

    /// <summary>
    /// Helper function that get the names of all Physical NIC's.
    /// </summary>
    ///
    /// <returns>list containing the names of all physical NIC's</returns>
    private List<string> GetAllNetworkInterfaces()
    {
        var interfaces = new List<string>();
        try
        {
            var query = new ObjectQuery("SELECT * FROM Win32_NetworkAdapter " +
                                        "WHERE (PNPDeviceID LIKE 'PCI%' " +
                                        "OR PNPDeviceID LIKE 'USB%' or PNPDeviceID LIKE 'PCMCIA%') " +
                                        "AND NetConnectionID IS NOT NULL");
            var searcher = new ManagementObjectSearcher(query);
            foreach (ManagementObject adapter in searcher.Get()) interfaces.Add(adapter["Name"].ToString());
        }
        catch (Exception e)
        {
            _logger.LogError(e,"Error while getting all Physical NIC's names");
        }
        return interfaces;
    }
}