using System.Management;

namespace MTCP_WSL2;

public class FileManager
{
    private const string AppName = "MPTCP";
    private const string AppContext = "MPTCP Enabler";
    private const string ConfigFileName = "config.json";
    private const string LogFileName = "log.txt";
    private const string WslConfigName = ".wslconfig";
    private const string KernelName = "bzImage";
    private const string sp = Path.DirectorySeparatorChar;

    public static string GetConfigPath()
    {
        var searcher = new ManagementObjectSearcher("SELECT UserName FROM Win32_ComputerSystem");
        var collection = searcher.Get();
        var username = (string)collection.Cast<ManagementBaseObject>().First()["UserName"];
        username = username.Split("\\")[1];
        var path = Path.GetPathRoot(Environment.SystemDirectory)
                   + "Users"
                   + sp
                   + username
                   + sp
                   + "AppData"
                   + sp
                   + "Local"
                   + sp
                   + AppName
                   + sp
                   + ConfigFileName;
        return path;
    }

    public static string GetLogPath()
    {
        var searcher = new ManagementObjectSearcher("SELECT UserName FROM Win32_ComputerSystem");
        var collection = searcher.Get();
        var username = (string)collection.Cast<ManagementBaseObject>().First()["UserName"];
        username = username.Split("\\")[1];
        var path = Path.GetPathRoot(Environment.SystemDirectory)
                   + "Users"
                   \+ sp
                   + username
                   + sp
                   + "AppData"
                   + sp
                   + "Local"
                   + sp
                   + AppName
                   + sp
                   + LogFileName;
        return path;
    }

    public static string GetKernelPath()
    {
        var localPath = GetApplicationFolderPath();
        return localPath
               + AppName
               + sp
               + AppContext
               + sp
               + KernelName;
    }


    public static string GetApplicationFolderPath()
    {
        return Environment.GetEnvironmentVariable("ProgramFiles(x86)") + sp;
    }

    public static string GetWSLConfigPath()
    {
        var searcher = new ManagementObjectSearcher("SELECT UserName FROM Win32_ComputerSystem");
        var collection = searcher.Get();
        var username = (string)collection.Cast<ManagementBaseObject>().First()["UserName"];
        username = username.Split("\\")[1];
        var path = Path.GetPathRoot(Environment.SystemDirectory)
                   + "Users"
                   + sp
                   + username
                   + sp
                   + WslConfigName;
        return path;
    }
}