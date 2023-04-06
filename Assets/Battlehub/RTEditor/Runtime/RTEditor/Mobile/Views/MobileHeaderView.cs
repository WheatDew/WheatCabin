using Battlehub.RTCommon;
using Battlehub.UIControls.DockPanels;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Battlehub.RTEditor.Mobile.Views
{
    public class MobileHeaderView : MonoBehaviour
    {
        [SerializeField]
        private bool m_createPopup = false;

        [SerializeField]
        private TextMeshProUGUI m_tooltip = null;

        private IRTE m_rte;
        private IWindowManager m_wm;
        private RuntimeWindow m_parentWindow;
        private Transform m_creatorWindow;

        public UnityEvent IsCreatingChanged;

        private bool m_isCreating;
        public bool IsCreating
        {
            get { return m_isCreating; }
            set
            {
                if (m_isCreating != value)
                {
                    m_isCreating = value;
                    if (m_isCreating)
                    {
                        m_isCreating = m_rte.CursorHelper.SetCursor(this, Utils.KnownCursor.DropAllowed);

                        if (m_creatorWindow != null)
                        {
                            m_wm.DestroyWindow(m_creatorWindow);
                            m_creatorWindow = null;
                        }
                    }
                    else
                    {
                        m_rte.CursorHelper.ResetCursor(this);
                    }

                    if (m_tooltip != null)
                    {
                        m_tooltip.gameObject.SetActive(m_isCreating);
                    }

                    IsCreatingChanged?.Invoke();
                }
            }
        }

        private void Awake()
        {
            m_rte = IOC.Resolve<IRTE>();
            m_wm = IOC.Resolve<IWindowManager>();
        }

        private void Start()
        {
            m_parentWindow = m_rte.GetWindow(RuntimeWindowType.Scene);
        }

        private void OnDestroy()
        {
            m_rte.ActiveWindowChanged -= OnActiveWindowChanged;
            m_rte = null;

            m_wm = null;

            m_parentWindow = null;
        }

        protected virtual void Update()
        {
            if (!m_parentWindow.IsPointerOver)
            {
                return;
            }

            if ((m_rte.Input.GetKey(KeyCode.LeftControl) && m_rte.Input.GetKey(KeyCode.LeftShift) || IsCreating) &&
               (m_rte.TouchInput.IsTouchSupported ? m_rte.Input.GetPointerDown(0) : m_rte.Input.GetPointerUp(0)))
            {
                ShowCreator();
            }

            if (m_rte.Input.GetKeyDown(KeyCode.Escape))
            {
                IsCreating = false;
            }
        }

        private void OnActiveWindowChanged(RuntimeWindow window)
        {
            if (m_creatorWindow != null && !m_wm.IsActive(m_creatorWindow))
            {
                m_rte.ActiveWindowChanged -= OnActiveWindowChanged;
                m_wm.DestroyWindow(m_creatorWindow);
                m_creatorWindow = null;
            }
        }

        public void ShowCreator()
        {
            if (m_parentWindow == null)
            {
                return;
            }

            if (m_parentWindow != m_rte.ActiveWindow || !m_parentWindow.IsPointerOver)
            {
                IsCreating = false;
                return;
            }

            if (m_createPopup)
            {
                CreatePopup();
            }
            else
            {
                Region region;
                Transform inspector = m_wm.GetWindow(BuiltInWindowNames.Inspector);
                region = inspector != null ? inspector.GetComponentsInParent<Region>(true).FirstOrDefault() : null;
                if (region != null && region.gameObject.activeInHierarchy)
                {
                    m_creatorWindow = m_wm.CreateWindow(MobileWindowNames.Creator, false, RegionSplitType.None, 0, inspector);
                    m_rte.ActiveWindowChanged -= OnActiveWindowChanged;
                    m_rte.ActiveWindowChanged += OnActiveWindowChanged;
                }
                else
                {
                    CreatePopup();
                }
            }

            IsCreating = false;
        }

        private void CreatePopup()
        {
            IRTEAppearance appearance = IOC.Resolve<IRTEAppearance>();
            Camera camera = appearance.UIForegroundScaler.GetComponent<Canvas>().worldCamera;

            Vector3 offset = new Vector3(20, -20, 0);
            Vector3 screenPoint = m_rte.Input.GetPointerXY(0) + offset;
            if(RectTransformUtility.ScreenPointToLocalPointInRectangle(m_wm.PopupRoot, screenPoint, camera, out Vector2 position))
            {
                m_wm.CreatePopup(MobileWindowNames.Creator, position, true);
            }
        }

    }
}
