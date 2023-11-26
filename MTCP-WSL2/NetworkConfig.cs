using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using MTCP_WSL2;
using Newtonsoft.Json;

public class NetworkConfig
{
    
   
    public List<NetworkInformation> Config { get; set; } = new();

    public bool ManageKernelLocation = true;
    public bool ManageNetworkConfiguration = true;
    public int SubflowNr { get; set; } = 2;
    public int AddAddrAcceptedNr { get; set; } = 4;
    public string DnsServer = "8.8.8.8";
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
    public string MacAddress { get; set; }
    public List<string> Types { get; set; } = new ();

    public bool Equals(NetworkInformation? other)
    {
        return InterfaceName.Equals(other?.InterfaceName);
    }
}

