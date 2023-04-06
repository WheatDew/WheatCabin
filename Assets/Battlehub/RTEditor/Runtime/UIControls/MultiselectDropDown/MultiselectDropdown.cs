using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static TMPro.TMP_Dropdown;

namespace Battlehub.UIControls
{
    public class MultiselectDropdown : MonoBehaviour
    {
        public class OptionData
        {
            public string Text;
        }

        public DropdownEvent onSelected = null;
        public DropdownEvent onUnselected = null;
        
        [SerializeField]
        private TMP_Dropdown m_dropdown = null;

        [SerializeField]
        private TextMeshProUGUI m_label = null;

        private bool m_isExpanded;
        private Toggle[] m_toggles;
        private List<OptionData> m_options;
        private readonly HashSet<int> m_selectedIndexes = new HashSet<int>();
        public int[] selectedIndexes
        {
            get { return m_selectedIndexes.ToArray(); }
        }

        public string displayText
        {
            get { return m_label.text; }
        }
        
        public List<OptionData> options
        {
            get { return m_options; }
            set
            {
                m_options = value;
                m_dropdown.options = m_options.Select(opt => new TMP_Dropdown.OptionData(opt.Text)).ToList();
            }
        }

        public void SelectWithoutNotify(int[] selectedIndexes)
        {
            m_selectedIndexes.Clear();
            foreach (int index in selectedIndexes)
            {
                m_selectedIndexes.Add(index);
            }
            OnSelectionChanged();
        }

        private void Awake()
        {
            m_options = m_dropdown.options.Select(opt => new OptionData { Text = opt.text }).ToList();
            m_dropdown.onValueChanged.AddListener(OnValueChanged);
            UpdateLabel();
        }

        private void OnDestroy()
        {
            if(m_dropdown.onValueChanged != null)
            {
                m_dropdown.onValueChanged.RemoveListener(OnValueChanged);
            }
        }

        private void LateUpdate()
        {
            if(m_isExpanded != m_dropdown.IsExpanded)
            {
                m_isExpanded = m_dropdown.IsExpanded;
                if(m_isExpanded)
                {
                    m_toggles = m_dropdown.GetComponentsInChildren<Toggle>().Where(tog => !tog.name.StartsWith("Item") ).ToArray();
                    for(int i = 0; i < m_toggles.Length; ++i)
                    {
                        Toggle toggle = m_toggles[i];
                        int index = i;
                        toggle.SetIsOnWithoutNotify(m_selectedIndexes.Contains(index));
                        toggle.onValueChanged.AddListener(value => OnToggleValueChanged(index, value));
                    }
                }
                else
                {
                    if(m_toggles != null)
                    {
                        for (int i = 0; i < m_toggles.Length; ++i)
                        {
                            Toggle toggle = m_toggles[i];
                            toggle.onValueChanged.RemoveAllListeners();
                        }
                        m_toggles = null;
                    }
                   
                }

            }
        }

        private void OnToggleValueChanged(int index, bool value)
        {
            if(value)
            {
                m_selectedIndexes.Add(index);
                onSelected.Invoke(index);
                OnSelectionChanged();
            }
            else
            {
                m_selectedIndexes.Remove(index);
                onUnselected.Invoke(index);
                OnSelectionChanged();
            }
        }

        private void OnValueChanged(int index)
        {
            UpdateLabel();
        }

        private void OnSelectionChanged()
        {
            UpdateLabel();
        }

        private void UpdateLabel()
        {
            if (m_selectedIndexes.Count == 0)
            {
                m_label.text = "Nothing";
            }
            else if (m_selectedIndexes.Count == m_options.Count)
            {
                m_label.text = "Everything";
            }
            else if (m_selectedIndexes.Count == 1)
            {
                m_label.text = m_dropdown.options[m_selectedIndexes.First()].text;
            }
            else
            {
                m_label.text = "Mixed...";
            }
        }

        
    }
}
