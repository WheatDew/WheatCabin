using Battlehub.RTCommon;
using Battlehub.RTHandles;
using Battlehub.Utils;
using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTEditor
{
    public class RectTransformEditor : ComponentEditor
    {
        [SerializeField]
        private AnchorPresetSelector m_presetSelector = null;
        [SerializeField]
        private Vector3Editor m_posEditor = null;
        [SerializeField]
        private Vector3Editor m_sizeEditor = null;
        [SerializeField]
        private Vector2Editor m_anchorMinEditor = null;
        [SerializeField]
        private Vector2Editor m_anchorMaxEditor = null;
        [SerializeField]
        private Vector2Editor m_pivotEditor = null;
        [SerializeField]
        private Vector3Editor m_rotationEditor = null;
        [SerializeField]
        private Vector3Editor m_scaleEditor = null;
        [SerializeField]
        private TextMeshProUGUI m_posXLabel = null;
        [SerializeField]
        private TextMeshProUGUI m_posYLabel = null;
        [SerializeField]
        private TextMeshProUGUI m_posZLabel = null;
        [SerializeField]
        private TextMeshProUGUI m_widthLabel = null;
        [SerializeField]
        private TextMeshProUGUI m_heightLabel = null;
        
        private ILocalization m_lc;

        protected override void Awake()
        {
            base.Awake();
            m_lc = IOC.Resolve<ILocalization>();
            m_presetSelector.Captions = new AnchorPresetSelector.AlignmentCaptions
            {
                Left = m_lc.GetString("ID_RTEditor_AnchorPreset_Left"),
                Center = m_lc.GetString("ID_RTEditor_AnchorPreset_Center"),
                Right = m_lc.GetString("ID_RTEditor_AnchorPreset_Right"),
                Top = m_lc.GetString("ID_RTEditor_AnchorPreset_Top"),
                Middle = m_lc.GetString("ID_RTEditor_AnchorPreset_Middle"),
                Bottom = m_lc.GetString("ID_RTEditor_AnchorPreset_Bottom"),
                Stretch = m_lc.GetString("ID_RTEditor_AnchorPreset_Stretch"),
                Custom = m_lc.GetString("ID_RTEditor_AnchorPreset_Custom"),
            };
            m_presetSelector.Selected += OnAnchorPresetSelected;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if(m_presetSelector != null)
            {
                m_presetSelector.Selected -= OnAnchorPresetSelected;
            }
        }

        protected override void BuildEditor(IComponentDescriptor componentDescriptor, PropertyDescriptor[] descriptors)
        {
            DestroyGizmos();
            TryCreateGizmos(componentDescriptor);

            PropertyDescriptor sizeDesc = descriptors[0];
            PropertyDescriptor anchorMinDesc = descriptors[1];
            PropertyDescriptor anchorMaxDesc = descriptors[2];
            PropertyDescriptor pivotDesc = descriptors[3];
            PropertyDescriptor posDesc = descriptors[4];
            PropertyDescriptor rotationDesc = descriptors[5];
            PropertyDescriptor scaleDesc = descriptors[6];

            InitEditor(m_posEditor, posDesc);
            InitEditor(m_sizeEditor, sizeDesc);
            InitEditor(m_anchorMinEditor, anchorMinDesc);
            InitEditor(m_anchorMaxEditor, anchorMaxDesc);
            InitEditor(m_pivotEditor, pivotDesc);
            InitEditor(m_rotationEditor, rotationDesc);
            InitEditor(m_scaleEditor, scaleDesc);

            UpdatePreview();
            UpdatePositionAndSizeLabels();
            UpdatePropertyEditorsInteractableState();
        }

        protected override PropertyDescriptor[] GetPropertyDescriptors(Type componentType, ComponentEditor editor, object converter)
        {
            IComponentDescriptor componentDescriptor = GetComponentDescriptor();
            object[] converters = (object[])componentDescriptor.CreateConverter(this);
            return new[]
            {
                new PropertyDescriptor("", converters, Strong.MemberInfo((RectTransformPropertyConverter x) => x.Size), Strong.MemberInfo((RectTransform x) => x.sizeDelta)),
                new PropertyDescriptor(m_lc.GetString("ID_RTEditor_CD_RectTransform_AnchorMin"), Components, Strong.MemberInfo((RectTransform x) => x.anchorMin)),
                new PropertyDescriptor(m_lc.GetString("ID_RTEditor_CD_RectTransform_AnchorMax"), Components, Strong.MemberInfo((RectTransform x) => x.anchorMax)),
                new PropertyDescriptor(m_lc.GetString("ID_RTEditor_CD_RectTransform_Pivot"), Components, Strong.MemberInfo((RectTransform x) => x.pivot)),
                new PropertyDescriptor("", converters, Strong.MemberInfo((RectTransformPropertyConverter x) => x.Pos), Strong.MemberInfo((RectTransform x) => x.position)),
                new PropertyDescriptor(m_lc.GetString("ID_RTEditor_CD_RectTransform_Rotation"), converters, Strong.MemberInfo((RectTransformPropertyConverter x) => x.LocalEuler), Strong.MemberInfo((RectTransform x) => x.localEulerAngles)),
                new PropertyDescriptor(m_lc.GetString("ID_RTEditor_CD_RectTransform_Scale"), converters, Strong.MemberInfo((RectTransformPropertyConverter x) => x.LocalScale), Strong.MemberInfo((RectTransform x) => x.localScale))
            };
        }

        protected override void DestroyEditor()
        {
            DestroyGizmos();
        }

        protected override void OnValueReloaded()
        {
            base.OnValueReloaded();

            UpdatePreview();
            UpdatePositionAndSizeLabels();
        }

        protected override void OnValueChanged()
        {
            base.OnValueChanged();
            RefreshTransformHandles();

            UpdatePreview();
            UpdatePositionAndSizeLabels();
        }

        protected override void OnEndEdit()
        {
            base.OnEndEdit();
            ResetTransformHandles();
        }

        protected override void OnResetClick()
        {
            base.OnResetClick();
            ResetTransformHandles();
        }

        private static void RefreshTransformHandles()
        {
            BaseHandle[] handles = FindObjectsOfType<BaseHandle>();
            foreach (BaseHandle handle in handles)
            {
                handle.Refresh();
            }
        }

        private static void ResetTransformHandles()
        {
            BaseHandle[] handles = FindObjectsOfType<BaseHandle>();
            foreach (BaseHandle handle in handles)
            {
                handle.Targets = handle.RealTargets;
            }
        }

        private void UpdatePropertyEditorsInteractableState()
        {
            DrivenTransformProperties drivenProps = GetDrivenTransformProperties();
            m_posEditor.IsXInteractable = (drivenProps & DrivenTransformProperties.AnchoredPositionX) == 0;
            m_posEditor.IsYInteractable = (drivenProps & DrivenTransformProperties.AnchoredPositionY) == 0;
            m_posEditor.IsZInteractable = (drivenProps & DrivenTransformProperties.AnchoredPositionZ) == 0;
            m_sizeEditor.IsXInteractable = (drivenProps & DrivenTransformProperties.SizeDeltaX) == 0;
            m_sizeEditor.IsYInteractable = (drivenProps & DrivenTransformProperties.SizeDeltaY) == 0;
            m_anchorMinEditor.IsXInteractable = (drivenProps & DrivenTransformProperties.AnchorMinX) == 0;
            m_anchorMinEditor.IsYInteractable = (drivenProps & DrivenTransformProperties.AnchorMinY) == 0;
            m_anchorMaxEditor.IsXInteractable = (drivenProps & DrivenTransformProperties.AnchorMaxX) == 0;
            m_anchorMaxEditor.IsYInteractable = (drivenProps & DrivenTransformProperties.AnchorMaxY) == 0;
            m_pivotEditor.IsXInteractable = (drivenProps & DrivenTransformProperties.PivotX) == 0;
            m_pivotEditor.IsYInteractable = (drivenProps & DrivenTransformProperties.PivotY) == 0;
            m_rotationEditor.IsInteractable = (drivenProps & DrivenTransformProperties.Rotation) == 0;
            m_scaleEditor.IsXInteractable = (drivenProps & DrivenTransformProperties.ScaleX) == 0;
            m_scaleEditor.IsYInteractable = (drivenProps & DrivenTransformProperties.ScaleY) == 0;
            m_scaleEditor.IsZInteractable = (drivenProps & DrivenTransformProperties.ScaleZ) == 0;
        }

        public DrivenTransformProperties GetDrivenTransformProperties()
        {
            DrivenTransformProperties result = DrivenTransformProperties.None;

            for(int i = 0; i < Components.Length; ++i)
            {
                Component component = Components[i];
                Canvas canvas = component.GetComponent<Canvas>();
                if(canvas != null)
                {
                    if(canvas.renderMode == RenderMode.ScreenSpaceCamera || canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                    {
                        result = DrivenTransformProperties.All;
                        break;
                    }
                }

                LayoutElement layoutElement = component.GetComponent<LayoutElement>();
                if(layoutElement == null || !layoutElement.ignoreLayout)
                {
                    Transform parent = component.transform.parent;
                    if(parent != null)
                    {
                        LayoutGroup layoutGroup = parent.GetComponent<LayoutGroup>();
                        if(layoutGroup is GridLayoutGroup)
                        {
                            result |= DrivenTransformProperties.AnchoredPositionX;
                            result |= DrivenTransformProperties.AnchoredPositionY;
                            result |= DrivenTransformProperties.AnchorMin;
                            result |= DrivenTransformProperties.AnchorMax;
                            result |= DrivenTransformProperties.SizeDelta;
                        }
                        else if(layoutGroup is HorizontalOrVerticalLayoutGroup)
                        {
                            result |= DrivenTransformProperties.AnchoredPositionX;
                            result |= DrivenTransformProperties.AnchoredPositionY;
                            result |= DrivenTransformProperties.AnchorMin;
                            result |= DrivenTransformProperties.AnchorMax;

                            HorizontalOrVerticalLayoutGroup hvlg = (HorizontalOrVerticalLayoutGroup)layoutGroup;
                            if(hvlg.childControlWidth)
                            {
                                result |= DrivenTransformProperties.SizeDeltaX;
                            }
                            if (hvlg.childControlHeight)
                            {
                                result |= DrivenTransformProperties.SizeDeltaY;
                            }
                        }
                    }
                }
            }

            return result;
        }

        private void UpdatePreview()
        {
            RectTransform rt = (RectTransform)Components.FirstOrDefault();
            if(rt != null)
            {
                m_presetSelector.Preview.CopyFrom(rt);
                m_presetSelector.UpdateCaptions();
            }

            bool isRootCanvas = Components.Any(comp => comp.GetComponent<Canvas>() && (comp.transform.parent == null || comp.transform.parent.GetComponentInParent<Canvas>() == null));
            m_presetSelector.interactable = !isRootCanvas;
        }

        private void UpdatePositionAndSizeLabels()
        {
            Vector3 anchorMax = m_anchorMaxEditor.GetValue();
            Vector3 anchorMin = m_anchorMinEditor.GetValue();
            if (RectTransformPropertyConverter.Approximately(anchorMin.x, anchorMax.x))
            {
                m_posXLabel.text = m_lc.GetString("ID_RTEditor_CD_RectTransform_PosX");
                m_widthLabel.text = m_lc.GetString("ID_RTEditor_CD_RectTransform_Width");
            }
            else
            {
                m_posXLabel.text = m_lc.GetString("ID_RTEditor_CD_RectTransform_Left");
                m_widthLabel.text = m_lc.GetString("ID_RTEditor_CD_RectTransform_Right");
            }

            if (RectTransformPropertyConverter.Approximately(anchorMin.y, anchorMax.y))
            {
                m_posYLabel.text = m_lc.GetString("ID_RTEditor_CD_RectTransform_PosY");
                m_heightLabel.text = m_lc.GetString("ID_RTEditor_CD_RectTransform_Height");
            }
            else
            {
                m_posYLabel.text = m_lc.GetString("ID_RTEditor_CD_RectTransform_Top");
                m_heightLabel.text = m_lc.GetString("ID_RTEditor_CD_RectTransform_Bottom");
            }

            m_posZLabel.text = m_lc.GetString("ID_RTEditor_CD_RectTransform_PosZ");
        }

        private void OnAnchorPresetSelected(AnchorPreset preset)
        {
            Editor.Undo.BeginRecord();

            for(int i = 0; i < Components.Length; ++i)
            {
                RectTransform rt = Components[i] as RectTransform;
                if(rt == null)
                {
                    continue;
                }

                int index = i;

                Vector2 anchorMinOld = rt.anchorMin;
                Vector2 anchorMaxOld = rt.anchorMax;
                Vector2 anchoredPositionOld = rt.anchoredPosition;
                Vector2 pivotOld = rt.pivot;
                Vector2 sizeDeltaOld = rt.sizeDelta;
                Vector2 offsetMinOld = rt.offsetMin;
                Vector2 offsetMaxOld = rt.offsetMax;

                preset.CopyTo(rt, preset.IsPivotVisible, preset.IsPositionVisible);

                Vector2 anchorMinNew = rt.anchorMin;
                Vector2 anchorMaxNew = rt.anchorMax;
                Vector2 anchoredPositionNew = rt.anchoredPosition;
                Vector2 pivotNew = rt.pivot;
                Vector2 sizeDeltaNew = rt.sizeDelta;
                Vector2 offsetMinNew = rt.offsetMin;
                Vector2 offsetMaxNew = rt.offsetMax;

                Editor.Undo.CreateRecord(redo =>
                {
                    RectTransform rectTransform = Components[index] as RectTransform;
                    rectTransform.anchorMin = anchorMinNew;
                    rectTransform.anchorMax = anchorMaxNew;
                    rectTransform.anchoredPosition = anchoredPositionNew;
                    rectTransform.pivot = pivotNew;
                    rectTransform.sizeDelta = sizeDeltaNew;
                    rectTransform.offsetMin = offsetMinNew;
                    rectTransform.offsetMax = offsetMaxNew;

                    return true;
                }, undo =>
                {
                    RectTransform rectTransform = Components[index] as RectTransform;
                    rectTransform.anchorMin = anchorMinOld;
                    rectTransform.anchorMax = anchorMaxOld;
                    rectTransform.anchoredPosition = anchoredPositionOld;
                    rectTransform.pivot = pivotOld;
                    rectTransform.sizeDelta = sizeDeltaOld;
                    rectTransform.offsetMin = offsetMinOld;
                    rectTransform.offsetMax = offsetMaxOld;
                    return true;
                });
            }

            Editor.Undo.EndRecord();

            RefreshTransformHandles();
        }
    }
}

