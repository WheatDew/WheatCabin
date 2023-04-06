using Battlehub.RTCommon;
using Battlehub.UIControls;
using UnityEngine;

namespace Battlehub.RTEditor
{
    public class CreateSceneLayer : RuntimeWindowExtension
    {
        [SerializeField]
        private RectTransform m_layer = null;

        public override string WindowTypeName => BuiltInWindowNames.Scene;

        protected override void Extend(RuntimeWindow window)
        {
            Instantiate(m_layer, window.ViewRoot).Stretch();
        }
    }
}