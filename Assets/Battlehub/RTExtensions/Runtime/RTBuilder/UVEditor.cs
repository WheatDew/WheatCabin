using Battlehub.RTCommon;
using Battlehub.RTEditor;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTBuilder
{
    public class UVEditor : MonoBehaviour
    {
        private IProBuilderTool m_tool;
        private IWindowManager m_wm;

        [SerializeField]
        private GameObject m_uvModePanel = null;

        [SerializeField]
        private Button m_convertToAutoUVsButton = null;

        [SerializeField]
        private Button m_convertToManualUVsButton = null;

        [SerializeField]
        private GameObject m_uvAutoEditorPanel = null;

        [SerializeField]
        private GameObject m_uvManualEditorPanel = null;

        [SerializeField]
        private GameObject m_uvNoSelectedFacesPanel = null;

        [SerializeField]
        private TextMeshProUGUI m_modeText = null;

        private Transform m_proBuilderWindow;
        private IRTE m_editor;

        private void Awake()
        {
            m_tool = IOC.Resolve<IProBuilderTool>();
            m_editor = IOC.Resolve<IRTE>();
        }

        private void Start()
        {
            if(m_tool != null)
            {
                OnToolSelectionChanged();
                m_tool.SelectionChanged += OnToolSelectionChanged;
            }

            m_editor.Selection.SelectionChanged += OnSelectionChanged;

            if (m_convertToAutoUVsButton != null)
            {
                m_convertToAutoUVsButton.onClick.AddListener(OnConvertToAutoUVsClick);
            }

            if(m_convertToManualUVsButton != null)
            {
                m_convertToManualUVsButton.onClick.AddListener(OnConvertToManualUVsClick);
            }
        }

        private void OnEnable()
        {
            m_tool.UVEditingMode = true;
            UpdateVisualState();
        }

        private bool m_quit = false;
        private void OnApplicationQuit()
        {
            m_quit = true;
        }

        private void OnDisable()
        {
            if(!m_quit)
            {
                m_tool.UVEditingMode = false;
            }
        }

        private void OnDestroy()
        {
            if(m_tool != null)
            {
                m_tool.SelectionChanged -= OnToolSelectionChanged;
            }

            if(m_editor != null)
            {
                m_editor.Selection.SelectionChanged -= OnSelectionChanged;
            }

            if (m_convertToAutoUVsButton != null)
            {
                m_convertToAutoUVsButton.onClick.RemoveListener(OnConvertToAutoUVsClick);
            }

            if (m_convertToManualUVsButton != null)
            {
                m_convertToManualUVsButton.onClick.RemoveListener(OnConvertToManualUVsClick);
            }
        }

        private void OnConvertToAutoUVsClick()
        {
            if (m_uvManualEditorPanel != null)
            {
                m_uvManualEditorPanel.gameObject.SetActive(false);
            }

            if (m_uvAutoEditorPanel != null)
            {
                m_uvAutoEditorPanel.gameObject.SetActive(true);
            }

            m_tool.ConvertUVs(true);
            UpdateVisualState();
        }

        private void OnConvertToManualUVsClick()
        {
            if (m_uvManualEditorPanel != null)
            {
                m_uvManualEditorPanel.gameObject.SetActive(true);
            }

            if (m_uvAutoEditorPanel != null)
            {
                m_uvAutoEditorPanel.gameObject.SetActive(false);
            }

            m_tool.ConvertUVs(false);
            UpdateVisualState();
            
        }

        private void OnToolSelectionChanged()
        {
            UpdateVisualState();
        }

        private void OnSelectionChanged(Object[] unselectedObjects)
        {
            UpdateVisualState();
        }

        private void UpdateVisualState()
        {
            if(m_tool == null)
            {
                return;
            }

            if (!m_tool.HasSelection)
            {
                if (m_uvAutoEditorPanel != null)
                {
                    m_uvAutoEditorPanel.gameObject.SetActive(false);
                }
                if (m_uvManualEditorPanel != null)
                {
                    m_uvManualEditorPanel.gameObject.SetActive(false);
                }
                if (m_uvModePanel != null)
                {
                    m_uvModePanel.gameObject.SetActive(false);
                }
                if (m_uvNoSelectedFacesPanel != null)
                {
                    m_uvNoSelectedFacesPanel.gameObject.SetActive(true);
                }
            }
            else
            {
                if (m_uvNoSelectedFacesPanel != null)
                {
                    m_uvNoSelectedFacesPanel.gameObject.SetActive(false);
                }

                if (m_uvModePanel != null)
                {
                    m_uvModePanel.gameObject.SetActive(true);
                }

                m_tool.TryUpdatePivotTransform();

             
                bool hasSelectedManualUVs = m_tool.HasSelectedManualUVs;
                bool hasSelectedAutoUVs = m_tool.HasSelectedAutoUVs;
                if(hasSelectedAutoUVs || hasSelectedManualUVs)
                {
                    if (m_uvAutoEditorPanel != null)
                    {
                        m_uvAutoEditorPanel.gameObject.SetActive(hasSelectedAutoUVs && !hasSelectedManualUVs);
                    }
                    if (m_uvManualEditorPanel != null)
                    {
                        m_uvManualEditorPanel.gameObject.SetActive(hasSelectedManualUVs && !hasSelectedAutoUVs);
                    }

                    if (m_modeText != null)
                    {
                        ILocalization lc = IOC.Resolve<ILocalization>();

                        if (hasSelectedAutoUVs && hasSelectedManualUVs)
                        {
                            m_modeText.text = lc.GetString("ID_RTBuilder_UVEditorAuto_UVModeMixed", "UV Mode: Mixed");
                        }
                        else if (hasSelectedAutoUVs)
                        {
                            m_modeText.text = lc.GetString("ID_RTBuilder_UVEditorAuto_UVModeAuto", "UV Mode: Auto");
                        }
                        else if (hasSelectedManualUVs)
                        {
                            m_modeText.text = lc.GetString("ID_RTBuilder_UVEditorAuto_UVModeManual", "UV Mode: Manual");
                        }
                    }

                    if (m_convertToAutoUVsButton != null)
                    {
                        m_convertToAutoUVsButton.gameObject.SetActive(!hasSelectedAutoUVs || hasSelectedManualUVs);
                    }

                    if (m_convertToManualUVsButton != null)
                    {
                        m_convertToManualUVsButton.gameObject.SetActive(!hasSelectedManualUVs || hasSelectedAutoUVs);
                    }
                }
                else
                {
                    if (m_uvAutoEditorPanel != null)
                    {
                        m_uvAutoEditorPanel.gameObject.SetActive(false);
                    }
                    if (m_uvManualEditorPanel != null)
                    {
                        m_uvManualEditorPanel.gameObject.SetActive(true);
                    }

                    if(m_modeText != null)
                    {
                        ILocalization lc = IOC.Resolve<ILocalization>();
                        m_modeText.text = lc.GetString("ID_RTBuilder_UVEditorAuto_UVModeManual", "UV Mode: Manual");
                    }

                    if (m_convertToAutoUVsButton != null)
                    {
                        m_convertToAutoUVsButton.gameObject.SetActive(false);
                    }

                    if (m_convertToManualUVsButton != null)
                    {
                        m_convertToManualUVsButton.gameObject.SetActive(false);
                    }
                }
            }
        }
    }
}

