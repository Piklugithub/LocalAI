using LocalAI.Application.Configuration;
using LocalAI.Application.DependencyInjection;
using LocalAI.Inference.DependencyInjection;
using LocalAI.Infrastructure.DependencyInjection;
using LocalAI.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Windows;
using System.Windows.Threading;

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

        try
        {
            await _host.StartAsync();

            // Initialize database first.
            using (var scope = _host.Services.CreateScope())
            {
                var databaseInitializer =
                    scope.ServiceProvider
                        .GetRequiredService<DatabaseInitializer>();

                await databaseInitializer.InitializeAsync();
            }

            // Show splash screen while LocalAI initializes.
            var splashWindow = new SplashWindow();

            splashWindow.Show();

            // Create the main window.
            var mainWindow = _host.Services
                .GetRequiredService<MainWindow>();

            // Perform model discovery, model loading,
            // conversation loading, etc.
            await mainWindow.InitializeAsync(
                splashWindow);

            // Initialization completed.
            splashWindow.Close();

            mainWindow.Show();

            MainWindow = mainWindow;
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"LocalAI could not start.\n\n{ex.Message}",
                "LocalAI Startup Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            Shutdown(-1);
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