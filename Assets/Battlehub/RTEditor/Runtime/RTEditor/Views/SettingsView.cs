using Battlehub.RTCommon;
using Battlehub.UIControls;
using Battlehub.UIControls.Dialogs;
using Battlehub.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Battlehub.RTEditor.Views
{
    public class SettingsView : View
    {
        [SerializeField]
        private HeaderLabel m_sceneSettingsHeader = null;

        [SerializeField]
        private BoolEditor m_isGridVisibleEditor = null;

        [SerializeField]
        private RangeEditor m_gridOpacityEditor = null;

        [SerializeField]
        private BoolEditor m_gridZTest = null;

        [SerializeField]
        private BoolEditor m_snapToGridEditor = null;

        [SerializeField]
        private RangeEditor m_gridSizeEditor = null;

        [SerializeField]
        private HeaderLabel m_uiSettingsHeader = null;

        [SerializeField]
        private BoolEditor m_uiAutoScaleEditor = null;

        [SerializeField]
        private RangeEditor m_uiScaleEditor = null;

        [SerializeField]
        private HeaderLabel m_sceneNavigationSettingsHeader = null;

        [SerializeField]
        private RangeEditor m_freeRotationSmoothSpeedEditor = null;

        [SerializeField]
        private BoolEditor m_rotationInvertXEditor = null;

        [SerializeField]
        private BoolEditor m_rotationInvertYEditor = null;

        [SerializeField]
        private RangeEditor m_freeMovementSmoothSpeedEditor = null;

        [SerializeField]
        private RangeEditor m_zoomSpeedEditor = null;

        [SerializeField]
        private BoolEditor m_constantZoomSpeedEditor = null;

        [SerializeField]
        private HeaderLabel m_measurementHeader = null;

        [SerializeField]
        private EnumEditor m_measurementSystemEditor = null;

        [SerializeField]
        private HeaderLabel m_graphicsHeader = null;

        [SerializeField]
        private EnumEditor m_graphicsQualityEditor = null;

        [SerializeField]
        private HeaderLabel m_lightSettingsHeader = null;

        [SerializeField]
        private ColorEditor m_lightColorEditor = null;

        [SerializeField]
        private FloatEditor m_lightIntensityEditor = null;

        [SerializeField]
        private EnumEditor m_shadowTypeEditor = null;

        [SerializeField]
        private RangeEditor m_shadowStrengthEditor = null;

        [SerializeField]
        private HeaderLabel m_themesHeader = null;

        [SerializeField]
        private OptionsEditor m_selectedThemeEditor = null;

        [SerializeField]
        private Transform m_panel = null;

        private List<GameObject> m_customSettings = new List<GameObject>();

        [SerializeField]
        private Button m_resetButton = null;

        public bool UIAutoScale
        {
            get { return m_settings.UIAutoScale; }
            set
            {
                m_settings.UIAutoScale = value;
                if (m_uiScaleEditor != null)
                {
                    m_uiScaleEditor.gameObject.SetActive(!UIAutoScale);
                }
                if (value)
                {
                    m_settings.UIScale = 1.0f;
                }
            }
        }

        private ILocalization m_localization;
        private ISettingsComponent m_settings;
        private Dialog m_parentDialog;

        protected override void Awake()
        {
            base.Awake();
            m_settings = IOC.Resolve<ISettingsComponent>();
            m_localization = IOC.Resolve<ILocalization>();
        }

        protected override void Start()
        {
            base.Start();
            m_parentDialog = GetComponentInParent<Dialog>();
            if (m_parentDialog != null)
            {
                m_parentDialog.IsOkVisible = false;
                m_parentDialog.IsCancelVisible = true;
                m_parentDialog.CancelText = m_localization.GetString("ID_RTEditor_SettingsDialog_Close", "Close");
            }

            if (m_uiSettingsHeader != null)
            {
                m_uiSettingsHeader.Init(null, null, null, null, m_localization.GetString("ID_RTEditor_SettingsDialog_UISettings", "UI Settings"));
            }

            if (m_sceneSettingsHeader != null)
            {
                m_sceneSettingsHeader.Init(null, null, null, null, m_localization.GetString("ID_RTEditor_SettingsDialog_SceneSettings", "Scene Settings"));
            }

            if (m_sceneNavigationSettingsHeader != null)
            {
                m_sceneNavigationSettingsHeader.Init(null, null, null, null, m_localization.GetString("ID_RTEditor_SettingsDialog_SceneNavigationSettings", "Scene Navigation Settings"));
            }

            if (m_measurementHeader != null)
            {
                m_measurementHeader.Init(null, null, null, null, m_localization.GetString("ID_RTEditor_SettingsDialog_MeasurementSettings", "Measurement Settings"));
            }

            if (m_isGridVisibleEditor != null)
            {
                m_isGridVisibleEditor.Init(m_settings, m_settings, Strong.PropertyInfo((ISettingsComponent x) => x.IsGridVisible), null, m_localization.GetString("ID_RTEditor_SettingsDialog_IsGridVisible", "Is Grid Visible"));
            }

            if (m_gridOpacityEditor != null)
            {
                m_gridOpacityEditor.Init(m_settings, m_settings, Strong.PropertyInfo((ISettingsComponent x) => x.GridOpacity), null, m_localization.GetString("ID_RTEditor_SettingsDialog_GridOpacity", "Grid Opacity"));
            }

            if (m_gridZTest != null)
            {
                m_gridZTest.Init(m_settings, m_settings, Strong.PropertyInfo((ISettingsComponent x) => x.GridZTest), null, m_localization.GetString("ID_RTEditor_SettingsDialog_GridZTest", "Grid Z Test"));
            }

            if (m_snapToGridEditor != null)
            {
                m_snapToGridEditor.Init(m_settings, m_settings, Strong.PropertyInfo((ISettingsComponent x) => x.IsGridEnabled), null, m_localization.GetString("ID_RTEditor_SettingsDialog_SnapToGrid", "Snap To Grid"));
            }

            if (m_gridSizeEditor != null)
            {
                m_gridSizeEditor.Min = 0.1f;
                m_gridSizeEditor.Max = 8;
                m_gridSizeEditor.Init(m_settings, m_settings, Strong.PropertyInfo((ISettingsComponent x) => x.GridSize), null, m_localization.GetString("ID_RTEditor_SettingsDialog_GridSize", "Grid Size"));
            }

            if (m_uiAutoScaleEditor != null)
            {
                m_uiAutoScaleEditor.Init(this, Strong.PropertyInfo((SettingsView x) => x.UIAutoScale), m_localization.GetString("ID_RTEditor_SettingsDialog_UIAutoScale", "UI Auto Scale"), false);
            }

            if (m_uiScaleEditor != null)
            {
                m_uiScaleEditor.Min = 0.5f;
                m_uiScaleEditor.Max = 3;
                m_uiScaleEditor.Init(m_settings, m_settings, Strong.PropertyInfo((ISettingsComponent x) => x.UIScale), null, m_localization.GetString("ID_RTEditor_SettingsDialog_UIScale", "UI Scale"),
                    () => { },
                    () => { },
                    () =>
                    {
                        m_settings.EndEditUIScale();
                        if (m_parentDialog != null)
                        {
                            StartCoroutine(CoEndEditUIScale());
                        }
                    });
                if (UIAutoScale)
                {
                    m_uiScaleEditor.gameObject.SetActive(false);
                }
            }

            if (m_freeMovementSmoothSpeedEditor != null)
            {
                m_freeMovementSmoothSpeedEditor.Min = 1.0f;
                m_freeMovementSmoothSpeedEditor.Max = 100.0f;
                m_freeMovementSmoothSpeedEditor.Init(m_settings, m_settings, Strong.PropertyInfo((ISettingsComponent x) => x.FreeMovementSmoothSpeed), null, m_localization.GetString("ID_RTEditor_SettingsDialog_FreeMovementSmoothSpeed", "Movement Smooth Speed"));
            }

            if (m_freeRotationSmoothSpeedEditor != null)
            {
                m_freeRotationSmoothSpeedEditor.Min = 1.0f;
                m_freeRotationSmoothSpeedEditor.Max = 100.0f;
                m_freeRotationSmoothSpeedEditor.Init(m_settings, m_settings, Strong.PropertyInfo((ISettingsComponent x) => x.FreeRotationSmoothSpeed), null, m_localization.GetString("ID_RTEditor_SettingsDialog_FreeRotationSmoothSpeed", "Rotation Smooth Speed"));
            }

            if (m_rotationInvertXEditor != null)
            {
                m_rotationInvertXEditor.Init(m_settings, m_settings, Strong.PropertyInfo((ISettingsComponent x) => x.RotationInvertX), null, m_localization.GetString("ID_RTEditor_SettingsDialog_RotationInvertX", "Rotation Invert X"));
            }

            if (m_rotationInvertYEditor != null)
            {
                m_rotationInvertYEditor.Init(m_settings, m_settings, Strong.PropertyInfo((ISettingsComponent x) => x.RotationInvertY), null, m_localization.GetString("ID_RTEditor_SettingsDialog_RotationInvertY", "Rotation Invert Y"));
            }

            if (m_zoomSpeedEditor != null)
            {
                m_zoomSpeedEditor.Min = 0.01f;
                m_zoomSpeedEditor.Max = 10.0f;
                m_zoomSpeedEditor.Init(m_settings, m_settings, Strong.PropertyInfo((ISettingsComponent x) => x.ZoomSensitivity), null, m_localization.GetString("ID_RTEditor_SettingsDialog_ZoomSpeed", "Zoom Speed"));
            }

            if (m_constantZoomSpeedEditor != null)
            {
                m_constantZoomSpeedEditor.Init(m_settings, m_settings, Strong.PropertyInfo((ISettingsComponent x) => x.ConstantZoomSpeed), null, m_localization.GetString("ID_RTEditor_SettingsDialog_ConstantZoomSpeed", "Constant Zoom Speed"));
            }

            if (m_measurementSystemEditor != null)
            {
                m_measurementSystemEditor.Init(m_settings, Strong.PropertyInfo((ISettingsComponent x) => x.SystemOfMeasurement), m_localization.GetString("ID_RTEditor_SettingsDialog_SystemOfMeasurement", "System Of Measurement"));
            }

            if (m_graphicsHeader != null)
            {
                m_graphicsHeader.gameObject.SetActive(RenderPipelineInfo.Type != RPType.Standard && GraphicsSettings.renderPipelineAsset != null && RenderPipelineInfo.IsBuiltInRendererPipelineAssetName(GraphicsSettings.renderPipelineAsset.name));
                m_graphicsHeader.Init(null, null, null, null, m_localization.GetString("ID_RTEditor_SettingsDialog_GraphicsSettings", "Graphics Settings"));
            }

            if (m_graphicsQualityEditor != null)
            {
                m_graphicsQualityEditor.gameObject.SetActive(RenderPipelineInfo.Type != RPType.Standard && GraphicsSettings.renderPipelineAsset != null && RenderPipelineInfo.IsBuiltInRendererPipelineAssetName(GraphicsSettings.renderPipelineAsset.name));
                m_graphicsQualityEditor.Init(m_settings, Strong.PropertyInfo((ISettingsComponent x) => x.GraphicsQuality), m_localization.GetString("ID_RTEditor_SettingsDialog_GraphicsQuality", "Quality"));
            }

            if (m_lightSettingsHeader != null)
            {
                m_lightSettingsHeader.Init(null, null, null, null, m_localization.GetString("ID_RTEditor_SettingsDialog_DefaultLightSettings", "Default Light Settings"));
            }

            if (m_lightColorEditor != null)
            {
                m_lightColorEditor.Init(m_settings, Strong.PropertyInfo((ISettingsComponent x) => x.LightColor), m_localization.GetString("ID_RTEditor_SettingsDialog_LightColor", "Light Color"));
            }

            if (m_lightIntensityEditor != null)
            {
                m_lightIntensityEditor.Init(m_settings, Strong.PropertyInfo((ISettingsComponent x) => x.LightIntensity), m_localization.GetString("ID_RTEditor_SettingsDialog_LightIntensity", "Light Intensity"));
            }

            if (m_shadowTypeEditor != null)
            {
                m_shadowTypeEditor.Init(m_settings, Strong.PropertyInfo((ISettingsComponent x) => x.ShadowType), m_localization.GetString("ID_RTEditor_SettingsDialog_ShadowType", "Shadow Type"));
            }

            if (m_shadowStrengthEditor != null)
            {
                m_shadowStrengthEditor.Min = 0;
                m_shadowStrengthEditor.Max = 1;
                m_shadowStrengthEditor.Init(m_settings, Strong.PropertyInfo((ISettingsComponent x) => x.ShadowStrength), m_localization.GetString("ID_RTEditor_SettingsDialog_ShadowStrenth", "Shadow Strength"));
            }

            if (m_themesHeader != null)
            {
                m_themesHeader.Init(null, null, null, null, m_localization.GetString("ID_RTEditor_SettingsDialog_Themes"));
            }

            if (m_selectedThemeEditor != null)
            {
                m_selectedThemeEditor.Options = m_settings.Themes == null || m_settings.Themes.Length == 0 ? new[] { new RangeOptions.Option(m_localization.GetString("ID_RTEditor_SettingsDialog_Default")) } : m_settings.Themes.Select(theme => new RangeOptions.Option(theme.name)).ToArray();
                m_selectedThemeEditor.Init(m_settings, m_settings, Strong.PropertyInfo((ISettingsComponent x) => x.SelectedThemeIndex), null, m_localization.GetString("ID_RTEditor_SettingsDialog_SelectedTheme"));
            }

            m_customSettings.Clear();
            for (int i = 0; i < m_settings.CustomSettings.Count; ++i)
            {
                GameObject prefab = m_settings.CustomSettings[i];
                if (prefab != null)
                {
                    GameObject customSettings = Instantiate(prefab);
                    customSettings.transform.SetParent(m_panel, false);
                    customSettings.transform.SetSiblingIndex(m_panel.childCount - 2);
                    m_customSettings.Add(customSettings);
                }
            }

            UnityEventHelper.AddListener(m_resetButton, btn => btn.onClick, OnResetClick);
        }

        private IEnumerator CoEndEditUIScale()
        {
            yield return new WaitForEndOfFrame();
            m_parentDialog.ParentRegion.Fit();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            UnityEventHelper.RemoveListener(m_resetButton, btn => btn.onClick, OnResetClick);
        }

        private void OnResetClick()
        {
            IWindowManager wm = IOC.Resolve<IWindowManager>();
            wm.Confirmation("Reset to defaults confirmation", "Are you sure you want to reset to default settings?",
                (dialog, yes) =>
                {
                    m_settings.ResetToDefaults();
                    if (m_uiScaleEditor != null)
                    {
                        m_uiScaleEditor.gameObject.SetActive(!UIAutoScale);
                    }
                },
                (dialog, no) => { },
                "Yes", "No");
        }
    }

}
