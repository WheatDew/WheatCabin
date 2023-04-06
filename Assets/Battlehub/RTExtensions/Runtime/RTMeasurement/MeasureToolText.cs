using Battlehub.RTCommon;
using TMPro;
using UnityEngine;

namespace Battlehub.RTMeasurement
{
    public class MeasureToolText : MonoBehaviour
    {
        private IRTE m_editor;
        private TextMeshPro m_text;

        [SerializeField]
        private float m_fontSize = 14;

        private void Awake()
        {
            m_editor = IOC.Resolve<IRTE>();
            m_text = GetComponent<TextMeshPro>();
            m_text.enabled = false;
            m_text.fontSize = 0;
        }

        private void Update()
        {
            RuntimeWindow activeWindow = m_editor.ActiveWindow;
            
            if (activeWindow != null && activeWindow.WindowType == RuntimeWindowType.Scene)
            {
                Camera camera = activeWindow.Camera;
                if (camera != null)
                {
                    float scale = GraphicsUtility.GetScreenScale(transform.position, camera) / 10;
                    transform.rotation = Quaternion.LookRotation(camera.transform.forward);
                    m_text.fontSize = m_fontSize * scale;
                    m_text.enabled = true;
                }
                else
                {
                    m_text.fontSize = 0;
                    m_text.enabled = false;
                }
            }
            else
            {
                m_text.fontSize = 0;
                m_text.enabled = false;
            }
        }
    }
}


