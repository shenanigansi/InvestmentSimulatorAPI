using Serilog;

namespace InvestmentSimulatorAPI.Static.Logger;

public static class LoggerConfig
{
    public static void Configure()
    {
        Directory.CreateDirectory("logs");
        
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File( 
                path: "Logs/DeviceStatuses/log-.txt",
                rollingInterval: RollingInterval.Day, 
                retainedFileCountLimit: 30, 
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.Console()
            .CreateLogger();
    }
}