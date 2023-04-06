#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Battlehub.RTEditor
{
    [CustomEditor(typeof(RTEAppearance))]
    public class RTEAppearanceEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            /*
            RTEAppearance rteAppearance = (RTEAppearance)target;

            if(GUILayout.Button("Reset Colors"))
            {
                rteAppearance.Colors = new RTEColors();
                EditorUtility.SetDirty(rteAppearance);
            }*/
        }
    }
}
#endif

