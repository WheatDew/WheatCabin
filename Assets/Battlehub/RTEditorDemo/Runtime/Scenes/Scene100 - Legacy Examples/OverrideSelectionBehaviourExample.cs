using Battlehub.RTCommon;
using Battlehub.RTHandles;
using UnityEngine;

namespace Battlehub.RTEditor
{
    public class OverrideSelectionBehaviourExample : SceneComponentExtension
    {
        protected override void OnSceneActivated(IRuntimeSceneComponent sceneComponent)
        {
            base.OnSceneActivated(sceneComponent);
            sceneComponent.SelectionChanging += OnSelectionChanging;
        }

        protected override void OnSceneDeactivated(IRuntimeSceneComponent sceneComponent)
        {
            base.OnSceneDeactivated(sceneComponent);
            sceneComponent.SelectionChanging -= OnSelectionChanging;
        }

        private void OnSelectionChanging(object sender, RuntimeSelectionChangingArgs e)
        {
            var selected = e.Selected;
            for (int i = selected.Count - 1; i >= 0; i--)
            {
                GameObject go = (GameObject)selected[i];
                ExposeToEditor exposed = go.GetComponent<ExposeToEditor>();
                ExposeToEditor parent = exposed.GetParent();
                if (parent != null)
                {
                    selected.Add(parent.gameObject);
                    selected.RemoveAt(i);
                }
            }
        }
    }

}

