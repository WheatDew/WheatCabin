using Battlehub.RTCommon;
using Battlehub.UIControls;
using UnityEngine;

namespace Battlehub.RTEditor.Examples.Scene3
{
    /// <summary>
    /// This is a simple example of how you can create a layer on top of a RuntimeWindow
    /// </summary>
    public class SimpleLayerExample : RuntimeWindowExtension
    {
        /// <summary>
        /// Layer prefab
        /// </summary>
        [SerializeField]
        private RectTransform m_layerPrefab;

        /// <summary>
        /// Type of window on top of which the layer will be created
        /// </summary>
        public override string WindowTypeName => BuiltInWindowNames.Scene;

        /// <summary>
        /// This method is called for every window of the specified type. 
        /// Here you can add any code that extends the window.
        /// In our case we just instantiate and stretch the layer with the Text control.
        /// For an advanced example, see AnnotationsLayerExample.cs
        /// </summary>
        /// <param name="window"></param>
        protected override void Extend(RuntimeWindow window)
        {
            RectTransform layer = Instantiate(m_layerPrefab, window.ViewRoot);
            layer.Stretch();
        }
    }
}

