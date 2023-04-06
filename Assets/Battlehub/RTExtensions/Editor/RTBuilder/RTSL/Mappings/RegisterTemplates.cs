#if UNITY_EDITOR
using Battlehub.RTSL;
using UnityEditor;

namespace Battlehub.RTBuilder
{
    public static class RegisterTemplates 
    {
        [InitializeOnLoadMethod]
        public static void Register()
        {
            RTSLPath.ClassMappingsTemplatePath.Add(BHRoot.PackageEditorContentPath + "/RTBuilder/RTSL/Mappings/RTBuilder.ClassMappingsTemplate.prefab");
            RTSLPath.SurrogatesMappingsTemplatePath.Add(BHRoot.PackageEditorContentPath + "/RTBuilder/RTSL/Mappings/RTBuilder.SurrogatesMappingsTemplate.prefab");
        }
    }
}
#endif
