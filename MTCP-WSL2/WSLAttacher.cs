using System.Collections.Concurrent;
using System.Diagnostics;
using System.Management;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using WSLAttachSwitch;
using Timer = System.Timers.Timer;

namespace MTCP_WSL2;

/// <summary>
///     Monitor WSL and allow to attach every Physical NIC into WSL
/// </summary>
public class WslAttacher
{
    private static readonly SafeBool IsWslRunning = new();
    private static readonly ConcurrentBag<NetworkInformation> Interfaces = new();
    private readonly CancellationToken _token;
    private readonly ILogger _logger;

    /// <summary>
    ///     Create an instance of WSLAttacher class
    /// </summary>
    public WslAttacher(ILogger logger,CancellationToken token)
    {
        _logger = logger;
        _token = token;
        IsWslRunning.Value = false;
        CheckIfWslStarted();
        CheckIfWslStopped();
        ClearUnManagedMem();
    }

    /// <summary>
    ///     This method should be raised when a NIC is added or recognized
    /// </summary>
    public void AddEvent(object? o, CollectionUpdateEvent e)
    {
        if (!Interfaces.Contains(e.NetworkInfo)) Interfaces.Add(e.NetworkInfo);
        if (IsWslProcessRunning())
        {
            string transMacAddress = MacAddressUtil.Transform(e.NetworkInfo.MacAddress);
            WSLAttachTool.Attach(e.NetworkInfo.FriendlyInterfaceName,transMacAddress);
        }
    }

    /**
     * Create a task that flush memory every 10 seconds
     */
    private void ClearUnManagedMem()
    {
        var aTimer = new Timer(10000);
        aTimer.Elapsed += (sender, args) =>
        {
            if (!_token.IsCancellationRequested)
            {
                FlushMemory();
            }
        };
        aTimer.AutoReset = true;
        aTimer.Enabled = true;
    }

    private void CheckIfWslStarted()
    {
        var queryString =
            "SELECT TargetInstance" +
            "  FROM __InstanceCreationEvent " +
            "WITHIN  5 " +
            " WHERE TargetInstance ISA 'Win32_Process' " +
            "   AND TargetInstance.Name = '" + "wslhost.exe" + "'";
        var scope = @"\\.\root\CIMV2";
        var watcher = new ManagementEventWatcher(scope, queryString);
        watcher.EventArrived += async (sender, e) =>
        {
            if (IsWslRunning.Value == false)
            {
                await Task.Delay(1000);
                foreach (var iface in Interfaces)
                {
                    string transMacAddress = MacAddressUtil.Transform(iface.MacAddress);
                    if (!WSLAttachTool.Attach(iface.FriendlyInterfaceName,transMacAddress))
                    {
                        _logger.LogInformation($"Interface {iface.InterfaceName} could not be attached.");
                    }
                    else
                    {
                        _logger.LogInformation($"Interface {iface.InterfaceName} attached successfully.");
                    }
                }
                IsWslRunning.Value = true;
            }
            e.NewEvent.Dispose();
        };
        watcher.Start();
    }

    private void CheckIfWslStopped()
    {
        var queryString =
            "SELECT TargetInstance" +
            "  FROM __InstanceDeletionEvent " +
            "WITHIN  5 " +
            " WHERE TargetInstance ISA 'Win32_Process' " +
            "   AND TargetInstance.Name = '" + "wslhost.exe" + "'";

        // The dot in the scope means use the current machine
        var scope = @"\\.\root\CIMV2";

        // Create a watcher and listen for events
        var watcher = new ManagementEventWatcher(scope, queryString);
        watcher.EventArrived += (sender, e) =>
        {
            IsWslRunning.Value = false;
            //See : https://qa.social.msdn.microsoft.com/Forums/en-US/158d5f4b-1238-4854-a66c-b51e37550c52/memory-leak-in-wmi-when-querying-event-logs?forum=netfxbcl
            e.NewEvent.Dispose();
        };
        watcher.Start();
    }
    
    /// <summary>
    ///     check if WSL is running
    /// </summary>
    /// <returns>true if running</returns>
    private static bool IsWslProcessRunning()
    {
        return Process.GetProcessesByName("wslhost").Length > 0;
    }

    [DllImport("kernel32.dll", EntryPoint = "SetProcessWorkingSetSize", ExactSpelling = true, CharSet =
        CharSet.Ansi, SetLastError = true)]
    private static extern int SetProcessWorkingSetSize(IntPtr process, int minimumWorkingSetSize, int
        maximumWorkingSetSize);

    /// <summary>
    ///     Flush the memory by calling the garbage collector
    /// </summary>
    private void FlushMemory()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1);
    }
}