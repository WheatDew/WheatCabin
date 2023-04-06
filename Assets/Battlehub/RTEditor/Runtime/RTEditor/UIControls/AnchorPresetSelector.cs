using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Battlehub.RTEditor
{
    public class AnchorPresetSelector : Selectable
    {
        public event Action<AnchorPreset> Selected;

        [SerializeField]
        private AnchorPreset m_preview = null;
        public AnchorPreset Preview
        {
            get { return m_preview; }
        }

        [SerializeField]
        private TMP_Dropdown m_dropDown = null;

        [SerializeField]
        private TextMeshProUGUI m_horizontalAlignmentLabel = null;

        [SerializeField]
        private TextMeshProUGUI m_verticalAlignmentLabel = null;

        [Serializable]
        public class AlignmentCaptions
        {
            public string Left = "left";
            public string Center = "center";
            public string Right = "right";
            public string Top = "top";
            public string Middle = "middle";
            public string Bottom = "bottom";
            public string Stretch = "stretch";
            public string Custom = "custom";
        }

        [SerializeField]
        private AlignmentCaptions m_captions;
        public AlignmentCaptions Captions
        {
            get { return m_captions; }
            set 
            {
                m_captions = value;
                UpdateCaptions();
            }
        }

        public void OnPresetClick(BaseEventData data)
        {
            PointerEventData pointerData = (PointerEventData)data;
            AnchorPreset preset = pointerData.pointerPress.GetComponent<AnchorPreset>();
            m_preview.CopyFrom(preset);
            
            if(Selected != null)
            {
                Selected(preset);
            }

            UpdateCaptions();
            m_dropDown.Hide();
        }


        public void UpdateCaptions()
        {
            switch(m_preview.HorizontalAlignment)
            {
                case AnchorPreset.HAlign.Left:
                    m_horizontalAlignmentLabel.text = Captions.Left;
                    break;
                case AnchorPreset.HAlign.Center:
                    m_horizontalAlignmentLabel.text = Captions.Center;
                    break;
                case AnchorPreset.HAlign.Right:
                    m_horizontalAlignmentLabel.text = Captions.Right;
                    break;
                case AnchorPreset.HAlign.Stretch:
                    m_horizontalAlignmentLabel.text = Captions.Stretch;
                    break;
                case AnchorPreset.HAlign.Custom:
                    m_horizontalAlignmentLabel.text = Captions.Custom;
                    break;
            }

            switch (m_preview.VerticalAlignment)
            {
                case AnchorPreset.VAlign.Top:
                    m_verticalAlignmentLabel.text = Captions.Top;
                    break;
                case AnchorPreset.VAlign.Middle:
                    m_verticalAlignmentLabel.text = Captions.Middle;
                    break;
                case AnchorPreset.VAlign.Bottom:
                    m_verticalAlignmentLabel.text = Captions.Bottom;
                    break;
                case AnchorPreset.VAlign.Stretch:
                    m_verticalAlignmentLabel.text = Captions.Stretch;
                    break;
                case AnchorPreset.VAlign.Custom:
                    m_verticalAlignmentLabel.text = Captions.Custom;
                    break;
            }
        }

        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.LeftShift))
            {
                foreach(AnchorPreset preset in m_dropDown.GetComponentsInChildren<AnchorPreset>())
                {
                    if(preset == m_preview)
                    {
                        continue;
                    }
                    preset.IsPivotVisible = true;
                }
            }
            else if(Input.GetKeyUp(KeyCode.LeftShift))
            {
                foreach (AnchorPreset preset in m_dropDown.GetComponentsInChildren<AnchorPreset>())
                {
                    if (preset == m_preview)
                    {
                        continue;
                    }
                    preset.IsPivotVisible = false;
                }
            }

            if (Input.GetKeyDown(KeyCode.LeftAlt))
            {
                foreach (AnchorPreset preset in m_dropDown.GetComponentsInChildren<AnchorPreset>())
                {
                    if (preset == m_preview)
                    {
                        continue;
                    }
                    preset.IsPositionVisible = true;
                }
            }
            else if (Input.GetKeyUp(KeyCode.LeftAlt))
            {
                foreach (AnchorPreset preset in m_dropDown.GetComponentsInChildren<AnchorPreset>())
                {
                    if (preset == m_preview)
                    {
                        continue;
                    }
                    preset.IsPositionVisible = false;
                }
            }
        }

        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            base.DoStateTransition(state, instant);

            m_preview.gameObject.SetActive(state != SelectionState.Disabled);
            m_horizontalAlignmentLabel.gameObject.SetActive(state != SelectionState.Disabled);
            m_verticalAlignmentLabel.gameObject.SetActive(state != SelectionState.Disabled);
            m_dropDown.interactable = state != SelectionState.Disabled;
        }
    }
}

