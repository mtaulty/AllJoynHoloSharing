using com.mtaulty.AJHoloServer;
using System;
using Windows.Devices.AllJoyn;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace AJHoloServer
{
  public sealed partial class MainControl : UserControl
  {
    public MainControl()
    {
      this.InitializeComponent();

      this.Loaded += OnLoaded;
    }
    void OnLoaded(object sender, RoutedEventArgs e)
    {
      // We need to attach to the AllJoyn bus.
      this.busAttachment = new AllJoynBusAttachment();

      this.serviceProducer = new AJHoloServerProducer(this.busAttachment);
      this.serviceImplementation = new AJHoloService(this.serviceProducer);
      this.serviceImplementation.StatUpdated += OnDeviceCountUpdated;
      this.serviceProducer.Service = this.serviceImplementation;
      this.serviceProducer.Start();

      this.txtServerStatus.Text = "server running";
    }

    async void OnDeviceCountUpdated(object sender, AJHoloServiceStatUpdatedEventArgs e)
    {
      await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
        () =>
        {
          Run txtBlock = null;

          switch (e.ChangedProperty)
          {
            case AJHoloServiceStatUpdatedEventArgs.Property.Devices:
              txtBlock = this.txtDevicesCount;         
              break;
            case AJHoloServiceStatUpdatedEventArgs.Property.Anchors:
              txtBlock = this.txtAnchorCount;
              break;
            case AJHoloServiceStatUpdatedEventArgs.Property.Holograms:
              txtBlock = this.txtHologramCount;
              break;
            default:
              break;
          }
          txtBlock.Text = e.Value.ToString();
        });
    }
    AJHoloService serviceImplementation;
    AJHoloServerProducer serviceProducer;
    AllJoynBusAttachment busAttachment;
  }
}
