using UnityEngine;
using Battlehub.UIControls.MenuControl;
using System;

namespace Battlehub.RTEditor
{
    public interface IContextMenu 
    {
        event EventHandler Opened;
        event EventHandler Closed;

        bool IsOpened
        {
            get;
        }
        void Open(MenuItemInfo[] items);
        void Close();
    }

    public class ContextMenu : MonoBehaviour, IContextMenu
    {
        [SerializeField]
        private Menu m_menu = null;

        [SerializeField]
        private RectTransform m_contextMenuArea = null;

        public event EventHandler Opened;
        public event EventHandler Closed;

        public bool IsOpened
        {
            get { return m_menu.IsOpened; }
        }

        private void Awake()
        {
            m_menu.Opened += OnMenuOpened;
            m_menu.Closed += OnMenuClosed;
        }

        private void OnDestroy()
        {
            if(m_menu != null)
            {
                m_menu.Opened -= OnMenuOpened;
                m_menu.Closed -= OnMenuClosed;
            }
            m_menu = null;
        }

        public void Open(MenuItemInfo[] items)
        {
            Canvas canvas = m_contextMenuArea.GetComponentInParent<Canvas>();
            Vector3 position;
            Vector2 pos = Input.mousePosition;
            
            Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

            if (!RectTransformUtility.RectangleContainsScreenPoint(m_contextMenuArea, pos, cam))
            {
                return;
            }

            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(m_contextMenuArea, pos, cam, out position))
            {
                m_menu.transform.position = position;
                m_menu.Items = items;
                m_menu.Open();
            }
        }

        public void Close()
        {
            m_menu.Close();
        }

        private void OnMenuOpened(object sender, EventArgs e)
        {
            Opened?.Invoke(this, EventArgs.Empty);
        }

        private void OnMenuClosed(object sender, EventArgs e)
        {
            Closed?.Invoke(this, EventArgs.Empty);
        }
    }
}

