using System.Windows;
using Anis.Core.Domain;
using System.Windows.Media.Animation;
using System.Windows.Interop;
using Anis.App.Win32;

namespace Anis.App.Views;

public partial class NotificationPopup : Window
{
    public NotificationPopup()
    {
        InitializeComponent();
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        // Get the window handle (HWND)
        var helper = new WindowInteropHelper(this);
        IntPtr hwnd = helper.Handle;

        // --- THIS IS THE FIX ---
        // We use our custom helper to show the window without activating it.
        NoActivateWindowHelper.ShowWindowWithoutActivation(hwnd);

        // Position the window at the bottom-right corner
        var workArea = SystemParameters.WorkArea;
        var margin = 10.0;
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