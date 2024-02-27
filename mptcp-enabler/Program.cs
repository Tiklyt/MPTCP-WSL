using MTCP_WSL2;
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
            Log.Fatal(ex, "an error occured when running the mptcp service");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}