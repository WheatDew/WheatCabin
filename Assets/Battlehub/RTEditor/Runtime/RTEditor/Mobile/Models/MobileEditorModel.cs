using Battlehub.RTCommon;
using Battlehub.UIControls;
using Battlehub.UIControls.DockPanels;
using Battlehub.UIControls.MenuControl;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Battlehub.RTEditor.Mobile.Models
{
    public interface IMobileEditorModel
    {
        event EventHandler<ValueChangedArgs<bool>> IsInspectorOpenedChanged;
        bool IsInspectorOpened
        {
            get;
            set;
        }

        event EventHandler<ValueChangedArgs<bool>> IsMainMenuOpenedChanged;
        bool IsMainMenuOpened
        {
            get;
            set;
        }

        MobileMenuItemModel[] MainMenuItems
        {
            get;
        }
    }

    public class MobileMenuItemModel
    {
        public class ValidationArgs
        {
            public bool IsValid
            {
                get;
                set;
            }

            public string Command
            {
                get;
                private set;
            }

            public bool HasChildren
            {
                get;
                private set;
            }

            public ValidationArgs(string command, bool hasChildren)
            {
                Command = command;
                IsValid = true;
                HasChildren = hasChildren;
            }
        }

        private Action<string> m_actionCallback;
        private Action<ValidationArgs> m_validateCallback;

        public string Text
        {
            get;
            private set;
        }

        public Sprite Icon
        {
            get;
            private set;
        }

        public string Command
        {
            get;
            private set;
        }

        public bool HasChildren
        {
            get { return Children != null && Children.Count > 0; }
        }

        public MobileMenuItemModel Parent
        {
            get;
            set;
        }


        public List<MobileMenuItemModel> Children
        {
            get;
            private set;
        }

        public MobileMenuItemModel(string text, Action<string> actionCallback, Action<ValidationArgs> validateCallback = null)
            : this(text, null, null, actionCallback, validateCallback)
        {
        }

        public MobileMenuItemModel(string text, Sprite icon, string command, Action<string> actionCallback, Action<ValidationArgs> validateCallback = null)
        {
            Text = text;
            Icon = icon != null ? icon : Resources.Load<Sprite>("Empty");
            Command = command;
            m_actionCallback = actionCallback;
            m_validateCallback = validateCallback != null ? validateCallback : args => { };
            Children = new List<MobileMenuItemModel>();
        }

        public void Action()
        {
            m_actionCallback?.Invoke(Command);
        }

        public bool Validate()
        {
            ValidationArgs arg = new ValidationArgs(Command, HasChildren);
            m_validateCallback?.Invoke(arg);
            return arg.IsValid;
        }
    }

    public class MobileEditorModel : MonoBehaviour, IMobileEditorModel
    {
        public event EventHandler<ValueChangedArgs<bool>> IsInspectorOpenedChanged;
        private bool m_isInspectorOpened;
        public bool IsInspectorOpened
        {
            get { return m_isInspectorOpened; }
            set
            {
                if (m_isInspectorOpened != value)
                {
                    m_isInspectorOpened = value;
                    if (m_inspectorRegion != null)
                    {
                        m_inspectorRegion.gameObject.SetActive(value);
                        Canvas.ForceUpdateCanvases();
                    }

                    IsInspectorOpenedChanged?.Invoke(this, new ValueChangedArgs<bool>(!m_isInspectorOpened, m_isInspectorOpened));
                }
            }
        }

        public event EventHandler<ValueChangedArgs<bool>> IsMainMenuOpenedChanged;
        private bool m_isMenuOpened;
   
        public bool IsMainMenuOpened
        {
            get { return m_isMenuOpened; }
            set
            {
                if(m_isMenuOpened != value)
                {
                    m_isMenuOpened = value;
                    if (m_isMenuOpened)
                    {
                        m_mainMenu = m_wm.CreatePopup(MobileWindowNames.MainMenu, false);
                        m_mainMenuWindow = m_mainMenu.GetComponent<RuntimeWindow>();

                        Region region = Region.FindRegion(m_mainMenu);
                        RectTransform rt = (RectTransform)region.transform;
                        rt.Stretch();

                        m_coCloseMainMenu = CoCloseMenu();
                        StartCoroutine(m_coCloseMainMenu);
                    }
                    else
                    {
                        if(m_mainMenu != null )
                        {
                            m_wm.DestroyWindow(m_mainMenu);
                            m_mainMenu = null;
                            m_mainMenuWindow = null;
                        }
                        
                        if(m_coCloseMainMenu != null)
                        {
                            StopCoroutine(m_coCloseMainMenu);
                            m_coCloseMainMenu = null;
                        }
                    }

                    IsMainMenuOpenedChanged?.Invoke(this, new ValueChangedArgs<bool>(!m_isMenuOpened, m_isMenuOpened));
                }
            }
        }

        public MobileMenuItemModel[] MainMenuItems
        {
            get;
            private set;
        }

        private IRTE m_rte;
        private IWindowManager m_wm;
        private Transform m_inspector;
        private Region m_inspectorRegion;

        private Transform m_mainMenu;
        private RuntimeWindow m_mainMenuWindow;
        private IEnumerator m_coCloseMainMenu;

        private void Awake()
        {
            IOC.RegisterFallback<IMobileEditorModel>(this);

            m_rte = IOC.Resolve<IRTE>();
            m_wm = IOC.Resolve<IWindowManager>();
            m_wm.WindowDestroyed += OnWindowDestroyed;
            m_wm.AfterLayout += OnAfterLayout;

            m_inspector = m_wm.GetWindow(BuiltInWindowNames.Inspector);
            if (m_inspector != null)
            {
                m_inspectorRegion = m_inspector.GetComponentsInParent<Region>(true).FirstOrDefault();
                m_inspectorRegion.gameObject.SetActive(IsInspectorOpened);
            }

            MainMenuItems = GetMainMenuItems();
        }

        private MobileMenuItemModel[] GetMainMenuItems()
        {
            var menuToMenuItems = MenuCreator.CreateMenuItems();
            var menuItemsList = new List<MenuItemInfo>();
            foreach (var kvp in menuToMenuItems)
            {
                if (kvp.Value.Count() == 0)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(kvp.Key))
                {
                    foreach (MenuItemInfo menuItem in kvp.Value)
                    {
                        menuItemsList.Add(menuItem);
                    }
                }
                else
                {
                    MenuItemInfo parentItem = new MenuItemInfo
                    {
                        Text = kvp.Key,
                        Path = kvp.Key
                    };
                    menuItemsList.Add(parentItem);

                    foreach (MenuItemInfo menuItem in kvp.Value)
                    {
                        menuItem.Path = $"{kvp.Key}/{menuItem.Path}";
                        menuItemsList.Add(menuItem);
                    }
                } 
            }

            var menuItems = menuItemsList.ToArray();
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

            return menuItemToModel.Values.Where(item => item.Parent == null).ToArray();
        }

        private void OnDestroy()
        {
            IOC.UnregisterFallback<IMobileEditorModel>(this);

            if(m_wm != null)
            {
                m_wm.WindowDestroyed -= OnWindowDestroyed;
                m_wm.AfterLayout -= OnAfterLayout;
                m_wm = null;
            }

            if (m_coCloseMainMenu != null)
            {
                StopCoroutine(m_coCloseMainMenu);
                m_coCloseMainMenu = null;
            }

            m_inspector = null;
            m_inspectorRegion = null;
            m_rte = null;
        }

        private void OnWindowDestroyed(Transform window)
        {
            if (window == m_mainMenu)
            {
                IsMainMenuOpened = false;
            }
        }

        private void OnAfterLayout(IWindowManager wm)
        {
            m_inspector = m_wm.GetWindow(BuiltInWindowNames.Inspector);
            if (m_inspector != null)
            {
                m_inspectorRegion = m_inspector.GetComponentsInParent<Region>(true).FirstOrDefault();
                m_inspectorRegion.gameObject.SetActive(IsInspectorOpened);
            }
        }

        private bool m_closing;
        private IEnumerator CoCloseMenu()
        {
            while (IsMainMenuOpened)
            {
                yield return null;
                if (!m_mainMenuWindow.IsPointerOver && m_rte.Input.IsAnyKeyDown())
                {
                    m_closing = true;
                }
                
                if(m_closing && !m_rte.Input.IsAnyKey())
                {
                    m_closing = false;
                    IsMainMenuOpened = false;
                }
            }
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
    }
}
