using Battlehub.RTCommon;
using Battlehub.RTEditor.Binding.Adapters;
using Battlehub.RTEditor.Mobile.Models;
using Battlehub.RTEditor.Mobile.ViewModels;
using Battlehub.UIControls.MenuControl;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Battlehub.RTEditor.Mobile
{
    public class MobileContextMenu : MonoBehaviour, IContextMenu
    {
        public bool IsOpened { get; private set; }

        public event EventHandler Opened;
        public event EventHandler Closed;

        private RuntimeWindow m_contextMenuWindow;
        private IRTE m_rte;
        private IMobileEditorModel m_editorModel;
        private IWindowManager m_wm;
        
        private void Start()
        {
            m_rte = IOC.Resolve<IRTE>();
            m_wm = IOC.Resolve<IWindowManager>();
            m_editorModel = IOC.Resolve<IMobileEditorModel>();
            if(m_editorModel != null)
            {
                m_editorModel.IsInspectorOpenedChanged += OnIspectorIsInspectorOpenedChanged;
            }

            IOC.UnregisterFallback<IContextMenu>();
            IOC.RegisterFallback<IContextMenu>(this);
        }

        private void OnDestroy()
        {
            IOC.UnregisterFallback<IContextMenu>(this);

            if (m_rte != null)
            {
                m_rte.ActiveWindowChanged -= OnActiveWindowChanged;
                m_rte = null;
            }

            if(m_editorModel != null)
            {
                m_editorModel.IsInspectorOpenedChanged -= OnIspectorIsInspectorOpenedChanged;
            }

            m_wm = null;
        }

        private void Update()
        {
            if(m_contextMenuWindow != null && !m_contextMenuWindow.IsPointerOver && m_rte.Input.IsAnyKeyDown())
            {
                Close(true);
            }
        }

        private void OnActiveWindowChanged(RuntimeWindow window)
        {
            Close(false);
        }

        private void OnIspectorIsInspectorOpenedChanged(object sender, ValueChangedArgs<bool> e)
        {
            Close(true);
        }


        public void Open(MenuItemInfo[] menuItems)
        {
            Close();

            m_contextMenuWindow = CreatePopup(m_rte.ActiveWindow);
            m_rte.ActiveWindowChanged -= OnActiveWindowChanged;
            m_rte.ActiveWindowChanged += OnActiveWindowChanged;

            var pathToMenuItem = MenuCreator.GetPathToMenuItem(menuItems);
            var parentPathToMenuItems = MenuCreator.GetParentPathToMenuItems(pathToMenuItem.Values.ToArray());

            Dictionary<string, MobileMenuItemModel> menuItemToModel = new Dictionary<string, MobileMenuItemModel>();
            foreach (var kvp in pathToMenuItem)
            {
                menuItemToModel.Add(kvp.Key, ToMenuItemModel(kvp.Value));
            }

            foreach (var kvp in parentPathToMenuItems)
            {
                MobileMenuItemModel parentViewModel = menuItemToModel[kvp.Key];
                foreach (string childPath in kvp.Value)
                {
                    MobileMenuItemModel childViewModel = menuItemToModel[childPath];
                    parentViewModel.Children.Add(childViewModel);
                    childViewModel.Parent = parentViewModel;
                }
            }

            MobileMenuViewModel viewModel = m_contextMenuWindow.GetComponentInChildren<MobileMenuViewModel>();
            viewModel.MenuItemModels = menuItemToModel.Values.Where(item => item.Parent == null).ToArray();
            IsOpened = true;
            Opened?.Invoke(this, EventArgs.Empty);
        }


        private MobileMenuItemModel ToMenuItemModel(MenuItemInfo menuItem)
        {
            string text = menuItem.Text;
            if (string.IsNullOrEmpty(text))
            {
                text = menuItem.Path.Split('/').LastOrDefault();
            }

            return new MobileMenuItemModel(
                text, menuItem.Icon, menuItem.Command,
                cmd => menuItem.Action?.Invoke(cmd),
                args =>
                {
                    MenuItemValidationArgs _args = new MenuItemValidationArgs(args.Command, args.HasChildren);
                    menuItem.Validate?.Invoke(_args);
                    args.IsValid = _args.IsValid;
                });
        }

        public void Close()
        {
            Close(true);
        }

        public void Close(bool force = false)
        {
            if (!IsOpened)
            {
                return;
            }

            if (m_contextMenuWindow != null && (!m_wm.IsActive(m_contextMenuWindow.transform) || force))
            {
                m_rte.ActiveWindowChanged -= OnActiveWindowChanged;
                m_wm.DestroyWindow(m_contextMenuWindow.transform);
                m_contextMenuWindow = null;
            }

            m_rte.ActivateWindow(null);

            IsOpened = false;
            Closed?.Invoke(this, EventArgs.Empty);
        }

      
        private RuntimeWindow CreatePopup(RuntimeWindow parentWindow)
        {
            RuntimeWindow rootWindow = parentWindow.transform.parent.GetComponentInParent<RuntimeWindow>();
            if(rootWindow != null)
            {
                parentWindow = rootWindow;
            }

            Rect rect = parentWindow.ViewRoot.rect;

            Vector3 position = parentWindow.ViewRoot.TransformPoint(rect.min);
            position = m_wm.PopupRoot.InverseTransformPoint(position);

            Transform contextMenu = m_wm.CreatePopup(MobileWindowNames.ContextMenu, position, false, rect.width, rect.height);
            return contextMenu.GetComponent<RuntimeWindow>();
        }

    }
}
