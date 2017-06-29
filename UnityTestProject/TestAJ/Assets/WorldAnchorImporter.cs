using UnityEngine;
using UnityEngine.VR.WSA.Sharing;
using System.Linq;

#if UNITY_UWP && !UNITY_EDITOR
using System.Threading.Tasks;
#endif // UNITY_UWP

public static class WorldAnchorImporter
{
#if UNITY_UWP && !UNITY_EDITOR
  public static async Task<bool> ImportWorldAnchorToGameObjectAsync(
    GameObject gameObject,
    byte[] worldAnchorBits)
  {
    var completion = new TaskCompletionSource<bool>();
    bool worked = false;

    WorldAnchorTransferBatch.ImportAsync(worldAnchorBits,
      (reason, batch) =>
      {
        if (reason == SerializationCompletionReason.Succeeded)
        {
          var anchorId = batch.GetAllIds().First();
          batch.LockObject(anchorId, gameObject);
          worked = true;
        }
        batch.Dispose();
      }
    );
    await completion.Task;

    return (worked);
  }
#endif // UNITY_UWP
}