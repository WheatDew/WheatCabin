using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Battlehub.RTCommon
{
    public interface IUIRaycaster
    {   
        Camera eventCamera
        {
            get;
        }
        void Raycast(List<RaycastResult> results);
        void Raycast(PointerEventData eventData, List<RaycastResult> results);
    }

    public class RTEUIRaycaster : MonoBehaviour, IUIRaycaster
    {
        public Camera eventCamera
        {
            get { return m_raycasters[0].eventCamera; }
        }

        [SerializeField]
        private BaseRaycaster[] m_raycasters = null;
        private IInput m_input;
        private IRTE m_editor;
     
        private void Awake()
        {
            m_editor = IOC.Resolve<IRTE>();
            m_input = m_editor.Input;
            if(m_raycasters == null || m_raycasters.Length == 0 || m_raycasters[0] == null)
            {
                BaseRaycaster raycaster = gameObject.GetComponent<BaseRaycaster>();
                if(raycaster == null)
                {
                    GraphicRaycaster graphicsRaycaster = gameObject.AddComponent<GraphicRaycaster>();
                    graphicsRaycaster.blockingObjects = GraphicRaycaster.BlockingObjects.None;
                    raycaster = graphicsRaycaster; 
                    //raycaster.mask => ?
                }

                m_raycasters = new[]
                {
                    raycaster
                };
            }
        }

        public void Raycast(List<RaycastResult> results)
        {
            PointerEventData eventData = new PointerEventData(m_editor.EventSystem);
            eventData.position = m_input.GetPointerXY(0);
            for(int i = 0; i < m_raycasters.Length; ++i)
            {
                BaseRaycaster raycaster = m_raycasters[i];
                if(raycaster != null)
                {
                    raycaster.Raycast(eventData, results);
                }
            } 
        }

        public void Raycast(PointerEventData eventData, List<RaycastResult> results)
        {
            eventData.position = m_input.GetPointerXY(0);
            for (int i = 0; i < m_raycasters.Length; ++i)
            {
                BaseRaycaster raycaster = m_raycasters[i];
                if (raycaster != null)
                {
                    raycaster.Raycast(eventData, results);
                }
            }
        }
    }
}
