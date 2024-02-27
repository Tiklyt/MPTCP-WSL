using System.Diagnostics;

namespace MPTCP_WSL;

public class WSLAwaker
{
    
    public WSLAwaker(NetworkConfig config)
    {
        if (!config.KeepWSL2Awake)
        {
            return;
        }
            string command = "wsl -e sleep infinity";
            Process process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/c " + command,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
    }
}