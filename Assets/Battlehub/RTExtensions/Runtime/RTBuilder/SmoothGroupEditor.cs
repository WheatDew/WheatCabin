using Battlehub.ProBuilderIntegration;
using Battlehub.UIControls;
using Battlehub.Utils;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTBuilder
{
    public class SmoothGroupArgs : EventArgs
    {
        public readonly int Index;
        public SmoothGroupArgs(int index)
        {
            Index = index;
        }
    }

    public class SmoothGroupEditor : MonoBehaviour
    {
        public event EventHandler ExpandSelection;
        public event EventHandler Clear;
        public event EventHandler<SmoothGroupArgs> Smooth;

        [SerializeField]
        private TextMeshProUGUI m_txtGroupName = null;

        [SerializeField]
        private Button m_btnExpandSelection = null;

        [SerializeField]
        private Button m_btnClear = null;

        [SerializeField]
        private Toggle m_toggleGroup = null;

        private Toggle[] m_toggles;

        private PBSmoothGroupData m_data;
        public PBSmoothGroupData Data
        {
            get { return m_data; }
            set
            {
                m_data = value;
                m_txtGroupName.text = m_data.PBMesh.name;
            }
        }

        private void Start()
        {
            m_toggles = new Toggle[PBSmoothing.smoothRangeMax - PBSmoothing.smoothRangeMin];
            m_toggles[0] = m_toggleGroup;
            for (int i = 1; i < m_toggles.Length; ++i)
            {
                m_toggles[i] = Instantiate(m_toggleGroup, m_toggleGroup.transform.parent);
            }

            UnityEventHelper.AddListener(m_btnClear, btn => btn.onClick, OnClearClick);
            UnityEventHelper.AddListener(m_btnExpandSelection, btn => btn.onClick, OnExpandSelectionClick);
            for(int i = 0; i < m_toggles.Length; ++i)
            {
                UpdateToogleText(i);

                int index = i;
                m_toggles[i].name = "Toggle Group " + i.ToString();
                UnityEventHelper.AddListener(m_toggles[i], toggle => toggle.onValueChanged, value => OnToggleValueChanged(index, value));
            }

            foreach (int group in m_data.Groups)
            {
                if(group == PBSmoothing.smoothingGroupNone)
                {
                    continue;
                }

                SetToggleState(group - 1, true);
            }
        }

        private void OnDestroy()
        {
            UnityEventHelper.RemoveListener(m_btnClear, btn => btn.onClick, OnClearClick);
            UnityEventHelper.RemoveListener(m_btnExpandSelection, btn => btn.onClick, OnExpandSelectionClick);
            if(m_toggles != null)
            {
                for (int i = 0; i < m_toggles.Length; ++i)
                {
                    if(m_toggles[i] != null)
                    {
                        UnityEventHelper.RemoveAllListeners(m_toggles[i], toggle => toggle.onValueChanged);
                    }
                }
            }
        }

        private void UpdateToogleText(int i)
        {
            TextMeshProUGUI text = m_toggles[i].GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = (i + 1).ToString();
            }
        }

        private void OnClearClick()
        {
            if(Clear != null)
            {
                Clear(this, EventArgs.Empty);
            }
        }

        private void OnExpandSelectionClick()
        {
            if(ExpandSelection != null)
            {
                ExpandSelection(this, EventArgs.Empty);
            }
        }

        private void OnToggleValueChanged(int index, bool value)
        {
            if (Smooth != null)
            {
                Smooth(this, new SmoothGroupArgs(index + 1));
            }

            for(int i = 0; i < m_toggles.Length; ++i)
            {
                SetToggleState(i, false);
            }

            foreach (int group in m_data.Groups)
            {
                if (group == PBSmoothing.smoothingGroupNone)
                {
                    continue;
                }

                SetToggleState(group - 1, true);
            }
        }

        public void Select(int index, bool value)
        {
            SetToggleState(index, value);
        }

        private void SetToggleState(int index, bool value)
        {
            Toggle toggle = m_toggles[index];

            toggle.SetIsOnWithoutNotify(value);

            Image groupColorImage = toggle.transform.Find("GroupColor").GetComponent<Image>();
            if (value)
            {
                groupColorImage.color = Colors.Kellys[(index + 1) % Colors.Kellys.Length];
            }
            else
            {
                groupColorImage.color = new Color(0, 0, 0, 0);
            }
        }
    }
}
