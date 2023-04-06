using UnityEngine;

namespace Battlehub.RTEditor
{
    public class EditorsMapStorage : MonoBehaviour
    {
        public const string EditorsMapPrefabName = "Battlehub_EditorsMapAuto";
        public const string EditorsMapTemplateName = "Battlehub_EditorsMapTemplate";

        //[HideInInspector]
        public GameObject[] Editors;

        //[HideInInspector]
        public bool IsDefaultMaterialEditorEnabled;
        //[HideInInspector]
        public GameObject DefaultMaterialEditor;
        //[HideInInspector]
        public Shader[] Shaders;
        //[HideInInspector]
        public GameObject[] MaterialEditors;
        //[HideInInspector]
        public bool[] IsMaterialEditorEnabled;
     
    }
}
