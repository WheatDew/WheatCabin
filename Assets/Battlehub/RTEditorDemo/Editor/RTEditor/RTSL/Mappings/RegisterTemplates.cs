#if UNITY_EDITOR
using Battlehub.RTSL;
using UnityEditor;

namespace Battlehub.RTEditor.Demo
{
    public static class RegisterTemplates 
    {
        [InitializeOnLoadMethod]
        public static void Register()
        {
            RTSLPath.ClassMappingsTemplatePath.Add(BHRoot.PackageEditorContentPath + "/RTEditor/RTSL/Mappings/RTEditorDemo.ClassMappingsTemplate.prefab");
        }
    }
}
#endif
