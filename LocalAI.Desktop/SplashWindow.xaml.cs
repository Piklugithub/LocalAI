using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace LocalAI.Desktop;

public partial class SplashWindow
{
    public SplashWindow()
    {
        InitializeComponent();

        Loaded += SplashWindow_Loaded;
    }

    public void SetStatus(string message)
    {
        StatusTextBlock.Text = message;
    }

    private void SplashWindow_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        StartLoadingAnimation();
    }

    private void StartLoadingAnimation()
    {
        var storyboard = new Storyboard();

        AddDotAnimation(
            storyboard,
            Dot1,
            TimeSpan.Zero);

        AddDotAnimation(
            storyboard,
            Dot2,
            TimeSpan.FromMilliseconds(180));

        AddDotAnimation(
            storyboard,
            Dot3,
            TimeSpan.FromMilliseconds(360));

        storyboard.Begin();
    }

    private static void AddDotAnimation(
        Storyboard storyboard,
        UIElement element,
        TimeSpan beginTime)
    {
        var animation = new DoubleAnimation
        {
            From = 0.25,
            To = 1.0,
            Duration = TimeSpan.FromMilliseconds(500),
            AutoReverse = true,
            RepeatBehavior = RepeatBehavior.Forever,
            BeginTime = beginTime
        };

        Storyboard.SetTarget(
            animation,
            element);

        Storyboard.SetTargetProperty(
            animation,
            new PropertyPath(
                UIElement.OpacityProperty));

        storyboard.Children.Add(animation);
    }
}