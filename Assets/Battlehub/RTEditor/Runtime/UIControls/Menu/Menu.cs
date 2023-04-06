using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Battlehub.UIControls.MenuControl
{
    [Serializable]
    public class MenuItemValidationArgs
    {
        public bool IsValid
        {
            get;
            set;
        }

        public bool HasChildren
        {
            get;
            set;
        }

        public bool IsVisible
        {
            get;
            set;
        }

        public string Command
        {
            get;
            private set;
        }

        public MenuItemValidationArgs(string command, bool hasChildren)
        {
            IsValid = !hasChildren;
            IsVisible = true;
            Command = command;
            HasChildren = hasChildren;
        }
    }


    [Serializable]
    public class MenuItemValidationEvent : UnityEvent<MenuItemValidationArgs>
    {
    }

    [Serializable]
    public class MenuItemEvent : UnityEvent<string>
    {
    }


    [Serializable]
    public class MenuItemInfo
    {
        public string Path;
        public string Text;
        public Sprite Icon;
        public int PrefabIndex;

        public string Command;
        public MenuItemEvent Action;
        public MenuItemValidationEvent Validate;

        public bool IsHidden;
        public bool IsOn;

        [HideInInspector, Obsolete("Use IsHidden instead")]
        public bool IsVisible = true;
    }


    public class Menu : MonoBehaviour
    {
        public event EventHandler Opened;
        public event EventHandler Closed;

        [SerializeField]
        private MenuItemInfo[] m_items = null;
        public MenuItemInfo[] Items
        {
            get { return m_items; }
            set
            {
                SetMenuItems(value, true);
            }
        }

        public void SetMenuItems(MenuItemInfo[] menuItems, bool databind = true)
        {
            m_items = menuItems;
            if(databind)
            {
                DataBind();
            }
            
        }

        [SerializeField, HideInInspector, Obsolete/*26.01.2021*/] 
        private MenuItem m_menuItemPrefab = null;

        [SerializeField]
        private MenuItem[] m_menuItemPrefabs = null;

        [SerializeField]
        private RectTransform m_anchor = null;
        public RectTransform Anchor
        {
            get { return m_anchor; }
            set { m_anchor = value; }
        }

        [SerializeField]
        private Vector2 m_anchorOffset = Vector2.zero;
        public Vector2 AnchorOffset
        {
            get { return m_anchorOffset; }
            set { m_anchorOffset = value; }
        }
     
        [SerializeField]
        private Transform m_panel = null;

        private Transform m_root;
        private Transform Root
        {
            get
            {
                if(m_root == null)
                {
                    m_root = transform.parent;
                }
                return m_root;
            }
        }

        private int m_depth;
        public int Depth
        {
            get { return m_depth; }
            set { m_depth = value; }
        }

        private MenuItem m_child;
        public MenuItem Child
        {
            get { return m_child; }
            set
            {
                if(m_child != null && m_child != value && m_child.Submenu != null)
                {
                    MenuItem oldChild = m_child;
                    m_child = value;
                    oldChild.Unselect();
                }
                else
                {
                    m_child = value;
                }

                if(m_child != null)
                {
                    m_child.Select(true);
                }
            }
        }

        private MenuItem m_parent;
        public MenuItem Parent
        {
            get { return m_parent; }
            set
            {
                m_parent = value;
            }
        }

        public int ActualItemsCount
        {
            get { return m_panel.childCount; }
        }

        public bool IsOpened
        {
            get { return gameObject.activeSelf; }
        }

        [SerializeField]
        private CanvasGroup m_canvasGroup = null;

        [SerializeField]
        private float FadeInSpeed = 2;

        private bool m_skipUpdate;

        private void Awake()
        {
            if (m_panel == null)
            {
                m_panel = transform;
            }

            if (m_canvasGroup != null)
            {
                m_canvasGroup.alpha = 0;
            }

            CopyPrefabs();
        }

    
        private void OnDestroy()
        {
            if(Closed != null)
            {
                Closed(this, EventArgs.Empty);
            }
        }

        private void CopyPrefabs()
        {
            if (m_menuItemPrefabs.Length == 0)
            {
                #pragma warning disable CS0612
                if (m_menuItemPrefab == null)
                {
                    Debug.LogError("Add at least 1 MenuItem prefab to Menu Item Prefabs array");
                }
                else
                {
                    m_menuItemPrefabs = new[] { m_menuItemPrefab };
                }
                #pragma warning restore CS0612
            }
        }

        private void DataBind()
        {
            Clear();
            CopyPrefabs();

            Dictionary<string, MenuItemInfo> pathToItem = new Dictionary<string, MenuItemInfo>();
            Dictionary<string, List<MenuItemInfo>> pathToChildren = new Dictionary<string, List<MenuItemInfo>>();
            if (m_items != null)
            {
                for (int i = 0; i < m_items.Length; ++i)
                {
                    MenuItemInfo menuItemInfo = m_items[i];
                    if (string.IsNullOrEmpty(menuItemInfo.Path) || menuItemInfo.IsHidden)
                    {
                        continue;
                    }

                    menuItemInfo.Path = menuItemInfo.Path.Replace("\\", "/");
                    string[] pathParts = menuItemInfo.Path.Split('/');
                    if (pathParts.Length == m_depth + 1)
                    {
                        if (string.IsNullOrEmpty(menuItemInfo.Text) || pathParts.Length > 1)
                        {
                            menuItemInfo.Text = pathParts[m_depth];
                        }
                        pathToItem[pathParts[m_depth]] = menuItemInfo;
                    }
                    else
                    {
                        string path = string.Join("/", pathParts, 0, m_depth + 1);
                        List<MenuItemInfo> childrenList;
                        if (!pathToChildren.TryGetValue(path, out childrenList))
                        {
                            childrenList = new List<MenuItemInfo>();
                            pathToChildren.Add(path, childrenList);
                        }

                        if (!pathToItem.ContainsKey(pathParts[m_depth]))
                        {
                            pathToItem[pathParts[m_depth]] = new MenuItemInfo
                            {
                                Text = pathParts[m_depth],
                                Path = path
                            };
                        }

                        if (string.IsNullOrEmpty(menuItemInfo.Text) || pathParts.Length > 1)
                        {
                            menuItemInfo.Text = pathParts[m_depth + 1];
                        }
                        childrenList.Add(menuItemInfo);
                    }
                }
            }

            foreach (MenuItemInfo menuItemInfo in pathToItem.Values)
            {
                MenuItem menuItemPrefab = GetMenuItemPrefab(menuItemInfo);
                MenuItem menuItem = Instantiate(menuItemPrefab, m_panel, false);
                menuItem.name = "MenuItem";
                menuItem.Depth = Depth + 1;
                menuItem.Root = Root;

                List<MenuItemInfo> childrenList;
                if (pathToChildren.TryGetValue(menuItemInfo.Path, out childrenList))
                {
                    menuItem.Children = childrenList.ToArray();
                }

                menuItem.Item = menuItemInfo;
            }
        }

        private void Clear()
        {
            foreach (Transform child in m_panel)
            {
                Destroy(child.gameObject);
            }
            m_panel.DetachChildren();
        }

        public void Open()
        {
            m_skipUpdate = true;

            gameObject.SetActive(true);

            RectTransform anchor = m_anchor;

            if (anchor != null)
            {
                Vector3[] corners = new Vector3[4];
                anchor.GetWorldCorners(corners);
                transform.position = corners[0];

                Vector3 lp = transform.localPosition;
                lp.z = 0;
                transform.localPosition = lp + (Vector3)m_anchorOffset;
            }
            
            DataBind();
            
            //if(m_anchor == null)
            {
                Fit();
            }   
         
            if(Opened != null)
            {
                Opened(this, EventArgs.Empty);
            }
        }

        public void Close()
        {
            Clear();
            gameObject.SetActive(false);

            if(Closed != null)
            {
                Closed(this, EventArgs.Empty);
            }
        }

        
        private Vector2 CalculateMenuSize()
        {
            HashSet<string> rootPathHs = new HashSet<string>();
            Vector2 size = Vector2.zero;
            for (int i = 0; i < m_items.Length; ++i)
            {
                string rootPath = m_items[i].Path;
                if (rootPath.Contains('/'))
                {
                    rootPath = rootPath.Split('/')[0];
                }
               
                if (!rootPathHs.Add(rootPath))
                {
                    continue;
                }
                MenuItem menuItemPrefab = GetMenuItemPrefab(m_items[i]);
                RectTransform rt = menuItemPrefab.GetComponent<RectTransform>();
                size.x = Mathf.Max(size.x, rt.rect.width);
                size.y += rt.rect.height;
            }

            return size;
        }

        private MenuItem GetMenuItemPrefab(MenuItemInfo menuItemInfo)
        {
            MenuItem menuItemPrefab = null;

            int prefabIndex = 0;
            if(menuItemInfo != null)
            {
                prefabIndex = menuItemInfo.PrefabIndex;
            }
            if (0 <= prefabIndex && prefabIndex < m_menuItemPrefabs.Length)
            {
                menuItemPrefab = m_menuItemPrefabs[prefabIndex];
            }
            if (menuItemPrefab == null)
            {
                Debug.LogWarning("Using default menu item prefab");
                menuItemPrefab = m_menuItemPrefabs.Where(prefabs => prefabs != null).FirstOrDefault();
            }

            return menuItemPrefab;
        }

        private void Fit()
        {
            RectTransform rootRT = (RectTransform)Root;
            Vector3 position = rootRT.InverseTransformPoint(transform.position);

            Vector2 topLeft = -Vector2.Scale(rootRT.rect.size, rootRT.pivot);
            Vector2 size = CalculateMenuSize();

            float offset = 3;
            if(m_anchor != null)
            {
                offset = 0;
            }

            if (position.x + size.x - offset > topLeft.x + rootRT.rect.width)
            {
                position.x = position.x - size.x - offset;
            }
            else
            {
                position.x += offset;
            }

            if (position.y - size.y < topLeft.y)
            {
                if(m_anchor != null)
                {
                    Vector3[] corners = new Vector3[4];
                    m_anchor.GetWorldCorners(corners);

                    Vector3 anchorTopLeft = rootRT.InverseTransformPoint(corners[1]);
                    position.y = anchorTopLeft.y + size.y;
                }
                else
                {
                    position.y -= (position.y - size.y) - topLeft.y;
                }
            }

            transform.position = rootRT.TransformPoint(position);

            Vector3 lp = transform.localPosition;
            lp.z = 0;
            transform.localPosition = lp;
        }

        private void LateUpdate()
        {
            if(m_skipUpdate)
            {
                m_skipUpdate = false;
                return;
            }

            if(m_canvasGroup != null && m_canvasGroup.alpha < 1)
            {
                m_canvasGroup.alpha += Time.deltaTime * FadeInSpeed;
            }

            if(Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2) || Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
            {
                if(m_child == null)
                {
                    MenuItem parentMenuItem = m_parent;
                    while (parentMenuItem != null && !parentMenuItem.IsPointerOver)
                    {
                        Menu parentMenu = parentMenuItem.GetComponentInParent<Menu>();
                        if(parentMenu == null)
                        {
                            Destroy(gameObject);
                            return;
                        }

                        //Commented out 02.04.2021 -> prevents menu close
                        //if(parentMenu.Child != null && parentMenu.Child.Submenu != this)
                        //{
                        //    break;
                        //}

                        parentMenuItem = parentMenu.m_parent;
                        if (parentMenuItem != null)
                        {
                            Destroy(parentMenu.gameObject);
                        }
                        else
                        {
                            parentMenu.Close();
                        }
                    }
                    
                    if(m_parent == null)
                    {
                        Close();
                    }
                    else
                    {
                        if (!m_parent.IsPointerOver)
                        {
                            Destroy(gameObject);
                        }
                    }
                }
            }
        }
    }
}
