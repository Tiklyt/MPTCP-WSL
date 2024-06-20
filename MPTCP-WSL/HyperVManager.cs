using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace MPTCP_WSL;

/// <summary>
///     Allow to create external Hyper-V switch
/// </summary>
public class HyperVManager
{
    private static readonly SemaphoreSlim Mutex = new(1);
    private readonly ILogger _logger;
    public EventHandler<CollectionUpdateEvent> OnAdd = null!;
    private readonly string vSwitchPrefix = "MPTCP - ";


    public HyperVManager(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    ///     Check if an external Hyper-V is already created for a specific NIC
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
    ///     Create an external Hyper-V switch for a specific NIC
    /// </summary>
    /// <remarks>
    ///     all created Hyper-V switch begin with "MTCP - ", only this type of naming is recognized by this app,
    ///     other naming convention will lead to complete disregard by the application and might not function as expected
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
                        OnAdd.Invoke(this, new CollectionUpdateEvent
                        {
                            Type = EventType.Addition,
                            NetworkInfo = netInfo
                        });
                        _logger.LogInformation($"vSwitch : {id} created successfully");
                    }
                    else
                    {
                        _logger.LogInformation($"Unable to create the vSwitch : {id} reason : " +
                                               $"{RunPowerShell(command).Output}");
                    }
                }
                else
                {
                    _logger.LogInformation($"the vSwitch {id} : already exists");
                    netInfo.FriendlyInterfaceName = vSwitchPrefix + netInfo.FriendlyInterfaceName;
                    OnAdd.Invoke(this, new CollectionUpdateEvent
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
    ///     Helper function that allow to run some PowerShell command
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
            var statusCode = powerShellProcess.ExitCode;

            return (statusCode, output.Length != 0 ? output : errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running powershell command");
            return (-1, ex.Message);
        }
    }
}