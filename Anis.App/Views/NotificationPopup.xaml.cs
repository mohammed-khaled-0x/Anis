using System.Windows;
using Anis.Core.Domain;
using System.Windows.Media.Animation;

namespace Anis.App.Views;

public partial class NotificationPopup : Window
{
    public NotificationPopup()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        // Get the primary screen's working area (excludes the taskbar)
        var workArea = SystemParameters.WorkArea;

        // Position the window at the bottom-right corner
        var margin = 10.0; // Margin from the screen edges
        this.Left = workArea.Right - this.ActualWidth - margin;
        this.Top = workArea.Bottom - this.ActualHeight - margin;
    }
    public void CloseWithAnimation()
    {
        var anim = new DoubleAnimation(0, (Duration)TimeSpan.FromSeconds(0.3));
        anim.Completed += (s, _) => this.Close();
        this.BeginAnimation(UIElement.OpacityProperty, anim);
    }
    private void Border_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        CloseWithAnimation();
    }
    public void SetData(Clip clip, Reciter reciter)
    {
        TextDisplay.Text = clip.Text;
        TitleDisplay.Text = $"{reciter.Name} - {clip.Title}";
    }
}