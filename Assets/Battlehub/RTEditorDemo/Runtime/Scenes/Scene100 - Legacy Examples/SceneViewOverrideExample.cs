

using UnityEngine;

namespace Battlehub.RTEditor.Demo
{
    public class SceneViewOverrideExample : Views.SceneView
    {
        public override bool CanDropExternalObjects 
        {
            get { return base.CanDropExternalObjects; }
            set
            {
                base.CanDropExternalObjects = value;
                Debug.Log($"SceneViewOverrideExample CanDropExternalObjects = {value}" );
            }
        }

        protected override void Awake()
        {
            base.Awake();
            Debug.Log("SceneViewOverrideExample Awake");
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Debug.Log("SceneViewOverrideExample OnDestroy");
        }
    }

}
