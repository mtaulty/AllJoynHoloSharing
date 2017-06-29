namespace AJHoloClientLibrary
{
  using com.mtaulty.AJHoloServer;
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using System.Numerics;
  using System.Threading.Tasks;
  using Windows.Devices.AllJoyn;

  public static class AJHoloServerConnection
  {
    static AJHoloServerConnection()
    {
      consumerReady = null;
      worldAnchorsAdded = new List<Guid>();
    }
    public static IAJHoloServerCallbacks CallbackHandler { get; set; }
    public static async Task StartAsync()
    {
      await WaitForConsumerReadyAsync();
      serviceConsumer.DeviceConnectedCountChanged += OnDeviceCountChanged;
      serviceConsumer.Signals.WorldAnchorAddedReceived += OnWorldAnchorAddedReceivedAsync;
      serviceConsumer.Signals.HologramRemovedReceived += OnHologramRemovedReceived;
      serviceConsumer.Signals.HologramAddedReceived += OnHologramAddedReceived;

      // We attempt to instantiate all the world anchors and holograms that are
      // already on the server...

      // First the anchors...
      var anchorIds = await serviceConsumer.GetWorldAnchorIdsAsync();

      if (anchorIds?.AnchorData?.Count > 0)
      {
        foreach (var anchorId in anchorIds.AnchorData)
        {
          await GetAndPublishWorldAnchorDetailsAsync(anchorId);
        }
      }

      // Now the holograms...
      var hologramIds = await serviceConsumer.GetHologramIdsAndNamesAsync();
      var holoDetails = await serviceConsumer.GetHologramTransformsAsync();

      if ((hologramIds?.HoloIds?.Count > 0) &&
        (hologramIds?.HoloIds?.Count == holoDetails?.HoloPositions?.Count))
      {
        for (int i = 0; i < hologramIds.HoloIds.Count; i++)
        {
          var anchorId = Guid.Parse(hologramIds.HoloIds[i].Value1);
          var holoId = Guid.Parse(hologramIds.HoloIds[i].Value2);
          var holoTypeName = hologramIds.HoloIds[i].Value3;
          var position = new Vector3(
            (float)holoDetails.HoloPositions[i].Value1,
            (float)holoDetails.HoloPositions[i].Value2,
            (float)holoDetails.HoloPositions[i].Value3);

          CallbackHandler.HologramAdded(
            anchorId, holoId, holoTypeName, position);
        }
      }
    }
    static async void OnWorldAnchorAddedReceivedAsync(
      AJHoloServerSignals sender, AJHoloServerWorldAnchorAddedReceivedEventArgs args)
    {
      // We don't want to go and download our own world anchors as we already
      // have them.
      if (!worldAnchorsAdded.Contains(Guid.Parse(args.AnchorId)))
      {
        await GetAndPublishWorldAnchorDetailsAsync(args.AnchorId);
      }
    }
    static async Task GetAndPublishWorldAnchorDetailsAsync(string anchorId)
    {
      var done = false;
      var startIndex = 0u;
      var bufferSize = ALLJOYN_CHUNK_SIZE;
      byte[] buffer = null;
      AJHoloServerGetWorldAnchorResult result = null;

      using (var stream = new MemoryStream())
      {
        while (!done)
        {
          result = await serviceConsumer.GetWorldAnchorAsync(
            anchorId, startIndex, bufferSize);

          if (!(done = (result.Status != AllJoynStatus.Ok)))
          {
            stream.Write(result.AnchorData.ToArray(), 0, result.AnchorData.Count);
            startIndex += (uint)result.AnchorData.Count;
          }
        }
        if (result.Status == 0xA000)
        {
          stream.Seek(0, SeekOrigin.Begin);
          buffer = stream.ToArray();
        }
      }
      if (buffer != null)
      {
        await CallbackHandler.WorldAnchorAddedAsync(Guid.Parse(anchorId), buffer);
      }
    }
    static void OnHologramAddedReceived(
      AJHoloServerSignals sender, AJHoloServerHologramAddedReceivedEventArgs args)
    {
      var anchorId = Guid.Parse(args.AnchorId);
      var holoId = Guid.Parse(args.HoloId);
      var holoTypeName = args.HoloTypeName;
      var position = new Vector3(
        (float)args.Position.Value1,
        (float)args.Position.Value2,
        (float)args.Position.Value3);

      CallbackHandler.HologramAdded(
        anchorId, holoId, holoTypeName, position);
    }
    static void OnHologramRemovedReceived(
      AJHoloServerSignals sender, AJHoloServerHologramRemovedReceivedEventArgs args)
    {
      CallbackHandler.HologramRemoved(Guid.Parse(args.HoloId));
    }
    static void OnDeviceCountChanged(AJHoloServerConsumer sender, object args)
    {
      CallbackHandler.DeviceCountUpdated((uint)args);
    }
    public static async Task AddWorldAnchorAsync(
      Guid identifier, byte[] bits, uint chunkSize = ALLJOYN_CHUNK_SIZE)
    {
      if (!worldAnchorsAdded.Contains(identifier))
      {
        worldAnchorsAdded.Add(identifier);
      }
      await WaitForConsumerReadyAsync();

      var bufferCount = (int)Math.Ceiling((double)bits.Length / (double)chunkSize);

      for (int i = 0; i < bufferCount; i++)
      {
        var lastBlock = (i == bufferCount - 1);
        var offset = (uint)(i * chunkSize);
        var length = (uint)Math.Min(bits.Length - offset, chunkSize);

        var slice = new ArraySegment<byte>(bits, (int)offset, (int)length);

        await serviceConsumer.AddWorldAnchorAsync(
          identifier.ToString(),
          offset,
          length,
          lastBlock,
          slice);
      }
    }
    public static async Task AddHologramAsync(
      Guid anchorIdentifier,
      Guid hologramIdentifier,
      string hologramTypeName,
      Vector3 relativePosition)
    {
      await WaitForConsumerReadyAsync();

      await serviceConsumer.AddHologramToAnchorAsync(
        anchorIdentifier.ToString(),
        hologramIdentifier.ToString(),
        hologramTypeName,
        new AJHoloServerPosition()
        {
          Value1 = relativePosition.X,
          Value2 = relativePosition.Y,
          Value3 = relativePosition.Z
        }
      );
    }
    static async Task WaitForConsumerReadyAsync()
    {
      if (consumerReady == null)
      {
        consumerReady = new TaskCompletionSource<bool>();

        var watcher = AllJoynBusAttachment.GetWatcher(
          new string[] { AJHoloServerConsumer.InterfaceName });

        watcher.Added += async (s, e) =>
        {
          serviceConsumer = await AJHoloServerConsumer.FromIdAsync(e.Id);
          consumerReady.SetResult(true);

          watcher.Stop();
        };
        watcher.Start();

        await consumerReady.Task;
      }
    }
    // The AllJoyn docs that I can find suggest that the maximum size of a buffer
    // is 2^17 = 128K. Having tried with that size, I get very bad performance
    // where it appears that 128K takes around 30s to transfer with no traffic
    // appearing on the network until around the 29s mark.
    // To try and mitigate this somewhat, I've dropped this to 1K to see if
    // sending tiny buffers helps.
    const uint ALLJOYN_CHUNK_SIZE = 32 * 1024;

    static TaskCompletionSource<bool> consumerReady;
    static AJHoloServerConsumer serviceConsumer;
    static List<Guid> worldAnchorsAdded;
  }
}