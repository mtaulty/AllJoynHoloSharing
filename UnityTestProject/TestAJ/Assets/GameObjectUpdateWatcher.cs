using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_UWP && !UNITY_EDITOR
using System.Threading.Tasks;
#endif

public class GameObjectUpdateWatcher : MonoBehaviour
{
#if UNITY_UWP && !UNITY_EDITOR
  public GameObjectUpdateWatcher()
  {
    this.completed = new TaskCompletionSource<bool>();
  }
  public Func<bool> TestCondition { get; set; }
  void Update()
  {
    if ((TestCondition != null) && TestCondition())
    {
      this.completed.SetResult(true);
    }
  }
  public async static Task WaitForBooleanTestConditionAsync(
    GameObject gameObject,
    Func<bool> testCondition)
  {
    var component = gameObject.AddComponent<GameObjectUpdateWatcher>();
    component.TestCondition = testCondition;
    await component.completed.Task;
    Destroy(component);
  }
  TaskCompletionSource<bool> completed;
#else
  public GameObjectUpdateWatcher()
	{
      
	}
#endif
}
