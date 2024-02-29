using System.Diagnostics;

public class WSLAwaker
{
    private Process awakerProcess;
    private NetworkConfig _config;
    public WSLAwaker(NetworkConfig config)
    {
        _config = config;
        if (_config.KeepWSL2Awake)
        {
            Task.Run(() =>
            {
                awakerProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "wsl",
                        Arguments = "sleep infinity",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                awakerProcess.Start();
            });
        }
    }
    
    public void Stop()
    {
        if (_config.KeepWSL2Awake)
        {
            awakerProcess.Kill();
        }
    }
}