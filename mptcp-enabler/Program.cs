using MPTCP_WSL;
using Serilog;
using Serilog.Events;

namespace mptcp_enabler;

public class Program
{
    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .Enrich.FromLogContext().WriteTo
            .File(FileManager.GetLogPath())
            .CreateLogger();

        try
        {
            Host.CreateDefaultBuilder(args).UseWindowsService()
                .ConfigureServices(services => { services.AddHostedService<Worker>(); })
                .UseSerilog()
                .Build().Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "An error occured when running the MPTCP Enabler service");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}