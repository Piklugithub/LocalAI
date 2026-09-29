using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using LocalAI.Application.Configuration;
using LocalAI.Application.DependencyInjection;
using LocalAI.Infrastructure.DependencyInjection;
using LocalAI.Inference.DependencyInjection;

namespace LocalAI.Desktop;

public partial class App : System.Windows.Application
{
    private readonly IHost _host;

    public App()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, config) =>
            {
                config.SetBasePath(
                    AppContext.BaseDirectory);

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

                services.AddLocalAIInfrastructure();

                services.AddLocalAIInference();

                services.AddSingleton<MainWindow>();
            })
            .Build();
    }

    protected override async void OnStartup(
        StartupEventArgs e)
    {
        base.OnStartup(e);

        var splashWindow =
            new SplashWindow();

        splashWindow.Show();

        // Give WPF a chance to render the splash screen.
        await Dispatcher.InvokeAsync(
            () => { },
            DispatcherPriority.Render);

        try
        {
            splashWindow.SetStatus(
                "Starting application...");

            await _host.StartAsync();

            splashWindow.SetStatus(
                "Preparing LocalAI...");

            var mainWindow =
                _host.Services
                    .GetRequiredService<MainWindow>();

            await mainWindow.InitializeAsync(
                splashWindow);

            splashWindow.SetStatus(
                "Ready");

            // Small delay so the user can see "Ready".
            await Task.Delay(
                300);

            MainWindow = mainWindow;

            splashWindow.Close();

            mainWindow.Show();
        }
        catch (Exception ex)
        {
            splashWindow.Close();

            MessageBox.Show(
                $"LocalAI could not start.\n\n{ex.Message}",
                "LocalAI Startup Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            Shutdown();
        }
    }

    protected override async void OnExit(
        ExitEventArgs e)
    {
        try
        {
            await _host.StopAsync();
        }
        finally
        {
            _host.Dispose();
        }

        base.OnExit(e);
    }
}