using MPTCP_WSL;
using Newtonsoft.Json;

public class NetworkConfig
{
    public bool ManageEndpoint = true;
    public bool ManageKernelLocation = true;

    public NetworkConfig()
    {
        Config = new List<NetworkInformation>();
    }


    public List<NetworkInformation> Config { get; set; } = new();
    public Proxy Proxy { get; set; } = new();
    public int SubflowNr { get; set; } = 2;
    public int AddAddrAcceptedNr { get; set; } = 4;

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
        var cfgPath = FileManager.GetConfigPath();
        if (cfgPath == null) return;
        var json = JsonConvert.SerializeObject(this, Formatting.Indented);
        File.WriteAllText(cfgPath, json);
    }

    public static NetworkConfig LoadConfigFromFile()
    {
        try
        {
            var cfgPath = FileManager.GetConfigPath();
            if (File.Exists(cfgPath))
            {
                var json = File.ReadAllText(cfgPath);
                var config = JsonConvert.DeserializeObject<NetworkConfig>(json);
                if (config == null) return new NetworkConfig();
                return config;
            }

            return new NetworkConfig();
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
    public List<string> Types { get; set; } = new();

    public bool Equals(NetworkInformation? other)
    {
        return WindowsMacAddress.Equals(other?.WindowsMacAddress);
    }
}

public class Proxy
{
    public string password = "";
    public string proxyAddress = "";
    public string proxyPort = "";
    public string user = "";
}