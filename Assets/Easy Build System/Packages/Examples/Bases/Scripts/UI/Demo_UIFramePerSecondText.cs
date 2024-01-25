/// <summary>
/// Project : Easy Build System
/// Class : Demo_UIFramePerSecondText.cs
/// Namespace : EasyBuildSystem.Examples.Bases.Scripts.UI
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using UnityEngine;
using UnityEngine.UI;

namespace EasyBuildSystem.Examples.Bases.Scripts.UI
{
    public class Demo_UIFramePerSecondText : MonoBehaviour
    {
        [SerializeField] Text m_FramePerSecondText;
        
        float m_DeltaTime;

        void Update()
        {
            m_DeltaTime += (Time.deltaTime - m_DeltaTime) * 0.1f;
            m_FramePerSecondText.text = "FPS " + Mathf.Ceil(1f / m_DeltaTime).ToString();
        }
    }
}