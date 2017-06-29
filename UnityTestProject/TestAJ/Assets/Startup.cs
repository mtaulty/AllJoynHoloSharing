using HoloToolkit.Unity.InputModule;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.VR.WSA;
using UnityEngine.VR.WSA.Input;
using UnityEngine.VR.WSA.Sharing;


#if UNITY_UWP && !UNITY_EDITOR
using AJHoloClientLibrary;
using System.Numerics;
using System.Threading.Tasks;

#endif // UNITY_UWP


public class Startup : MonoBehaviour
#if UNITY_UWP && !UNITY_EDITOR
  , AJHoloClientLibrary.IAJHoloServerCallbacks
#endif // UNITY_UWP
{
  public void OnAnchor()
  {
#if UNITY_UWP && !UNITY_EDITOR
    this.CreateAnchorAsync();
#endif // UNITY_UWP
  }
  public void OnCube()
  {
#if UNITY_UWP && !UNITY_EDITOR
    this.CreateCubeAsync();
#endif // UNITY_UWP
  }
#if UNITY_UWP && !UNITY_EDITOR
  private void Start()
  {
    // TODO: Find a better way of doing this.
    this.managedThreadId = Environment.CurrentManagedThreadId;

    AJHoloServerConnection.CallbackHandler = this;
    this.gameObjects = new Dictionary<Guid, GameObject>();
    InternalStart();
  }
  async void InternalStart()
  {
    this.SetStatusDisplay("Starting, please wait...");
    await AJHoloServerConnection.StartAsync();
    this.SetStatusDisplay("waiting for voice command - lock or cube");
  }
  void MakeWorldAnchorGameObject()
  {
    // Make the object and add it to the parent.
    this.worldAnchor = new GameObject();
    var parentObject = GameObject.Find("Parent");
    this.worldAnchor.transform.parent = parentObject.transform;
    this.worldAnchor.transform.position = this.MakePositionOnGaze(3.0f);
  }
  async void CreateAnchorAsync()
  {
    // For the moment, we leave you with only one world anchor and we don't
    // worry about the race condition between you and another user.
    if (this.worldAnchor == null)
    {
      this.MakeWorldAnchorGameObject();

      // Now, anchor it and export the blob that represents that anchor
      this.SetStatusDisplay("exporting anchor on device, please wait...");
      byte[] worldAnchorBits = await
        WorldAnchorExporter.AddAndExportWorldAnchorForGameObjectAsync(this.worldAnchor);

      this.SetStatusDisplay("exporting anchor on device, please wait...");

      if (worldAnchorBits != null)
      {
        // Make a unique identifier for our new world anchor.
        this.worldAnchorId = Guid.NewGuid();

        this.SetStatusDisplay("copying over network, please wait...");

        // Send it out over the network.
        await AJHoloServerConnection.AddWorldAnchorAsync(
          this.worldAnchorId,
          worldAnchorBits,
          32 * 1024);
      }
      this.SetStatusDisplay("waiting for voice command - cube");
    }
  }
  async void CreateCubeAsync()
  {
    if (this.worldAnchor != null)
    {
      var relativePosition = this.MakePositionOnGazeRelativeToAnchor(3.0f);

      this.SetStatusDisplay("copying hologram details over network, please wait...");

      await AJHoloServerConnection.AddHologramAsync(
        this.worldAnchorId,
        Guid.NewGuid(),
        "cube",
        new System.Numerics.Vector3(relativePosition.x, relativePosition.y, relativePosition.z));

      this.SetStatusDisplay("waiting for voice command - cube");
    }
  }
  UnityEngine.Vector3 MakePositionOnGaze(float distance)
  {
    var worldPosition =
      Camera.main.transform.position +
      (distance * Camera.main.transform.forward);

    return (worldPosition);
  }
  UnityEngine.Vector3 MakePositionOnGazeRelativeToAnchor(float distance)
  {
    var worldPosition = this.MakePositionOnGaze(distance);
    var relativePosition = worldPosition - this.worldAnchor.transform.position;
    return (relativePosition);
  }
  public async Task WorldAnchorAddedAsync(Guid identifier, byte[] bits)
  {
    if (this.worldAnchor == null)
    {
      this.DispatchOnAppThread(
        async () =>
        {
          this.MakeWorldAnchorGameObject();

          this.SetStatusDisplay("importing anchor over network - please wait...");

          await WorldAnchorImporter.ImportWorldAnchorToGameObjectAsync(
            this.worldAnchor, bits);

          this.SetStatusDisplay("waiting for voice command - cube");
        }
      );
    }
  }
  public void HologramAdded(
    Guid anchorIdentifier,
    Guid hologramIdentifier,
    string hologramTypeName,
    System.Numerics.Vector3 position)
  {
    this.DispatchOnAppThread(
      () =>
      {
        if (this.worldAnchor != null)
        {
          switch (hologramTypeName)
          {
            case "cube":
              var gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
              gameObject.transform.localScale = new UnityEngine.Vector3(0.25f, 0.25f, 0.25f);
              gameObject.transform.parent = this.worldAnchor.transform;
              gameObject.transform.localPosition =
              new UnityEngine.Vector3(position.X, position.Y, position.Z);
              this.gameObjects[hologramIdentifier] = gameObject;
              break;
            default:
              break;
          }
        }
      }
    );
  }
  public void HologramRemoved(Guid hologramIdentifier)
  {
    this.DispatchOnAppThread(
      () =>
      {
        GameObject gameObject;

        if (this.gameObjects.TryGetValue(hologramIdentifier, out gameObject))
        {
          this.gameObjects.Remove(hologramIdentifier);
          Destroy(gameObject);
        }
      }
    );
  }
  public void DeviceCountUpdated(uint deviceCount)
  {
  }
  void DispatchOnAppThread(Action action)
  {
    if (Environment.CurrentManagedThreadId != this.managedThreadId)
    {
      UnityEngine.WSA.Application.InvokeOnAppThread(
        () => action(), false);
    }
    else
    {
      action();
    }
  }
  void SetStatusDisplay(string text)
  {
    this.DispatchOnAppThread(
      () =>
      {
        var uiText = GameObject.Find("UITextPrefab").GetComponentInChildren<UnityEngine.UI.Text>();
        uiText.text = text;
      }
    );
  }
  Dictionary<Guid, GameObject> gameObjects;
  Guid worldAnchorId;
  GameObject worldAnchor;
  int managedThreadId;
#endif // UNITY_UWP
}