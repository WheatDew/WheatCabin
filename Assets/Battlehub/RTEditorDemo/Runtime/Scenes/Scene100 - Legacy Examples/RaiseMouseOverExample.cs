using Battlehub.RTCommon;
using System.Collections.Generic;
using UnityEngine;

namespace Battlehub.RTEditor
{
    public class RaiseMouseOverExample : MonoBehaviour
    {
        private RuntimeWindow m_window;
        
        private void Awake()
        {
            m_window = GetComponentInChildren<RuntimeWindow>(true);
        }

        private void Update()
        {
            if(!m_window.IsPointerOver || m_window != m_window.Editor.ActiveWindow)
            {
                return;
            }

            RaycastHit hit;
            if(Physics.Raycast(m_window.Pointer, out hit))
            {
                if (m_window.Editor.Input.GetPointerDown(0))
                {
                    hit.collider.SendMessage("OnMouseDown", SendMessageOptions.DontRequireReceiver);
                }
                else if(m_window.Editor.Input.GetPointerUp(0))
                {
                    hit.collider.SendMessage("OnMouseUp", SendMessageOptions.DontRequireReceiver);
                }
                hit.collider.SendMessage("OnMouseOver", SendMessageOptions.DontRequireReceiver);
            }
        }
    }
}


