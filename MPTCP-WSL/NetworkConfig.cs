using MPTCP_WSL;
using Newtonsoft.Json;

public class NetworkConfig
{
    
   
    public List<NetworkInformation> Config { get; set; } = new();
    public Proxy Proxy { get; set; } = new();
    public bool ManageKernelLocation = true;
    public bool ManageEndpoint = true;
    public bool KeepWSL2Awake = true;
    public int SubflowNr { get; set; } = 2;
    public int AddAddrAcceptedNr { get; set; } = 4;
    public NetworkConfig()
    {
        Config = new List<NetworkInformation>();
    }
    
    public void NetworkMonitorOnOnUpdate(object? sender, CollectionUpdateEvent e)
    {
        if (!Config.Contains(e.NetworkInfo))
        {
            Config.Add(e.NetworkInfo);
            SaveConfigToFile();
        }
    }
    

    public void SaveConfigToFile()
    {
        string json = JsonConvert.SerializeObject(this, Formatting.Indented);
        File.WriteAllText(FileManager.GetConfigPath(), json);
    }

    public static NetworkConfig LoadConfigFromFile()
    {
        try
        {
            if (File.Exists(FileManager.GetConfigPath()))
            {
                string json = File.ReadAllText(FileManager.GetConfigPath());
                var config = JsonConvert.DeserializeObject<NetworkConfig>(json);
                if (config == null) return new NetworkConfig();
                else return config;
            }
            else
            { 
                return new NetworkConfig();
            }
        }
        catch (Exception ex)
        {
            return new NetworkConfig();
        }
    }
}

public class NetworkInformation : IEquatable<NetworkInformation>
{
    public string InterfaceName { get; set; }
    public string FriendlyInterfaceName { get; set; }
    public string WindowsMacAddress { get; set; }
    public string LinuxMacAddress { get; set; }
    public List<string> Types { get; set; } = new ();

    public bool Equals(NetworkInformation? other)
    {
        return WindowsMacAddress.Equals(other?.WindowsMacAddress);
    }
}

public class Proxy
{
    private string proxyAddress;
    private string proxyPort;
    private string user;
    private string password;
}

