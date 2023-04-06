using UnityEngine;
using UnityWeld.Binding.Adapters;

namespace Battlehub.RTEditor.Views
{
    public class HierarchyView : HierarchicalDataView
    {
        [SerializeField]
        private BoolToColorAdapterOptions m_activeSelfToColorOptions = null;

        public Color EnabledItemColor
        {
            get { return m_activeSelfToColorOptions.TrueColor; }
            set { m_activeSelfToColorOptions.TrueColor = value; }
        }

        public Color DisabledItemColor
        {
            get { return m_activeSelfToColorOptions.FalseColor; }
            set { m_activeSelfToColorOptions.FalseColor = value; }
        }


        protected virtual void Update()
        {
            ViewInput.HandleInput();
        }
    }
}
