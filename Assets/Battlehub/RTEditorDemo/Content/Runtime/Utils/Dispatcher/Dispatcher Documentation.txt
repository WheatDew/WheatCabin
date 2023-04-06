----------------------------------------------------------------------
Main thread dispatcher script is located in Assets\Battlehub\Dispatcher\Dispatcher.cs file
Demo scene is located in Assets\Battlehub\Dispatcher\Demo.unity file

----------------------------------------------------------------------
Dispatcher
class in Battlehub.Dispatcher / Inherits from: MonoBehavior

----------------------------------------------------------------------
Description
Dispatcher's main purpose is to dispatch and execute actions 
from background thread to main thread.

To enable Dispatcher
1) Add Empty GameObject to Scene
2) Add Assets\Battlehub\Dispatcher\Dispatcher.cs C# Script as a component to this gameObject
3) Now you can use Dispatcher.Current.BeginInvoke function as follows:

----------------------------------------------------------------------
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Threading;
using Battlehub.Dispatcher

public class MyDispatcherTest : MonoBehaviour
{
    [SerializeField]
    private Text Output;

    private void Start()
    {
        for (int i = 0; i < 10; ++i)
        {
            Thread t = new Thread(ThreadFunction);
            t.Start("Dispatched from Thread " + i);
        }
    }
        
    private void ThreadFunction(object param)
    {
        Dispatcher.Current.BeginInvoke(() =>
        {
            Output.text += param;
            Output.text += Environment.NewLine;
        });
    }
}

----------------------------------------------------------------------
Action from above example will be executed on main thread
() =>
        {
            Output.text += param;
            Output.text += Environment.NewLine;
        }

----------------------------------------------------------------------
Static Properties:
public static Dispatcher Current { get; } - gets Dispatcher instance

Public Functions:
public void BeginInvoke(Action action) - dispatch and execute actions from background thread to main thread

-------------------------------------------------------------------
For additional step by step short video tutorial visit: https://www.youtube.com/watch?v=k5mbg5kVveU
If you have any questions regarding Dispatcher script feel free to contact Vadim.Andriyanov@outlook.com