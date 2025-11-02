using MMORPG_SERVER;
using Serilog;

internal class Program
{
    static async Task Main()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Async(a => a.Console())
            .WriteTo.Async(a => a.File("Logger/DailyLog.txt", rollingInterval : RollingInterval.Day))
            .CreateLogger();

        GameServer server = new GameServer(8080);
        await server.Run();
    }
}