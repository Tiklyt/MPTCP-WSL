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
            .File(@"C:\tmp\mptcp\log.txt")
            .CreateLogger();

        try
        {
            Host.CreateDefaultBuilder(args).UseWindowsService()
                .ConfigureServices(services => { services.AddHostedService<Worker>(); }).UseSerilog()
                .Build().Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "There was a problem starting the mptcp service.");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}