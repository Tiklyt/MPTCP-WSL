namespace MTCP_WSL2;

public class FileManager
{
    private const string AppName = "MPTCP";
    private const string AppContext = "MPTCP Enabler";
    private const string ConfigFileName = "config.json";
    private const string LogFileName = "log.txt";
    private const string WslConfigName = ".wslconfig";
    private const string KernelName = "bzImage";

    public static string GetConfigPath()
    {
        string localPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return localPath + Path.DirectorySeparatorChar + AppName + Path.DirectorySeparatorChar + ConfigFileName;
    }

    public static string GetLogPath()
    {
        string localPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return localPath + Path.DirectorySeparatorChar + AppName + Path.DirectorySeparatorChar + LogFileName;
    }
    
    public static string GetKernelPath()
    {
        string localPath = GetApplicationFolderPath();
        return localPath 
               + AppName 
               + Path.DirectorySeparatorChar 
               + AppContext 
               + Path.DirectorySeparatorChar 
               + KernelName;
    }


    public static string GetApplicationFolderPath()
    {
        return Environment.GetEnvironmentVariable("ProgramFiles(x86)") + Path.DirectorySeparatorChar;
    }

    public static string GetWSLConfigPath()
    {
        string localPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return localPath + Path.DirectorySeparatorChar + WslConfigName;
    }
    
}