using UnityEngine;
using UnityEngine.EventSystems;

namespace Battlehub.UIControls
{
    public class MultiTogglePanel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public MultiToggle Toggle
        {
            get;
            set;
        }

        private bool m_isPointerOver;

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
            if(!m_isPointerOver && Input.anyKey)
            {
                Hide();
            }
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            transform.SetParent(Toggle.transform);
            m_isPointerOver = false;
        }
    }
}
