using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTBuilder
{
    public class WireframeToggle : MonoBehaviour
    {
        [SerializeField]
        private Toggle m_toggle = null;
        public Toggle Toggle
        {
            get { return m_toggle; }
        }

        public bool IsStarted
        {
            get { return m_isStarted; }
        }

        private bool m_isStarted;
        private void Start()
        {
            m_isStarted = true;
            m_toggle.isOn = GetComponent<Wireframe>() != null;
            m_toggle.onValueChanged.AddListener(OnWireframeToggleValueChanged);
        }

        private void OnDestroy()
        {
            if(m_toggle != null)
            {
                m_toggle.onValueChanged.RemoveListener(OnWireframeToggleValueChanged);
            }
        }

        private void OnWireframeToggleValueChanged(bool value)
        {
            Wireframe wireframe = GetComponent<Wireframe>();
            if (!value)
            {
                if(wireframe)
                {
                    Destroy(wireframe);
                }   
            }
            else
            {
                if(!wireframe)
                {
                    AddComponent();
                }
            }
        }

        protected virtual void AddComponent()
        {
            gameObject.AddComponent<Wireframe>();
        }
    }

}
