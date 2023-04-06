using Battlehub.RTCommon;
using Battlehub.UIControls.DockPanels;
using System.ComponentModel;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityWeld.Binding;

namespace Battlehub.RTEditor.Mobile.Controls
{
    [Binding]
    public class MobileCreatorTrigger : MonoBehaviour, INotifyPropertyChanged
    {
        [SerializeField]
        private bool m_createPopup = false;

        [SerializeField]
        private TextMeshProUGUI m_tooltip = null;

        private IRTE m_rte;
        private IWindowManager m_wm;
        private RuntimeWindow m_parentWindow;
        private Transform m_creatorWindow;
        
        private bool m_creating;
        [Binding]
        public bool Creating
        {
            get { return m_creating; }
            set
            {
                if (m_creating != value)
                {
                    m_creating = value;
                    if(m_creating)
                    {
                        m_creating = m_rte.CursorHelper.SetCursor(this, Utils.KnownCursor.DropAllowed);

                        if(m_creatorWindow != null)
                        {
                            m_wm.DestroyWindow(m_creatorWindow);
                            m_creatorWindow = null;
                        }
                    }
                    else
                    {
                        m_rte.CursorHelper.ResetCursor(this);
                    }

                    if(m_tooltip != null)
                    {
                        m_tooltip.gameObject.SetActive(m_creating);
                    }
                    
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Creating)));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Awake()
        {
            m_rte = IOC.Resolve<IRTE>();
            m_wm = IOC.Resolve<IWindowManager>();
        }

        private void Start()
        {
            m_parentWindow = GetComponentInParent<RuntimeWindow>();
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
            if (m_parentWindow != m_rte.ActiveWindow)
            {
                return;
            }

            if((m_rte.Input.GetKey(KeyCode.LeftControl) && m_rte.Input.GetKey(KeyCode.LeftShift) || Creating) &&
               (m_rte.TouchInput.IsTouchSupported ? m_rte.Input.GetPointerDown(0) : m_rte.Input.GetPointerUp(0)))
            {
                ShowCreator();   
            }

            if(m_rte.Input.GetKeyDown(KeyCode.Escape))
            {
                Creating = false;
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
            if(m_parentWindow == null)
            {
                return;
            }

            if (m_parentWindow != m_rte.ActiveWindow || !m_parentWindow.IsPointerOver)
            {
                Creating = false;
                return;
            }

            if(m_createPopup)
            {
                CreatePopup();
            }
            else
            {
                Region region; 
                Transform inspector = m_wm.GetWindow(BuiltInWindowNames.Inspector);
                region = inspector.GetComponentsInParent<Region>(true).FirstOrDefault();
                if(region != null && region.gameObject.activeInHierarchy)
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
  
            Creating = false;
        }

        private void CreatePopup()
        {
            Vector2 offset = new Vector2(-10, 10);
            Vector2 position = m_parentWindow.Pointer.ScreenPoint + offset;

            ISettingsComponent settings = IOC.Resolve<ISettingsComponent>();
            if (settings != null)
            {
                position /= settings.UIScale;
            }

            m_wm.CreatePopup(MobileWindowNames.Creator, position, true);
        }
    }
}
