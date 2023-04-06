using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Battlehub.UIControls.MenuControl
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class MenuDefinitionAttribute : Attribute
    {
        public string SceneName
        {
            get;
            private set;
        }

        public int Order
        {
            get;
            private set;
        }
       
        public MenuDefinitionAttribute(int order = 0, string sceneName = null)
        {
            Order = order;
            SceneName = sceneName;
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class MenuCommandAttribute : Attribute
    {
        public string Path
        {
            get;
            private set;
        }

        public string IconPath
        {
            get;
            private set;
        }

        public bool Validate
        {
            get;
            private set;
        }

        public bool Hide
        {
            get;
            private set;
        }

        public int Priority
        {
            get;
            private set;
        }

  
        [Obsolete]
        public bool RequiresInstance
        {
            get;
            private set;
        }

        public MenuCommandAttribute(string path, bool validate = false, bool hide = false, int priority = int.MaxValue, bool requiresInstance = false)
        {
            Path = path;
            Validate = validate;
            Hide = hide;
            Priority = priority;
        }

        public MenuCommandAttribute(string path, string iconPath, bool requiresInstance = false)
        {
            Path = path;
            Validate = false;
            IconPath = iconPath;
            Priority = int.MaxValue;
        }

        public MenuCommandAttribute(string path, string iconPath, int priority, bool requiresInstance = false)
        {
            Path = path;
            Validate = false;
            IconPath = iconPath;
            Priority = priority;
        }
    }

    public class MenuCreatingEventArgs : EventArgs
    {
        public string MenuName
        {
            get;
            private set;
        }

        public List<MenuItemInfo> MenuItems
        {
            get;
            private set;
        }

        public MenuCreatingEventArgs(string menuName, List<MenuItemInfo> menuItems)
        {
            MenuName = menuName;
            MenuItems = menuItems;
        }
    }

    [DefaultExecutionOrder(-25)]
    public class MenuCreator : MonoBehaviour
    {
        public event EventHandler<MenuCreatingEventArgs> MenuCreating;

        [SerializeField]
        private GameObject m_topMenu = null;

        [SerializeField]
        private GameObject m_menuPanel = null;

        [SerializeField]
        private MainMenuButton m_menuButtonPrefab = null;

        [SerializeField]
        private Menu m_menuPrefab = null;

        private class MenuItemWithPriority
        {
            public MenuItemInfo Info;
            public int Priority;

            public MenuItemWithPriority(MenuItemInfo menuItemInfo, int priority)
            {
                Info = menuItemInfo;
                Priority = priority;
            }

            public MenuItemWithPriority()
            {
                Info = new MenuItemInfo();
                Priority = int.MaxValue;
            }
        }

        private void Awake()
        {
            if (m_menuPanel == null)
            {
                m_menuPanel = gameObject;
            }

            if (m_topMenu == null)
            {
                m_topMenu = gameObject;
            }

            if (m_menuButtonPrefab == null)
            {
                Debug.LogError("Set Menu Button Prefab");
                return;
            }

            if (m_menuPrefab == null)
            {
                Debug.LogError("Set Menu Prefab");
                return;
            }



            bool wasButtonPrefabActive = m_menuButtonPrefab.gameObject.activeSelf;
            bool wasMenuPrefabActive = m_menuPrefab.gameObject.activeSelf;

            Dictionary<string, Menu> menuDictionary = new Dictionary<string, Menu>();
            Dictionary<string, List<MenuItemWithPriority>> menuItemsDictionary = new Dictionary<string, List<MenuItemWithPriority>>();
            Menu[] menus = m_menuPanel.GetComponentsInChildren<Menu>(true);
            for (int i = 0; i < menus.Length; ++i)
            {
                if (!menuDictionary.ContainsKey(menus[i].name))
                {
                    menuDictionary.Add(menus[i].name, menus[i]);

                    if (menus[i].Items != null)
                    {
                        List<MenuItemWithPriority> menuItemsWithPriority = new List<MenuItemWithPriority>();
                        for (int priority = 0; priority < menus[i].Items.Length; ++priority)
                        {
                            MenuItemInfo menuItemInfo = menus[i].Items[priority];
                            menuItemsWithPriority.Add(new MenuItemWithPriority(menuItemInfo, priority));
                        }
                        menuItemsDictionary.Add(menus[i].name, menuItemsWithPriority);
                    }
                    else
                    {
                        menuItemsDictionary.Add(menus[i].name, new List<MenuItemWithPriority>());
                    }
                }
            }

            Dictionary<string, MainMenuButton> menuButtonDictionary = new Dictionary<string, MainMenuButton>();
            MainMenuButton[] menuButtons = m_topMenu.GetComponentsInChildren<MainMenuButton>(true);
            for (int i = 0; i < menuButtons.Length; ++i)
            {
                if (!menuButtonDictionary.ContainsKey(menuButtons[i].name))
                {
                    menuButtonDictionary.Add(menuButtons[i].name, menuButtons[i]);
                }
            }

            string[] menuNames = CreateMenuItems(menuItemsDictionary);
            foreach (string menuName in menuNames)
            {
                if(!menuDictionary.ContainsKey(menuName))
                {
                    bool wasActive = m_menuPrefab.gameObject.activeSelf;
                    m_menuPrefab.gameObject.SetActive(false);

                    Menu menu = Instantiate(m_menuPrefab, m_menuPanel.transform, false);
                    menu.Items = null;

                    m_menuPrefab.gameObject.SetActive(wasActive);
                    menuDictionary.Add(menuName, menu);
                }
              
                if(!menuButtonDictionary.ContainsKey(menuName))
                {
                    bool wasActive = m_menuButtonPrefab.gameObject.activeSelf;
                    m_menuButtonPrefab.gameObject.SetActive(false);

                    MainMenuButton btn = CreateMenuButton(menuName);
                    btn.Menu = menuDictionary[menuName];
                    btn.gameObject.SetActive(true);

                    m_menuButtonPrefab.gameObject.SetActive(wasActive);
                    menuButtonDictionary.Add(menuName, btn);
                }
            }

            m_menuPrefab.gameObject.SetActive(wasMenuPrefabActive);
            m_menuButtonPrefab.gameObject.SetActive(wasButtonPrefabActive);

            foreach (KeyValuePair<string, List<MenuItemWithPriority>> kvp in menuItemsDictionary)
            {
                IEnumerable<MenuItemInfo> menuItemsOrderedByPriority = kvp.Value.OrderBy(m => m.Priority).Select(m => m.Info);
                if (MenuCreating != null)
                {
                    List<MenuItemInfo> menuItems = menuItemsOrderedByPriority.ToList();
                    MenuCreating(this, new MenuCreatingEventArgs(kvp.Key, menuItems));
                    menuDictionary[kvp.Key].SetMenuItems(menuItems.ToArray(), false);
                }
                else
                {
                    menuDictionary[kvp.Key].SetMenuItems(menuItemsOrderedByPriority.ToArray(), false);
                }
            }
        }

        public static Dictionary<string, MenuItemInfo> GetPathToMenuItem(MenuItemInfo[] menuItems)
        {
            Dictionary<string, MenuItemInfo> pathToMenuItem = menuItems.ToDictionary(item => item.Path);
            foreach (var path in pathToMenuItem.Keys.ToArray())
            {
                string[] pathParts = path.Split('/'); 
                if(pathParts.Length == 1)
                {
                    continue;
                }

                for (int i = 1; i < pathParts.Length; i++)
                {
                    string parentPath = string.Join("/", pathParts.Take(i));
                    if (!pathToMenuItem.TryGetValue(parentPath, out MenuItemInfo menuItem))
                    {
                        menuItem = new MenuItemInfo { Path = parentPath };
                        pathToMenuItem.Add(parentPath, menuItem);
                    }
                }   
            }

            return pathToMenuItem;
        }

        public static Dictionary<string, List<string>> GetParentPathToMenuItems(MenuItemInfo[] menuItems)
        {
            Dictionary<string, List<string>> parentPathToMenuItems = new Dictionary<string, List<string>>();
            for (int i = 0; i < menuItems.Length; ++i)
            {
                MenuItemInfo menuItem = menuItems[i];
                string[] pathParts = menuItem.Path.Replace("\\", "/").Split('/');
                if (pathParts.Length == 1)
                {
                    continue;
                }

                string parentPath = string.Join("/", pathParts, 0, pathParts.Length - 1);
                if (!parentPathToMenuItems.TryGetValue(parentPath, out List<string> children))
                {
                    children = new List<string>();
                    parentPathToMenuItems.Add(parentPath, children);
                }

                children.Add(menuItem.Path);
            }

            return parentPathToMenuItems;
        }

        public static Dictionary<string, IEnumerable<MenuItemInfo>> CreateMenuItems()
        {
            Dictionary<string, List<MenuItemWithPriority>> menuItemsDictionary = new Dictionary<string, List<MenuItemWithPriority>>();
            CreateMenuItems(menuItemsDictionary);
            return menuItemsDictionary.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.OrderBy(m => m.Priority).Select(m => m.Info));
        }

        private static string[] CreateMenuItems(Dictionary<string, List<MenuItemWithPriority>> menuItemsDictionary)
        {
            HashSet<string> menuNamesHs = new HashSet<string>();
            List<string> menuNames = new List<string>();

            Scene activeScene = SceneManager.GetActiveScene();
            string activeSceneName = activeScene.name.ToLower();

            List<Assembly> assemblies = new List<Assembly>();
            foreach (string assemblyName in KnownAssemblies.Names)
            {
                var asName = new AssemblyName();
                asName.Name = assemblyName;
                try
                {
                    Assembly asm = Assembly.Load(asName);
                    assemblies.Add(asm);
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e.ToString());
                }
            }

            IEnumerable<KeyValuePair<Type, object>> menuDefinitions = assemblies.SelectMany(asm => asm.GetTypesWithAttribute(typeof(MenuDefinitionAttribute))).OrderBy(kvp => ((MenuDefinitionAttribute)kvp.Value).Order);
            foreach (KeyValuePair<Type, object> kvp in menuDefinitions)
            {
                MenuDefinitionAttribute menuDef = (MenuDefinitionAttribute)kvp.Value;
                string sceneName = menuDef.SceneName;
                if (!string.IsNullOrEmpty(sceneName) && sceneName.ToLower() != activeSceneName)
                {
                    continue;
                }

                Type menuDefType = kvp.Key;
                MethodInfo[] methods = menuDefType.GetMethods(BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public);

                bool canFindInstance = typeof(Component).IsAssignableFrom(menuDefType);
                UnityEngine.Object instance = null;

                for (int i = 0; i < methods.Length; ++i)
                {
                    MethodInfo mi = methods[i];
                    MenuCommandAttribute cmd = (MenuCommandAttribute)mi.GetCustomAttributes(typeof(MenuCommandAttribute), true).FirstOrDefault();
                    if (cmd == null || string.IsNullOrEmpty(cmd.Path))
                    {
                        continue;
                    }

                    string[] pathParts = cmd.Path.Split('/');
                    if (pathParts.Length < 1)
                    {
                        continue;
                    }

                    string menuName = pathParts[0];
                    if (!mi.IsStatic)
                    {
                        if (!canFindInstance)
                        {
                            continue;
                        }

                        if (instance == null)
                        {
                            instance = FindObjectOfType(menuDefType);
                            if (instance == null)
                            {
                                canFindInstance = false;
                                continue;
                            }
                        }
                    }


                    if (!menuItemsDictionary.ContainsKey(menuName))
                    {
                        menuItemsDictionary.Add(menuName, new List<MenuItemWithPriority>());
                    }

                    if(menuNamesHs.Add(menuName))
                    {
                        menuNames.Add(menuName);
                    }

                    if (pathParts.Length == 1)
                    {
                        if (cmd.Hide)
                        {
                            menuItemsDictionary[menuName] = null;
                        }
                    }

                    if (pathParts.Length < 2)
                    {
                        continue;
                    }

                    string path = string.Join("/", pathParts.Skip(1));
                    List<MenuItemWithPriority> menuItems = menuItemsDictionary[menuName];
                    if(menuItems == null)
                    {
                        continue;
                    }
                    MenuItemWithPriority menuItem = menuItems.Where(item => item.Info.Path == path).FirstOrDefault();
                    if (menuItem == null)
                    {
                        menuItem = new MenuItemWithPriority();
                        menuItems.Add(menuItem);
                    }

                    menuItem.Info.Path = string.Join("/", pathParts.Skip(1));
                    menuItem.Info.Icon = !string.IsNullOrEmpty(cmd.IconPath) ? Resources.Load<Sprite>(cmd.IconPath) : null;
                    menuItem.Info.Text = pathParts.Last();

                    if (cmd.Validate)
                    {
                        try
                        {
                            Func<bool> validate = (Func<bool>)mi.CreateDelegate(typeof(Func<bool>), instance);
                            menuItem.Info.Validate = new MenuItemValidationEvent();
                            menuItem.Info.Validate.AddListener(new UnityAction<MenuItemValidationArgs>(args => args.IsValid = validate()));
                        }
                        catch (ArgumentException e)
                        {
                            Debug.LogWarning("Method signature is invalid. bool Func() is expected. Path -> " + string.Join("/", pathParts) + "; Type -> " + menuDefType.FullName + "; Method -> " + mi.Name + "; Exception -> " + e.ToString());
                        }
                    }
                    else
                    {
                        try
                        {
                            Action action = (Action)mi.CreateDelegate(typeof(Action), instance);
                            menuItem.Info.Action = new MenuItemEvent();
                            menuItem.Info.Action.AddListener(new UnityAction<string>(args => action()));
                        }
                        catch (ArgumentException e)
                        {
                            Debug.LogWarning("Method signature is invalid. void Action() is expected. Path -> " + string.Join("/", pathParts) + "; Type -> " + menuDefType.FullName + "; Method -> " + mi.Name + "; Exception -> " + e.ToString());
                        }
                    }

                    if (cmd.Hide)
                    {
                        menuItems.Remove(menuItem);
                    }

                    menuItem.Priority = cmd.Priority;
                }
            }

            foreach (string menuName in menuItemsDictionary.Keys.ToArray())
            {
                if (menuItemsDictionary[menuName] == null)
                {
                    menuItemsDictionary.Remove(menuName);
                    menuNamesHs.Remove(menuName);
                    menuNames.Remove(menuName);
                }
            }

            return menuNames.ToArray();
        }

        public MainMenuButton CreateMenu(string menuName)
        {
            bool wasButtonPrefabActive = m_menuButtonPrefab.gameObject.activeSelf;
            bool wasMenuPrefabActive = m_menuPrefab.gameObject.activeSelf;

            m_menuButtonPrefab.gameObject.SetActive(false);
            m_menuPrefab.gameObject.SetActive(false);

            Menu menu = Instantiate(m_menuPrefab, m_menuPanel.transform, false);
            menu.name = menuName;
            menu.Items = null;

            MainMenuButton btn = CreateMenuButton(menuName);
            btn.Menu = menu;
            btn.gameObject.SetActive(true);

            m_menuPrefab.gameObject.SetActive(wasMenuPrefabActive);
            m_menuButtonPrefab.gameObject.SetActive(wasButtonPrefabActive);
            return btn;
        }

        private MainMenuButton CreateMenuButton(string menuName)
        {
            MainMenuButton btn = Instantiate(m_menuButtonPrefab, m_topMenu.transform, false);
            btn.name = menuName;
            btn.Text = menuName;
            
            Text txt = btn.GetComponentInChildren<Text>(true);
            if (txt != null)
            {
                txt.text = menuName;
            }

            return btn;
        }
    }
}
