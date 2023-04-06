using Battlehub.UIControls.Common;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Battlehub.UIControls.MenuControl
{
    public class MainMenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
    {
        [SerializeField]
        private Menu m_menu = null;
        public Menu Menu
        {
            get { return m_menu; }
            set
            {
                m_menu = value;
            }
        }

        [SerializeField]
        private Image m_selection = null;

        [SerializeField]
        private Color m_pointerOverColor = new Color32(0x00, 0x97, 0xFF, 0x7F);
        public Color PointerOverColor
        {
            get { return m_pointerOverColor; }
            set
            {
                m_pointerOverColor = value;
                if (m_selection != null)
                {
                    if (m_isPointerOver)
                    {
                        m_selection.color = m_pointerOverColor;
                    }
                }
            }
        }

        [SerializeField]
        private Color m_focusedColor = new Color32(0x00, 0x97, 0xFF, 0xFF);
        public Color FocusedColor
        {
            get { return m_focusedColor; }
            set { m_focusedColor = value; }
        }

        [SerializeField]
        private Color m_normalColor = new Color32(0xFF, 0xFF, 0xFF, 0x00);
        public Color NormalColor
        {
            get { return m_normalColor; }
            set
            {
                m_normalColor = value;
                if(m_selection != null)
                {
                    if(!m_isPointerOver)
                    {
                        m_selection.color = m_normalColor;
                    }
                }
            }
        }

        [SerializeField]
        private TextMeshProUGUI m_text = null;

        public string Text
        {
            get { return m_text != null ? m_text.text : null; }
            set
            {
                if(m_text != null)
                {
                    m_text.text = value;
                }
            }
        }

        private bool m_isPointerOver;
        private CanvasGroup m_canvasGroup;

        private void Awake()
        {
            if(m_selection == null)
            {
                m_selection = GetComponent<Image>();
            }

            if (m_selection != null)
            {
                m_selection.color = m_normalColor;
            }

            if (m_menu != null)
            {
                m_menu.Anchor = transform as RectTransform;
                m_menu.Opened += OnOpened;
                m_menu.Closed += OnClosed;

                m_menu.gameObject.SetActive(false);
            }
        }

        private void Start()
        {
            m_canvasGroup = GetComponentInParent<CanvasGroup>();
            if (m_menu != null && (m_menu.Items == null || m_menu.Items.Length == 0))
            {
                gameObject.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            if(m_menu != null)
            {
                m_menu.Opened -= OnOpened;
                m_menu.Closed -= OnClosed;
            }
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            if (!eventData.IsDefaultPointerId())
            {
                return;
            }

            if (m_canvasGroup && !m_canvasGroup.interactable)
            {
                return;
            }

            if (m_menu == null)
            {
                return;
            }

            if(m_menu.IsOpened)
            {
                m_menu.Close();
            }
            else
            {
                m_menu.Open();
            }
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            if (!eventData.IsDefaultPointerId())
            {
                return;
            }

            if (m_canvasGroup && !m_canvasGroup.interactable)
            {
                return;
            }

            m_isPointerOver = true;
            if(m_menu != null && m_menu.gameObject.activeSelf)
            {
                m_selection.color = m_focusedColor;
            }
            else
            {
                m_selection.color = m_pointerOverColor;
            }
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            if (!eventData.IsDefaultPointerId())
            {
                return;
            }

            if (m_canvasGroup && !m_canvasGroup.interactable)
            {
                return;
            }
          
            m_isPointerOver = false;
            if (m_menu != null && m_menu.gameObject.activeSelf)
            {
                m_selection.color = m_focusedColor;
            }
            else
            {
                m_selection.color = m_normalColor;
            }

            if(Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
            {
                m_menu.Close();
            }
        }

        private void OnClosed(object sender, System.EventArgs e)
        {
            if(m_isPointerOver)
            {
                m_selection.color = m_pointerOverColor;
            }
            else
            {
                m_selection.color = m_normalColor;
            }
        }

        private void OnOpened(object sender, System.EventArgs e)
        {
            m_selection.color = m_focusedColor;
        }

     
    }

}

