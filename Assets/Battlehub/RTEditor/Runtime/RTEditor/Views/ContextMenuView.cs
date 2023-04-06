using Battlehub.RTCommon;
using Battlehub.UIControls.MenuControl;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityWeld.Binding;

namespace Battlehub.RTEditor.Views
{
    [Binding]
    public class ContextMenuView : MonoBehaviour
    {
        private IEnumerable<MenuItemInfo> m_menuItems;

        public IEnumerable<MenuItemInfo> MenuItems
        {
            get { return m_menuItems; }
            set
            {
                if (m_menuItems != value)
                {
                    if(m_contextMenu != null)
                    {
                        m_contextMenu.Opened -= OnOpened;
                        m_contextMenu.Closed -= OnClosed;
                    }
                    
                    m_menuItems = value;
                    if(m_menuItems != null)
                    {
                        IsOpened = true;
                    }
                    else
                    {
                        IsOpened = false;
                    }
                    
                    if (m_contextMenu != null)
                    {
                        m_contextMenu.Opened += OnOpened;
                        m_contextMenu.Closed += OnClosed;
                    }

                }
            }
        }

        [NonSerialized]
        public UnityEvent IsOpenedChanged = new UnityEvent();
        public bool IsOpened
        {
            get { return m_contextMenu != null ? m_contextMenu.IsOpened : false; }
            set
            {
                if(m_contextMenu == null)
                {
                    return;
                }

                if(value)
                {
                    if(MenuItems != null && MenuItems.Any())
                    {
                        if(!m_contextMenu.IsOpened)
                        {
                            m_contextMenu.Open(MenuItems.ToArray());
                        }
                    }   
                }
                else
                {
                    if(m_contextMenu.IsOpened)
                    {
                        m_contextMenu.Close();
                    }
                }
            }
        }

        private IContextMenu m_contextMenu;
        private void Start()
        {
            m_contextMenu = IOC.Resolve<IContextMenu>();
            m_contextMenu.Opened += OnOpened;
            m_contextMenu.Closed += OnClosed;
        }

        private void OnDestroy()
        {
            if(m_contextMenu != null)
            {
                m_contextMenu.Opened -= OnOpened;
                m_contextMenu.Closed -= OnClosed;
                m_contextMenu = null;
            }            
        }

        private void OnOpened(object sender, EventArgs e)
        {
            IsOpenedChanged?.Invoke();
        }

        private void OnClosed(object sender, EventArgs e)
        {
            m_menuItems = null;
            IsOpenedChanged?.Invoke();
        }

    }

}
