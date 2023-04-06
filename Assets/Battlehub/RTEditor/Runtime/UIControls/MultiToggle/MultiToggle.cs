using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Battlehub.UIControls
{
    public class MultiToggle : MonoBehaviour
    {
        private RectTransform m_layer;
            
        [SerializeField]
        private RectTransform.Edge m_location = RectTransform.Edge.Right;

        [SerializeField]
        private MultiTogglePanel m_panel = null;

        private Toggle m_mainToggle;
        private int m_mainToggleIndex;
        private Toggle[] m_toggles;

        private void Awake()
        {
            m_layer = FindObjectOfType<MultiToggleLayer>().RectTransform;
            m_panel.gameObject.SetActive(false);
        }

        private void Start()
        {
            m_panel.Toggle = this;

            SelectToggle(0);
            m_toggles = m_panel.GetComponentsInChildren<Toggle>(true);
            for (int i = 0; i < m_toggles.Length; ++i)
            {
                m_toggles[i].onValueChanged.AddListener(OnToggleValueChanged);
            }
        }

        private void OnDestroy()
        {
            if(m_mainToggle != null)
            {
                m_mainToggle.onValueChanged.RemoveListener(OnMainToggleValueChanged);
            }

            for (int i = 0; i < m_toggles.Length; ++i)
            {
                if(m_toggles[i] != null)
                {
                    m_toggles[i].onValueChanged.RemoveListener(OnToggleValueChanged);
                }
            }
        }

        private void SelectToggle(int index)
        {
            m_mainToggleIndex = index;

            Toggle[] toggles = m_panel.GetComponentsInChildren<Toggle>(true);
            m_mainToggle = GetComponentInChildren<Toggle>();
            if (m_mainToggle != null)
            {
                m_mainToggle.onValueChanged.RemoveListener(OnMainToggleValueChanged);
                Destroy(m_mainToggle.gameObject);
            }

            toggles[index].gameObject.SetActive(false);
            m_mainToggle = Instantiate(toggles[index]);
            m_mainToggle.onValueChanged.RemoveAllListeners();
            m_mainToggle.group = null;
            m_mainToggle.gameObject.SetActive(true);
            toggles[index].gameObject.SetActive(true);
            m_mainToggle.onValueChanged.AddListener(OnMainToggleValueChanged);

            RectTransform rt = m_mainToggle.GetComponent<RectTransform>();
            rt.SetParent(transform, false);
            rt.SetSiblingIndex(0);
            rt.Stretch();
            rt.localPosition = Vector3.zero;
            rt.localScale = Vector3.one;
        }
        

        private void OnMainToggleValueChanged(bool value)
        {
            if(!value)
            {
                m_mainToggle.isOn = true;
                return;
            }
            if (!m_panel.gameObject.activeSelf)
            {
                m_panel.gameObject.SetActive(true);
                RectTransform rt = (RectTransform)m_panel.transform;
                rt.SetInsetAndSizeFromParentEdge(m_location, -5, 0);
                rt.SetParent(m_layer, true);
                m_toggles[m_mainToggleIndex].isOn = true;
            }
        }

        private void OnToggleValueChanged(bool value)
        {
            for(int i = 0; i < m_toggles.Length; ++i)
            {
                m_toggles[i].onValueChanged.RemoveListener(OnToggleValueChanged);
            }

            if(value)
            {
                for(int i = 0; i < m_toggles.Length; ++i)
                {
                    if(m_toggles[i].isOn)
                    {
                        SelectToggle(i);
                        break;
                    }
                }
            }
            else
            {
                m_mainToggle.onValueChanged.RemoveListener(OnMainToggleValueChanged);
                m_mainToggle.isOn = false;
                m_mainToggle.onValueChanged.AddListener(OnMainToggleValueChanged);

                m_panel.Hide();
            }

            for (int i = 0; i < m_toggles.Length; ++i)
            {
                m_toggles[i].onValueChanged.AddListener(OnToggleValueChanged);
            }
        }

    }
}
