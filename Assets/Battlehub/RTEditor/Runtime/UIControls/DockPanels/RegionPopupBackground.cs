using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Battlehub.UIControls.DockPanels
{
    [DefaultExecutionOrder(-2500)] //Make sure to execute before EventSystem
    public class RegionPopupBackground : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
    {
        public event EventHandler Close;

        private bool m_isPointerOver;

        private void Awake()
        {
            LayoutElement layoutElement = gameObject.AddComponent<LayoutElement>();
            layoutElement.ignoreLayout = true;

            Image image = gameObject.AddComponent<Image>();
            image.color = new Color(0, 0, 0, 0f);
            
            RectTransform rt = (RectTransform)transform;
            rt.Stretch();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            m_isPointerOver = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            m_isPointerOver = false;
        }

        private void Update()
        {
            if (m_isPointerOver)
            {
                if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
                {
                    Close?.Invoke(this, EventArgs.Empty);
                }
            }
        }


        public void OnPointerDown(PointerEventData eventData)
        {
            Close?.Invoke(this, EventArgs.Empty);
        }
    }
}
