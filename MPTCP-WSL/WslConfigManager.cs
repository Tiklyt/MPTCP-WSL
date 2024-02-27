using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Serilog;

namespace MPTCP_WSL;

public class WslConfigManager
{
    private readonly NetworkConfig _config;

    public WslConfigManager(NetworkConfig config)
    {
            _config = config;
            if (!config.ManageKernelLocation) return;
            string kernelPath = FileManager.GetKernelPath().Replace(@"\", @"\\");
            if (File.Exists(FileManager.GetWSLConfigPath()))
            {
                if (!IsKernelPathCorrect(kernelPath))
                {
                    UpdateKernelPath(kernelPath);
                }
            }
            else
            {
                File.WriteAllLines(FileManager.GetWSLConfigPath(),new []{"[wsl2]","kernel="+kernelPath});
            }
    }
    
    
    public static bool IsKernelPathCorrect(string expectedKernelPath)
    {
        try
        {
            string configFileContent = File.ReadAllText(FileManager.GetWSLConfigPath());
            Match match = Regex.Match(configFileContent, @"kernel\s*=\s*(.*)", RegexOptions.IgnoreCase);

            if (match.Success)
            {
                string actualKernelPath = match.Groups[1].Value.Trim();
                return string.Equals(actualKernelPath, expectedKernelPath, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    public static void UpdateKernelPath(string newKernelPath)
    {
        try
        {
            string configFileContent = File.ReadAllText(FileManager.GetWSLConfigPath());
            
            configFileContent = Regex.Replace(configFileContent, @"kernel\s*=\s*(.*)", $"kernel = {newKernelPath}");
            
            File.WriteAllText(FileManager.GetWSLConfigPath(), configFileContent);
        }
        catch (Exception ex)
        {
            Log.Error(ex,"Error while updating the kernel path");
        }
    }
}