using System.IO;
using System.Windows;

namespace LocalAI.Desktop.Services;

public enum AppTheme
{
    Light,
    Dark
}

public sealed class ThemeService
{
    private readonly string _settingsDirectory;
    private readonly string _themeFile;

    public AppTheme CurrentTheme { get; private set; }

    public ThemeService()
    {
        _settingsDirectory =
            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                "LocalAI");

        _themeFile =
            Path.Combine(
                _settingsDirectory,
                "theme.txt");

        CurrentTheme = LoadTheme();
    }

    public void ApplyTheme(AppTheme theme)
    {
        var themeUri = theme == AppTheme.Dark
            ? new Uri(
                "/LocalAI.Desktop;component/Themes/DarkTheme.xaml",
                UriKind.Relative)
            : new Uri(
                "/LocalAI.Desktop;component/Themes/LightTheme.xaml",
                UriKind.Relative);

        var themeDictionary =
            new ResourceDictionary
            {
                Source = themeUri
            };

        var dictionaries =
            System.Windows.Application.Current.Resources.MergedDictionaries;

        var existingTheme =
            dictionaries.FirstOrDefault(
                dictionary =>
                    dictionary.Source?.OriginalString
                        .Contains("Theme.xaml",
                            StringComparison.OrdinalIgnoreCase) == true);

        if (existingTheme is not null)
        {
            var index = dictionaries.IndexOf(existingTheme);

            dictionaries[index] = themeDictionary;
        }
        else
        {
            dictionaries.Add(themeDictionary);
        }

        CurrentTheme = theme;

        SaveTheme(theme);
    }

    private AppTheme LoadTheme()
    {
        try
        {
            if (!File.Exists(_themeFile))
            {
                return AppTheme.Light;
            }

            var value =
                File.ReadAllText(_themeFile)
                    .Trim();

            return Enum.TryParse<AppTheme>(
                value,
                ignoreCase: true,
                out var theme)
                ? theme
                : AppTheme.Light;
        }
        catch
        {
            return AppTheme.Light;
        }
    }

    private void SaveTheme(AppTheme theme)
    {
        try
        {
            Directory.CreateDirectory(_settingsDirectory);

            File.WriteAllText(
                _themeFile,
                theme.ToString());
        }
        catch
        {
            // Theme persistence failure should not prevent
            // the application from running.
        }
    }
}