using System.Diagnostics;
using System.Management;
using Microsoft.Extensions.Logging;

namespace MTCP_WSL2;


/// <summary>
/// Allow to create external Hyper-V switch
/// </summary>
public class HyperVManager
{
    public string vSwitchPrefix = "MPTCP - ";
    private static readonly SemaphoreSlim Mutex = new(1);
    public EventHandler<CollectionUpdateEvent> OnAdd = null!;
    private readonly ILogger _logger;


    public HyperVManager(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Check if an external Hyper-V is already created for a specific NIC
    /// </summary>
    /// <param name="interfaceName">The physical name of the NIC</param>
    /// <returns>true if </returns>
    private bool IsNetworkInterfaceSwitched(string interfaceName)
    {
        var switches = RunPowerShell("Get-VMNetworkAdapter -ManagementOS|Select -ExpandProperty SwitchName")
            .Output.Replace("\r", "");
        return switches.Contains(interfaceName);
    }

    /// <summary>
    ///  Get the name that Windows gave to an interface given a specific Physical NIC name.
    ///  <example>these name can be : Ethernet, Ethernet #1, Wi-Fi,...</example>
    /// </summary>
    /// <param name="interfaceName">the physical NIC name that we want to look for</param>
    /// <returns>the windows name</returns>
    private static string GetWindowsInterfaceName(string interfaceName)
    {
        var networkAdapterSearcher =
            new ManagementObjectSearcher("root\\CIMV2",
                "SELECT * FROM Win32_NetworkAdapter");
        var networkAdapterCollection = networkAdapterSearcher.Get();
        foreach (ManagementObject networkAdapter in networkAdapterCollection)
            if (interfaceName.Equals(networkAdapter["Name"].ToString()))
                return networkAdapter["NetConnectionID"].ToString();
        return "";
    }

    /// <summary>
    /// Create an external Hyper-V switch for a specific NIC
    /// </summary>
    /// <remarks>all created Hyper-V switch begin with "MTCP - ", only this type of naming is recognized by this app,
    /// other naming convention will lead to complete disregard by the application and might not function as expected
    /// </remarks>
    /// <param name="interfaceName">The name of the network interface for which the switch should be created</param>
    public async Task CreateHyperVSwitch(NetworkInformation netInfo)
    {
        await Mutex.WaitAsync(); //since Hyper-V don't allow creation of switch simultaneously, a mutex is put here
                                 //and is released when the Task is finished
        try
        {
            await Task.Run(() =>
            {
                var interfaceConnectionId = netInfo.FriendlyInterfaceName;
                var id = vSwitchPrefix + interfaceConnectionId;
                if (!IsNetworkInterfaceSwitched(interfaceConnectionId))
                {
                    var command =
                        $"New-VMSwitch -Name '{id}' -NetAdapterName '{interfaceConnectionId}' -AllowManagementOS $true";
                    if (RunPowerShell(command).StatusCode == 0)
                    {
                        netInfo.FriendlyInterfaceName = vSwitchPrefix + netInfo.FriendlyInterfaceName;
                        OnAdd.Invoke(this, new CollectionUpdateEvent()
                        {
                            Type = EventType.Addition,
                            NetworkInfo = netInfo
                        });
                        _logger.LogInformation($"vSwitch : {id} created successfully");
                    }
                    else
                    {
                        _logger.LogInformation($"Unable to create the vSwitch : {id}");
                    }
                }
                else
                {
                    _logger.LogInformation($"vSwitch : {id} already existing");
                    netInfo.FriendlyInterfaceName = vSwitchPrefix + netInfo.FriendlyInterfaceName;
                    OnAdd.Invoke(this, new CollectionUpdateEvent()
                    {
                        Type = EventType.Addition,
                        NetworkInfo = netInfo
                    });
                }
            });
        }
        finally
        {
            Mutex.Release();
        }
        
    }


    /// <summary>
    /// Helper function that allow to run some PowerShell command
    /// </summary>
    /// <param name="command">the command that we want to run</param>
    /// <returns>the output of the command</returns>
    private (int StatusCode, string Output) RunPowerShell(string command)
    {
        try
        {
            using var powerShellProcess = new Process();
            powerShellProcess.StartInfo.FileName = "powershell";
            powerShellProcess.StartInfo.Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command {command}";
            powerShellProcess.StartInfo.RedirectStandardOutput = true;
            powerShellProcess.StartInfo.RedirectStandardError = true;
            powerShellProcess.StartInfo.UseShellExecute = false;
            powerShellProcess.StartInfo.CreateNoWindow = true;
            powerShellProcess.Start();

            var output = powerShellProcess.StandardOutput.ReadToEnd();
            var errors = powerShellProcess.StandardError.ReadToEnd();

            powerShellProcess.WaitForExit();

            int statusCode = powerShellProcess.ExitCode;

            return (statusCode, output.Length != 0 ? output : errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,"Error running powershell command");
            return (-1, ex.Message);
        }
    }
}