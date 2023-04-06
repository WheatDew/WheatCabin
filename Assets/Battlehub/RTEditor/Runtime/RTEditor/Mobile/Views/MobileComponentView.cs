using Battlehub.RTCommon;
using Battlehub.RTEditor.Mobile.ViewModels;
using Battlehub.UIControls.DockPanels;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityWeld.Binding;

namespace Battlehub.RTEditor.Mobile.Views
{
    public class MobileComponentView : MonoBehaviour
    {
        [SerializeField]
        private Image m_iconImage = null;

        [SerializeField]
        private BoolEditor m_enabledEditor = null;

        [SerializeField]
        private TextMeshProUGUI m_headerText = null;

        [SerializeField]
        private Button m_resetButton = null;

        [SerializeField]
        private Button m_removeButton = null;

        [SerializeField]
        private Button m_pinButton = null;

        [SerializeField]
        private Transform m_headerPanel = null;

        [SerializeField]
        private Transform m_editorPanel = null;

        private IList<Component> m_components;

        public IList<Component> Components
        {
            get { return m_components; }
            set
            {
                if (m_components != value)
                {
                    m_components = value;

                    ComponentEditor componentEditor = GameObjectEditorUtils.CreateComponentEditor(m_editorPanel, m_components);
                    componentEditor.HeaderPanel.gameObject.SetActive(false);
                    componentEditor.IconImage = m_iconImage;
                    componentEditor.ExpanderToggle = null;
                    componentEditor.EnabledEditor = m_enabledEditor;
                    componentEditor.Header = m_headerText;
                    componentEditor.ResetButton = m_resetButton;
                    if (componentEditor.RemoveButton != null)
                    {
                        componentEditor.RemoveButton = m_removeButton;
                    }
                    else
                    {
                        m_removeButton.gameObject.SetActive(false);
                    }

                    m_headerPanel.gameObject.SetActive(true);
                }
            }
        }

        private bool m_isPopup;
        private IRTE m_editor;

        private void Awake()
        {
            Template targetTemplate = GetComponent<Template>();
            targetTemplate.InitChildBindings(MobileComponentViewModel.Empty);

            m_editor = IOC.Resolve<IRTE>();
            m_pinButton.onClick.AddListener(OnPin);
            m_editor.Object.ComponentDestroyed += OnComponentDestroyed;
        }

        private void Start()
        {
            if (m_components == null || m_components.Count == 0)
            {
                IWindowManager wm = IOC.Resolve<IWindowManager>();
                var window = GetComponentInParent<RuntimeWindow>().transform;
                wm.DestroyWindow(window);
            }

            m_isPopup = GetComponentInParent<RegionPopupBehaviour>();
            m_pinButton.gameObject.SetActive(m_isPopup);
        }

        private void OnDestroy()
        {
            m_pinButton.onClick.RemoveListener(OnPin);
            if(m_editor != null && m_editor.Object != null)
            {
                m_editor.Object.ComponentDestroyed -= OnComponentDestroyed;
            }
            m_editor = null;
        }

        private string GetHeaderText()
        {
   
            GameObject[] gameObjects = m_editor.Selection.gameObjects;
            int count = gameObjects != null ? gameObjects.Where(go => go != null).Count() : 0;
            if (count == 0)
            {
                return string.Empty;
            }

            if (count > 1)
            {
                return $"{count} objects";
            }

            return gameObjects.Where(go => go != null).First().name;
        }

        private void OnPin()
        {
            IWindowManager wm = IOC.Resolve<IWindowManager>();
            
            var window = GetComponentInParent<RuntimeWindow>().transform;
            string windowTypeName = wm.GetWindowTypeName(window);

            Transform pinnedWindow = wm.CreateWindow(windowTypeName, true);
            Template sourceTemplate = window.GetComponentInChildren<Template>(true);
            Template targetTemplate = pinnedWindow.GetComponentInChildren<Template>(true);
            if (sourceTemplate != null && targetTemplate != null)
            {
                targetTemplate.InitChildBindings(sourceTemplate.GetViewModel());
                targetTemplate.gameObject.SetActive(true);
            }

            wm.SetHeaderIcon(pinnedWindow, wm.GetHeaderIcon(window));
            wm.SetHeaderText(pinnedWindow, GetHeaderText());

            wm.CopyTransform(pinnedWindow, window.transform);

            wm.DestroyWindow(window.transform);
            wm.ActivateWindow(pinnedWindow);
        }

        private void OnComponentDestroyed(ExposeToEditor obj, Component component)
        {
            if (m_components.Contains(component))
            {
                var window = GetComponentInParent<RuntimeWindow>().transform;
                IWindowManager wm = IOC.Resolve<IWindowManager>();
                wm.DestroyWindow(window);
            }
        }

    }
}

