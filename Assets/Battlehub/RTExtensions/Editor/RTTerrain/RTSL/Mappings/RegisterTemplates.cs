#if UNITY_EDITOR
using Battlehub.RTSL;
using UnityEditor;

namespace Battlehub.RTTerrain
{
    public static class RegisterTemplates 
    {
        [InitializeOnLoadMethod]
        public static void Register()
        {
            RTSLPath.ClassMappingsTemplatePath.Add(BHRoot.PackageEditorContentPath + "/RTTerrain/RTSL/Mappings/RTTerrain.ClassMappingsTemplate.prefab");
        }
    }
}
#endif
