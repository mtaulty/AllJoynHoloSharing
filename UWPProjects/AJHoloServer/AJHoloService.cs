namespace AJHoloServer
{
  using com.mtaulty.AJHoloServer;
  using System;
  using System.Collections.Concurrent;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using System.Threading.Tasks;
  using Windows.Devices.AllJoyn;
  using Windows.Foundation;

  class AJHoloServiceStatUpdatedEventArgs : EventArgs
  {
    public enum Property
    {
      Devices,
      Anchors,
      Holograms
    }
    public Property ChangedProperty { get; set; }
    public uint Value { get; set; }
  }
  class AJHoloService : IAJHoloServerService
  {
    public event EventHandler<AJHoloServiceStatUpdatedEventArgs> StatUpdated;

    public AJHoloService(AJHoloServerProducer producer)
    {
      this.producer = producer;
      this.producer.SessionMemberAdded += OnSessionMemberAdded;
      this.producer.SessionMemberRemoved += OnSessionMemberRemoved;

      this.worldAnchors = new ConcurrentDictionary<Guid, byte[]>();
      this.uploadingWorldAnchors = new ConcurrentDictionary<Guid, MemoryStream>();
      this.holograms = new ConcurrentDictionary<Guid, HologramDetails>();
    }

    #region Public Async Operation Based Interface Members
    public IAsyncOperation<AJHoloServerAddHologramToAnchorResult> AddHologramToAnchorAsync(AllJoynMessageInfo info, string interfaceMemberAnchorId, string interfaceMemberHoloId, string interfaceMemberHoloTypeName, AJHoloServerPosition interfaceMemberPosition)
    {
      return (
        this.AddHologramToAnchorInternalAsync(
          info,
          interfaceMemberAnchorId,
          interfaceMemberHoloId,
          interfaceMemberHoloTypeName,
          interfaceMemberPosition).AsAsyncOperation());
    }

    public IAsyncOperation<AJHoloServerAddWorldAnchorResult> AddWorldAnchorAsync(
      AllJoynMessageInfo info,
      string interfaceMemberAnchorId,
      uint byteStart,
      uint byteLength,
      bool lastBlock,
      IReadOnlyList<byte> interfaceMemberAnchorData)
    {
      return (
        this.AddWorldAnchorInternalAsync(
          info,
          interfaceMemberAnchorId,
          byteStart,
          byteLength,
          lastBlock,
          interfaceMemberAnchorData).AsAsyncOperation());
    }
    public IAsyncOperation<AJHoloServerGetWorldAnchorIdsResult> GetWorldAnchorIdsAsync(
      AllJoynMessageInfo info)
    {
      return (
        this.GetWorldAnchorIdsInternalAsync(info).AsAsyncOperation());
    }
    public IAsyncOperation<AJHoloServerGetDeviceConnectedCountResult> GetDeviceConnectedCountAsync(AllJoynMessageInfo info)
    {
      return (
        this.GetDeviceConnectedCountInternalAsync(info).AsAsyncOperation());
    }

    public IAsyncOperation<AJHoloServerGetHologramIdsAndNamesResult> GetHologramIdsAndNamesAsync(AllJoynMessageInfo info)
    {
      return (
        this.GetHologramIdsAndNamesInternalAsync(info).AsAsyncOperation());
    }

    public IAsyncOperation<AJHoloServerGetHologramTransformsResult> GetHologramTransformsAsync(AllJoynMessageInfo info)
    {
      return (
        this.GetHologramTransformsInternalAsync(info).AsAsyncOperation());
    }

    public IAsyncOperation<AJHoloServerGetWorldAnchorResult> GetWorldAnchorAsync(
      AllJoynMessageInfo info,
      string interfaceMemberAnchorId,
      uint byteIndex,
      uint byteLength)
    {
      return (
        this.GetWorldAnchorInternalAsync(
          info, byteIndex, byteLength, interfaceMemberAnchorId).AsAsyncOperation());
    }

    public IAsyncOperation<AJHoloServerRemoveHologramResult> RemoveHologramAsync(AllJoynMessageInfo info, string interfaceMemberHoloId)
    {
      return (
        this.RemoveHologramInternalAsync(info, interfaceMemberHoloId).AsAsyncOperation());
    }
    #endregion

    #region Private Task Based Implementations

    async Task<AJHoloServerAddWorldAnchorResult> AddWorldAnchorInternalAsync(
      AllJoynMessageInfo info,
      string interfaceMemberAnchorId,
      uint byteStart,
      uint byteLength,
      bool lastBlock,
      IReadOnlyList<byte> interfaceMemberAnchorData)
    {
      MemoryStream stream = null;

      var id = Guid.Parse(interfaceMemberAnchorId);

      if (!this.uploadingWorldAnchors.TryGetValue(id, out stream))
      {
        stream = new MemoryStream();
        this.uploadingWorldAnchors[id] = stream;
      }
      lock (stream)
      {
        stream.Seek(byteStart, SeekOrigin.Begin);

        stream.Write(interfaceMemberAnchorData.ToArray(), 0, (int)byteLength);

        if (lastBlock)
        {
          stream.Seek(0, SeekOrigin.Begin);

          this.worldAnchors.TryAdd(id, stream.ToArray());

          this.FireStatUpdated(AJHoloServiceStatUpdatedEventArgs.Property.Anchors,
            (uint)this.worldAnchors.Count);
        }
      }
      if (lastBlock)
      {
        this.uploadingWorldAnchors.TryRemove(id, out stream);

        stream.Dispose();

        this.producer.Signals.WorldAnchorAdded(interfaceMemberAnchorId);
      }
      return (AJHoloServerAddWorldAnchorResult.CreateSuccessResult());
    }

    async Task<AJHoloServerGetWorldAnchorIdsResult> GetWorldAnchorIdsInternalAsync(
      AllJoynMessageInfo info)
    {
      var ids = this.worldAnchors.Keys.Select(k => k.ToString()).ToList();

      return (AJHoloServerGetWorldAnchorIdsResult.CreateSuccessResult(ids));
    }
    async Task<AJHoloServerGetDeviceConnectedCountResult> GetDeviceConnectedCountInternalAsync(
      AllJoynMessageInfo info)
    {
      return (AJHoloServerGetDeviceConnectedCountResult.CreateSuccessResult(
        this.connectedDeviceCount));
    }

    async Task<AJHoloServerGetWorldAnchorResult> GetWorldAnchorInternalAsync(
      AllJoynMessageInfo info,
      uint byteStart,
      uint byteLength,
      string interfaceMemberAnchorId)
    {
      ArraySegment<byte>? returnedBits = null;
      byte[] bits;

      if (this.worldAnchors.TryGetValue(Guid.Parse(interfaceMemberAnchorId), out bits))
      {
        if (byteStart < bits.Length)
        {
          var length = Math.Min(bits.Length - byteStart, byteLength);
          returnedBits = new ArraySegment<byte>(bits, (int)byteStart, (int)length);
        }
      }
      return (
        returnedBits.HasValue ?
        AJHoloServerGetWorldAnchorResult.CreateSuccessResult(returnedBits) :
        AJHoloServerGetWorldAnchorResult.CreateFailureResult(0xA000));
    }
    async Task<AJHoloServerAddHologramToAnchorResult> AddHologramToAnchorInternalAsync(
      AllJoynMessageInfo info,
      string interfaceMemberAnchorId,
      string interfaceMemberHoloId,
      string interfaceMemberHoloTypeName,
      AJHoloServerPosition interfaceMemberPosition)
    {
      var details = new HologramDetails(
        Guid.Parse(interfaceMemberAnchorId),
        Guid.Parse(interfaceMemberHoloId),
        interfaceMemberHoloTypeName,
        interfaceMemberPosition);

      this.holograms[Guid.Parse(interfaceMemberHoloId)] = details;

      this.producer?.Signals.HologramAdded(
        details.AnchorId.ToString(),
        details.HologramId.ToString(),
        details.HologramTypeName,
        details.Position);

      this.FireStatUpdated(AJHoloServiceStatUpdatedEventArgs.Property.Holograms,
        (uint)this.holograms.Count);

      return (AJHoloServerAddHologramToAnchorResult.CreateSuccessResult());
    }

    async Task<AJHoloServerGetHologramIdsAndNamesResult> GetHologramIdsAndNamesInternalAsync(
      AllJoynMessageInfo info)
    {
      var idsAndNames = this.holograms.Select(
        entry => new AJHoloServerHoloIdsItem()
        {
          Value1 = entry.Value.AnchorId.ToString(),
          Value2 = entry.Value.HologramId.ToString(),
          Value3 = entry.Value.HologramTypeName
        }
      ).ToList();
      return (AJHoloServerGetHologramIdsAndNamesResult.CreateSuccessResult(idsAndNames));
    }

    async Task<AJHoloServerGetHologramTransformsResult> GetHologramTransformsInternalAsync(
      AllJoynMessageInfo info)
    {
      var transforms = this.holograms.Select(
        entry => new AJHoloServerHoloPositionsItem()
        {
          Value1 = entry.Value.Position.Value1,
          Value2 = entry.Value.Position.Value2,
          Value3 = entry.Value.Position.Value3
        }
      ).ToList();

      return (AJHoloServerGetHologramTransformsResult.CreateSuccessResult(transforms));
    }

    async Task<AJHoloServerRemoveHologramResult> RemoveHologramInternalAsync(
      AllJoynMessageInfo info,
      string interfaceMemberHoloId)
    {
      HologramDetails details;

      this.holograms.TryRemove(Guid.Parse(interfaceMemberHoloId), out details);

      this.producer.Signals.HologramRemoved(interfaceMemberHoloId);

      this.FireStatUpdated(AJHoloServiceStatUpdatedEventArgs.Property.Holograms,
        (uint)this.holograms.Count);

      return (AJHoloServerRemoveHologramResult.CreateSuccessResult());
    }

    #endregion

    void OnSessionMemberRemoved(AJHoloServerProducer sender, AllJoynSessionMemberRemovedEventArgs args)
    {
      this.connectedDeviceCount--;
      this.FireStatUpdated(AJHoloServiceStatUpdatedEventArgs.Property.Devices,
        this.connectedDeviceCount);
      this.producer.EmitDeviceConnectedCountChanged();
    }
    void OnSessionMemberAdded(
      AJHoloServerProducer sender, AllJoynSessionMemberAddedEventArgs args)
    {
      this.connectedDeviceCount++;
      this.FireStatUpdated(AJHoloServiceStatUpdatedEventArgs.Property.Devices,
        this.connectedDeviceCount);
      this.producer.EmitDeviceConnectedCountChanged();
    }
    void FireStatUpdated(AJHoloServiceStatUpdatedEventArgs.Property stat,
      uint value)
    {
      this.StatUpdated?.Invoke(this,
        new AJHoloServiceStatUpdatedEventArgs()
        {
          ChangedProperty = stat,
          Value = value
        }
      );
    }
    ConcurrentDictionary<Guid, byte[]> worldAnchors;
    ConcurrentDictionary<Guid, MemoryStream> uploadingWorldAnchors;
    ConcurrentDictionary<Guid, HologramDetails> holograms;
    volatile uint connectedDeviceCount;
    AJHoloServerProducer producer;
  }
}
