using Battlehub.RTCommon;
using Battlehub.UIControls.Common;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Battlehub.RTEditor
{
    public class BubbleUpPointerExit : MonoBehaviour, IPointerExitHandler
    {
        public void OnPointerExit(PointerEventData eventData)
        {
            if (transform.parent != null)
            {
                ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, eventData, ExecuteEvents.pointerExitHandler);
            }

        }
    }

    [DefaultExecutionOrder(-90)]
    public class WindowOverlay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private IRTE m_rte;
        private bool m_pointerOver;
        private RuntimeWindow m_activeWindow;
        
        [SerializeField]
        private bool m_alwaysActivate = false;

        private void Awake()
        {
            m_rte = IOC.Resolve<IRTE>();
            m_activeWindow = m_rte.ActiveWindow;
        }

        private void Start()
        {
            Image image = GetComponent<Image>();
            if (image == null)
            {
                image = gameObject.AddComponent<Image>();
                image.color = new Color(0, 0, 0, 0);
            }

            if (m_rte.TouchInput.IsTouchSupported)
            {
                //Quick fix for devices with touch input. IPointerExit is not raised for some reason
                foreach (Selectable selectable in GetComponentsInChildren<Selectable>(true))
                {
                    selectable.gameObject.AddComponent<BubbleUpPointerExit>();
                }
            }
        }

        private void Update()
        {
            if(m_activeWindow != null || m_alwaysActivate)
            {
                int touchCount = m_rte.TouchInput.TouchCount;
                if(m_pointerOver && (m_rte.Input.IsAnyKeyDown() || m_rte.Input.GetAxis(InputAxis.Z) != 0) && (touchCount == 0 || touchCount == 1))
                {
                    m_rte.ActivateWindow(null);
                }
            }
            m_activeWindow = m_rte.ActiveWindow;   
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if(!eventData.IsDefaultPointerId())
            {
                return;
            }

            m_pointerOver = true;
            //Touch input support
            if (m_rte.Input.IsAnyKeyDown()) 
            {
                m_rte.ActivateWindow(null);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            m_pointerOver = false;
        }
    }
}


