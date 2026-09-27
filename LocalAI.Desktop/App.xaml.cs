using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using LocalAI.Infrastructure.DependencyInjection;
using LocalAI.Inference.DependencyInjection;

using LocalAI.Application.Configuration;
using LocalAI.Application.DependencyInjection;

namespace LocalAI.Desktop;

public partial class App : System.Windows.Application
{
    private readonly IHost _host;

    public App()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, config) =>
            {
                config.SetBasePath(AppContext.BaseDirectory);

                config.AddJsonFile(
                    "appsettings.json",
                    optional: false,
                    reloadOnChange: true);
            })
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddDebug();
            })
            .ConfigureServices((context, services) =>
            {
                services.Configure<InferenceOptions>(
                    context.Configuration.GetSection(
                        InferenceOptions.SectionName));

                services.Configure<ApplicationOptions>(
                    context.Configuration.GetSection(
                        ApplicationOptions.SectionName));

                services.AddLocalAIApplication();

                services.AddSingleton<MainWindow>();

                services.AddLocalAIApplication();

                services.AddLocalAIInfrastructure();

                services.AddLocalAIInference();

                services.AddSingleton<MainWindow>();
            })
            .Build();
    }

    protected override async void OnStartup(
        StartupEventArgs e)
    {
        await _host.StartAsync();

        var mainWindow = _host.Services
            .GetRequiredService<MainWindow>();

        mainWindow.Show();

        base.OnStartup(e);
    }

    protected override async void OnExit(
        ExitEventArgs e)
    {
        await _host.StopAsync();

        _host.Dispose();

        base.OnExit(e);
    }
}