using System.Text.RegularExpressions;
using Serilog;

namespace MPTCP_WSL;

public class WslConfigManager
{
    private readonly NetworkConfig _config;
    private readonly string wslConfigPath;

    public WslConfigManager(NetworkConfig config)
    {
        _config = config;
        if (!config.ManageKernelLocation) return;
        wslConfigPath = FileManager.GetWSLConfigPath();
        if (wslConfigPath == null) return;
        var kernelPath = FileManager.GetKernelPath().Replace(@"\", @"\\");

        if (File.Exists(wslConfigPath))
        {
            if (!IsKernelPathCorrect(kernelPath)) UpdateKernelPath(kernelPath);
        }
        else
        {
            File.WriteAllLines(wslConfigPath, new[] { "[wsl2]", "kernel=" + kernelPath });
        }
    }


    private bool IsKernelPathCorrect(string expectedKernelPath)
    {
        try
        {
            var configFileContent = File.ReadAllText(wslConfigPath);
            var match = Regex.Match(configFileContent, @"kernel\s*=\s*(.*)", RegexOptions.IgnoreCase);

            if (match.Success)
            {
                var actualKernelPath = match.Groups[1].Value.Trim();
                return string.Equals(actualKernelPath, expectedKernelPath, StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    private void UpdateKernelPath(string newKernelPath)
    {
        try
        {
            var configFileContent = File.ReadAllText(wslConfigPath);
            if (Regex.IsMatch(configFileContent, @"^kernel\s*=\s*.*$", RegexOptions.Multiline))
                configFileContent = Regex.Replace(configFileContent, @"^kernel\s*=\s*.*$", $"kernel = {newKernelPath}",
                    RegexOptions.Multiline);
            else
                configFileContent += $"kernel = {newKernelPath}\n";
            File.WriteAllText(wslConfigPath, configFileContent);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error while updating the kernel path");
        }
    }
}