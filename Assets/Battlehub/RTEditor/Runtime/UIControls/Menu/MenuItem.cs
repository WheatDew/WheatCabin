using Battlehub.UIControls.Common;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Battlehub.UIControls.MenuControl
{
    public delegate void MenuItemEventHandler(MenuItem menuItem);

    public class MenuItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField]
        private Color m_selectionColor = new Color32(0x00, 0x97, 0xFF, 0xFF);
        public Color SelectionColor
        {
            get { return m_selectionColor; }
            set { m_selectionColor = value; }
        }

        [SerializeField]
        private Color m_textColor = new Color32(0xFF, 0xFF, 0xFF, 0xFF);
        public Color TextColor
        {
            get { return m_textColor; }
            set { m_textColor = value; }
        }

        [SerializeField]
        private Color m_disabledSelectionColor = new Color32(0xC5, 0xBF, 0xBF, 0x7F);
        public Color DisabledSelectionColor
        {
            get { return m_disabledSelectionColor; }
            set { m_disabledSelectionColor = value; }
        }

        [SerializeField]
        private Color m_disableTextColor = new Color32(0x32, 0x32, 0x32, 0xFF);
        public Color DisableTextColor
        {
            get { return m_disableTextColor; }
            set { m_disableTextColor = value; }
        }

        [SerializeField]
        private Menu m_menuPrefab = null;

        [SerializeField]
        private Image m_icon = null;

        [SerializeField]
        private TextMeshProUGUI m_text = null;

        [SerializeField]
        private GameObject m_expander = null;

        [SerializeField]
        private Image m_selection = null;

        [SerializeField]
        private Image m_onIcon = null;

        [SerializeField,HideInInspector]
        private Transform m_root;
        public Transform Root
        {
            get { return m_root; }
            set { m_root = value; }
        }

        private int m_depth;
        public int Depth
        {
            get { return m_depth; }
            set { m_depth = value; }
        }


        private MenuItemInfo m_item;
        public MenuItemInfo Item
        {
            get { return m_item; }
            set
            {
                if(m_item != value)
                {
                    m_item = value;
                    DataBind();
                }
            }
        }

        private MenuItemInfo[] m_children;
        public MenuItemInfo[] Children
        {
            get { return m_children; }
            set { m_children = value; }
        }

        public bool HasChildren
        {
            get { return m_children != null && m_children.Length > 0; }
        }

        private bool m_isPointerOver;
        public bool IsPointerOver
        {
            get { return m_isPointerOver; }
        }

        private Menu m_submenu;
        public Menu Submenu
        {
            get { return m_submenu; }
        }

        private BaseRaycaster m_raycaster;

        private void Awake()
        {
            m_raycaster = GetComponentInParent<BaseRaycaster>();
            if(m_raycaster == null)
            {
                Canvas canvas = GetComponentInParent<Canvas>();
                if(canvas != null)
                {
                    m_raycaster = canvas.gameObject.AddComponent<BaseRaycaster>();
                }
            }
        }

        private void OnDestroy()
        {
            if(m_submenu != null)
            {
                Destroy(m_submenu.gameObject);
            }
        }

        private void DataBind()
        {
            if(m_item != null)
            {
                if(m_icon != null)
                {
                    m_icon.sprite = m_item.Icon;
                    m_icon.gameObject.SetActive(m_item.Icon != null);
                }
                
                if(m_text != null)
                {
                    m_text.text = m_item.Text;
                }
                
                if(m_expander != null)
                {
                    m_expander.SetActive(HasChildren);
                }
                
                if(m_onIcon != null)
                {
                    m_onIcon.gameObject.SetActive(m_item.IsOn);
                }
            }
            else
            {
                if (m_icon != null)
                {
                    m_icon.sprite = null;
                    m_icon.gameObject.SetActive(false);
                }

                if(m_text != null)
                {
                    m_text.text = string.Empty;
                }
                
                if(m_expander != null)
                {
                    m_expander.SetActive(false);
                }
                
                if (m_onIcon != null)
                {
                    m_onIcon.gameObject.SetActive(false);
                }
            }

            var validationResult = IsValid(false);
            if(validationResult.IsVisible)
            {
                if (validationResult.IsValid)
                {
                    if(m_text != null)
                    {
                        m_text.color = m_textColor;
                    }
                   
                    if(m_selection != null)
                    {
                        m_selection.color = m_selectionColor;
                    }
                }
                else
                {
                    if(m_text != null)
                    {
                        m_text.color = m_disableTextColor;
                    }
                    
                    if(m_selection != null)
                    {
                        m_selection.color = m_disabledSelectionColor;
                    }
                }
            }
            gameObject.SetActive(validationResult.IsVisible);
        }

        private MenuItemValidationArgs IsValid(bool checkChildren)
        {
            if(m_item == null)
            {
                return new MenuItemValidationArgs(m_item.Command, checkChildren && HasChildren) { IsValid = false, IsVisible = false };
            }

            if(m_item.Validate == null)
            {
                return new MenuItemValidationArgs(m_item.Command, checkChildren && HasChildren) { IsVisible = true };
            }

            MenuItemValidationArgs args = new MenuItemValidationArgs(m_item.Command, checkChildren && HasChildren);
            m_item.Validate.Invoke(args);
            return args;
        }

        //void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        //{
        //    Debug.Log("OnPointerUp" + eventData.pointerId);
        //    //On Tap
        //    if (eventData.pointerId == 0)
        //    {
        //        Action();
        //    }
        //}


        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            //Required to handle OnPointerClick in some cases ??
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {

        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
#if INPUTSYSTEM_1_0_1_OR_NEWER
            ExtendedPointerEventData extendedData = eventData as ExtendedPointerEventData;
            if (extendedData != null)
            {
                if(extendedData.button == PointerEventData.InputButton.Left)
                {
                    Action();
                }
            }
            else
#endif
            {
                //On Left Mouse Button Click
                if (eventData.pointerId == -1)
                {
                    Action();
                }
            }
        }

        private void Action()
        {
            var validationResult = IsValid(true);
            if (validationResult.IsValid)
            {
                if (this != null)
                {
                    Menu menu = GetComponentInParent<Menu>();
                    if (menu != null)
                    {
                        while (menu.Parent != null)
                        {
                            menu = menu.Parent.GetComponentInParent<Menu>();
                        }
                        menu.Close();
                    }
                }

                if (m_item.Action != null)
                {
                    m_item.Action.Invoke(m_item.Command);
                }
            }
            else
            {
                if (HasChildren)
                {
                    Select(false);
                }
            }
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            if (!eventData.IsDefaultPointerId())
            {
                return;
            }

            Select(false);
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            if (!eventData.IsDefaultPointerId())
            {
                return;
            }
            m_isPointerOver = false;
            if (m_submenu == null)
            {
                //On Tap
                if (eventData.pointerId == 0 && Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended) 
                {
                    Action();
                }
                else
                {
                    if (m_coUnselect != null)
                    {
                        StopCoroutine(m_coUnselect);
                        m_coUnselect = null;
                    }

                    if (m_coSelect != null)
                    {
                        StopCoroutine(m_coSelect);
                        m_coSelect = null;
                    }

                    Menu menu = GetComponentInParent<Menu>();
                    menu.Child = null;

                    if(m_selection != null)
                    {
                        m_selection.gameObject.SetActive(false);
                    }
                }  
            }
            else
            {

                bool isPointerOverSubmenu = IsPointerOverSubmenu(eventData);
                if (!isPointerOverSubmenu)
                {
                    Unselect();
                }
            }
        }

        private IEnumerator m_coSelect;
        private IEnumerator m_coUnselect;
        private IEnumerator CoSelect()
        {
            yield return new WaitForSeconds(0.2f);

            if (HasChildren)
            {
                if (m_submenu == null)
                {
                    m_submenu = Instantiate(m_menuPrefab, m_root, false);
                    m_submenu.Parent = this;
                    m_submenu.name = "Submenu";
                    m_submenu.Depth = m_depth;
                    m_submenu.Items = Children;
                    m_submenu.transform.position = FindPosition();
                    m_submenu.gameObject.SetActive(true);
                }
            }

            m_coSelect = null;
        }

        private IEnumerator CoUnselect()
        {
            yield return new WaitForSeconds(0.2f);

            if (m_submenu != null && m_submenu.Child == null)
            {
                Destroy(m_submenu.gameObject);
                m_submenu = null;
            }

            m_coUnselect = null;
        }

        public void Select(bool showSelectionOnly)
        {
            if(showSelectionOnly)
            {
                if(m_selection != null)
                {
                    m_selection.gameObject.SetActive(true);
                }
                return;
            }

            if (m_coUnselect != null)
            {
                StopCoroutine(m_coUnselect);
                m_coUnselect = null;
            }

            if(m_coSelect != null)
            {
                StopCoroutine(m_coSelect);
                m_coSelect = null;
            }

            m_isPointerOver = true;
            
            if(m_selection != null)
            {
                m_selection.gameObject.SetActive(true);
            }
            
            Menu menu = GetComponentInParent<Menu>();

            if(menu.Child != this)
            {
                if (menu.Child != null && menu.Child.Submenu != null)
                {
                    menu.Child.m_submenu.Close();
                }

                menu.Child = this;
            }
            

            if(menu.Parent != null)
            {
                Menu parentMenu = menu.Parent.GetComponentInParent<Menu>();
                if(parentMenu == null)
                {
                    Debug.LogWarning("parentMenu is null");
                }
                else
                {
                    parentMenu.Child = menu.Parent;
                }
            }

            m_coSelect = CoSelect();
            StartCoroutine(m_coSelect);
        }

        public void Unselect()
        {
            if (m_coUnselect != null)
            {
                StopCoroutine(m_coUnselect);
                m_coUnselect = null;
            }

            if (m_coSelect != null)
            {
                StopCoroutine(m_coSelect);
                m_coSelect = null;
            }

            if(m_selection != null)
            {
                m_selection.gameObject.SetActive(false);
            }
            
            Menu menu = GetComponentInParent<Menu>();
            menu.Child = null;

            m_coUnselect = CoUnselect();
            StartCoroutine(m_coUnselect);
        }


        private Vector3 FindPosition()
        {
            const float overlap = 5;

            RectTransform rootRT = (RectTransform)m_root;
            RectTransform rt = (RectTransform)transform;

            Vector2 size = new Vector2(rt.rect.width, rt.rect.height * m_submenu.ActualItemsCount);

            Vector3 position = -Vector2.Scale(rt.rect.size, rt.pivot);
            position.y = -position.y;
            position = rt.TransformPoint(position);
            position = rootRT.InverseTransformPoint(position);

            Vector2 topLeft = -Vector2.Scale(rootRT.rect.size, rootRT.pivot);
            
            if (position.x + size.x + size.x - overlap > topLeft.x + rootRT.rect.width)
            {
                position.x = position.x - size.x + overlap;
            }
            else
            {
                position.x = position.x + size.x - overlap;
            }            
            
            if (position.y - size.y < topLeft.y)
            {
                position.y -= (position.y - size.y) - topLeft.y;
            }

            return rootRT.TransformPoint(position);
        }

        private bool IsPointerOverSubmenu(PointerEventData eventData)
        {
            List<RaycastResult> raycastResultList = new List<RaycastResult>();
            m_raycaster.Raycast(eventData, raycastResultList);
            for(int i = 0; i < raycastResultList.Count; ++i)
            {
                RaycastResult raycastResult = raycastResultList[i];
                if(raycastResult.gameObject == m_submenu.gameObject)
                {
                    return true;
                }
            }

            return false;
        }
    }
}

