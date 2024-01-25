/// <summary>
/// Project : Easy Build System
/// Class : Demo_InputModule.cs
/// Namespace : EasyBuildSystem.Examples.Bases.Scripts
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using UnityEngine;
using UnityEngine.EventSystems;

#if EBS_INPUT_SYSTEM_SUPPORT
using UnityEngine.InputSystem.UI;
#endif

namespace EasyBuildSystem.Examples.Bases.Scripts
{
    [ExecuteInEditMode]
    public class Demo_InputModule : MonoBehaviour
    {
#if EBS_INPUT_SYSTEM_SUPPORT

    void OnEnable()
    {
        if (GetComponent<InputSystemUIInputModule>() == null)
        {
            gameObject.AddComponent<InputSystemUIInputModule>();
            DestroyImmediate(GetComponent<StandaloneInputModule>());
        }
    }

#else

        void OnEnable()
        {
            if (GetComponent<StandaloneInputModule>() == null)
            {
                gameObject.AddComponent<StandaloneInputModule>();
            }
        }

#endif
    }
}