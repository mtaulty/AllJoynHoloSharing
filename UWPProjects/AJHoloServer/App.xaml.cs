using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;

namespace AJHoloServer
{
  /// <summary>
  /// Provides application-specific behavior to supplement the default Application class.
  /// </summary>
  sealed partial class App : Application
  {
    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
      this.InitializeComponent();
    }

    /// <summary>
    /// Invoked when the application is launched normally by the end user.  Other entry points
    /// will be used such as when the application is launched to open a specific file.
    /// </summary>
    /// <param name="e">Details about the launch request and process.</param>
    protected override void OnLaunched(LaunchActivatedEventArgs e)
    {
      var mainControl = Window.Current.Content as MainControl;

      // Do not repeat app initialization when the Window already has content,
      // just ensure that the window is active
      if (mainControl == null)
      {
        // Create a Frame to act as the navigation context and navigate to the first page
        mainControl = new MainControl();

        // Place the frame in the current Window
        Window.Current.Content = mainControl;
      }

      if (e.PrelaunchActivated == false)
      {
        // Ensure the current window is active
        Window.Current.Activate();
      }
    }
  }
}
