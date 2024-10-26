using NetKit.Observer.Workers;

namespace NetKit.Observer;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        builder.Services.AddHostedService<ConfigurationWorker>();

        var host = builder.Build();
        host.Run();
    }
}