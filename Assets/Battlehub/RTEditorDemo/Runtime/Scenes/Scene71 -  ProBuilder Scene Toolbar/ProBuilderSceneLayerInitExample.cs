
using Battlehub.RTCommon;
using Battlehub.UIControls;
using UnityEngine;

namespace Battlehub.RTEditor.Examples.Scene71
{
    public class ProBuilderSceneLayerInitExample : RuntimeWindowExtension
    {
        [SerializeField]
        private RectTransform m_probuilderTools = null;

        public override string WindowTypeName => BuiltInWindowNames.Scene;

        protected override void Extend(RuntimeWindow window)
        {
            if(m_probuilderTools != null)
            {
                RectTransform layer = Instantiate(m_probuilderTools, window.ViewRoot);
                layer.Stretch();
            }
        }
    }

}
