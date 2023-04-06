using UnityEngine;
using Battlehub.RTCommon;
using TMPro;

namespace Battlehub.RTEditor
{
    [AddComponentMenu(""), /*System.Obsolete*/]
    public class HierarchyView : RuntimeWindow
    {
        public TMP_InputField FilterInput;
        public Transform TreePanel;
        public GameObject TreeViewPrefab;
        public Color DisabledItemColor = new Color(0.5f, 0.5f, 0.5f);
        public Color EnabledItemColor = new Color(0.2f, 0.2f, 0.2f);
        
        private HierarchyViewImpl m_impl;
        protected override void AwakeOverride()
        {
            WindowType = RuntimeWindowType.Hierarchy;
            base.AwakeOverride();
            if (!TreeViewPrefab)
            {
                Debug.LogError("Set TreeViewPrefab field");
                return;
            }
        }

        private void Start()
        {
            m_impl = GetComponent<HierarchyViewImpl>();
            if(!m_impl)
            {
                m_impl = gameObject.AddComponent<HierarchyViewImpl>();
            }

            if (!GetComponent<HierarchyViewInput>())
            {
                gameObject.AddComponent<HierarchyViewInput>();
            }
        }

        public void SelectAll()
        {
            m_impl.SelectAll();
        }
    }
}

