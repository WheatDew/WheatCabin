using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.UIControls.Binding
{
    [RequireComponent(typeof(Selectable))]
    public class SelectableBinding : ControlBinding
    {
        private Selectable m_dropDown = null;
        public override Component TargetControl
        {
            get
            {
                if(m_dropDown == null)
                {
                    m_dropDown = gameObject.GetComponent<Selectable>();
                }
                return m_dropDown;
            }
        }
    }
}
