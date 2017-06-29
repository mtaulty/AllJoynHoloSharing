using System;
using System.Numerics;
using System.Threading.Tasks;

namespace AJHoloClientLibrary
{
  public interface IAJHoloServerCallbacks
  {
    Task WorldAnchorAddedAsync(Guid identifier, byte[] bits);
    void HologramAdded(Guid anchorIdentifier, Guid hologramIdentifier, string hologramTypeName, Vector3 position);
    void HologramRemoved(Guid hologramIdentifier);
    void DeviceCountUpdated(uint deviceCount);
  }
}
