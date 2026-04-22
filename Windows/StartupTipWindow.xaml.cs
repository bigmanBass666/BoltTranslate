using System.Windows;
using System.Windows.Media.Animation;

namespace BoltTranslate.Windows;

public partial class StartupTipWindow : Window
{
    public StartupTipWindow()
    {
        InitializeComponent();
    }

    public void SetText(string text)
    {
        TipText.Text = text;
    }

    public void ShowFor(int durationMs)
    {
        Show();
        var timer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromMilliseconds(durationMs) };
        timer.Tick += (s, _) =>
        {
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(300));
            fadeOut.Completed += (sender, e) => Close();
            BeginAnimation(OpacityProperty, fadeOut);
            timer.Stop();
        };
        timer.Start();
    }
}
