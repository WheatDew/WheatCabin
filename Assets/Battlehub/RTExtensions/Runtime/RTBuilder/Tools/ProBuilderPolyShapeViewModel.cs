using Battlehub.RTCommon;
using Battlehub.RTEditor.ViewModels;
using UnityEngine;
using UnityWeld.Binding;

namespace Battlehub.RTBuilder
{
    [Binding]
    public class ProBuilderPolyShapeViewModel : ViewModelBase
    {
        [Binding]
        public LayerMask LayerMask
        {
            get { return m_polyShapeTool.LayerMask; }
            set
            {
                if (m_polyShapeTool.LayerMask != value)
                {
                    m_polyShapeTool.LayerMask = value;
                    RaisePropertyChanged(nameof(LayerMask));
                }
            }
        }

        private IPolyShapeTool m_polyShapeTool;

        private void Awake()
        {
            m_polyShapeTool = IOC.Resolve<IPolyShapeTool>();
        }

        private void OnDestroy()
        {
            m_polyShapeTool = null;
        }
    }

}

