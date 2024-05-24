namespace MPTCP_WSL;

public class FileManager
{
    private const string AppName = "MPTCP";
    private const string AppContext = "MPTCP Enabler";
    private const string ConfigFileName = "config.json";
    private const string LogFileName = "log.txt";
    private const string WslConfigName = ".wslconfig";
    private const string KernelName = "BzImage";
    private const string InstalledOnFileName = "installedOn";
    private static readonly string? UserPath = GetUsersPath();


    private static string? GetUsersPath()
    {
        var programFilesPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
        var filePath = Path.Combine(programFilesPath, AppName, InstalledOnFileName);
        try
        {
            var lines = File.ReadAllLines(filePath);
            if (lines.Length >= 2) return lines[1].Split("\\")[1];
            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public static string GetConfigPath()
    {
        if (UserPath == null)
        {
            return null;
        }
        return Path.Combine(Path.GetPathRoot(Environment.SystemDirectory), "Users", UserPath, "AppData", "Local",
            AppName, ConfigFileName);
    }

    public static string? GetLogPath()
    {
        if (UserPath == null)
        {
            return null;
        }
    return Path.Combine(Path.GetPathRoot(Environment.SystemDirectory), "Users", UserPath, "AppData", "Local",
            AppName, LogFileName);
    }

    public static string GetKernelPath()
    {
        return Path.Combine(GetApplicationFolderPath(), AppName, AppContext, KernelName);
    }


    private static string GetApplicationFolderPath()
    {
        return Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
    }

    public static string GetWSLConfigPath()
    {
        if (UserPath == null)
        {
            return null;
        }
        return Path.Combine(Path.GetPathRoot(Environment.SystemDirectory), "Users", UserPath, WslConfigName);
    }
}