using System.IO;
using UnityEngine;
using UnityEngine.VR.WSA;
using UnityEngine.VR.WSA.Sharing;

#if UNITY_UWP && !UNITY_EDITOR
using System.Threading.Tasks;
#endif // UNITY_UWP

public static class WorldAnchorExporter
{
#if UNITY_UWP && !UNITY_EDITOR
  public static async Task<byte[]> AddAndExportWorldAnchorForGameObjectAsync(
    GameObject gameObject)
  {
    var worldAnchor = gameObject.AddComponent<WorldAnchor>();
    byte[] bits = null;

    await GameObjectUpdateWatcher.WaitForBooleanTestConditionAsync(
      gameObject,
      () => worldAnchor.isLocated);

    using (var worldAnchorBatch = new WorldAnchorTransferBatch())
    {
      worldAnchorBatch.AddWorldAnchor("anchor", worldAnchor);

      var completion = new TaskCompletionSource<bool>();

      using (var memoryStream = new MemoryStream())
      {
        WorldAnchorTransferBatch.ExportAsync(
          worldAnchorBatch,
          data =>
          {
            memoryStream.Write(data, 0, data.Length);
          },
          reason =>
          {
            if (reason != SerializationCompletionReason.Succeeded)
            {
              bits = null;
            }
            else
            {
              bits = memoryStream.ToArray();
            }
            completion.SetResult(bits != null);
          }
        );
        await completion.Task;
      }
    }
    return (bits);
  }
#endif // UNITY_UWP
}
