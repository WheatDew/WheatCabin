#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Battlehub.RTSL
{
    public static class RTSLPath
    {
        public static string SaveLoadRoot
        {
            get { return BHRoot.PackageEditorContentPath + @"/RTSL"; }
        }

        public static string UserRoot
        {
            get
            {
                string userRoot = EditorPrefs.GetString("RTSL_Data_RootFolder");
                if (string.IsNullOrEmpty(userRoot))
                {
                    string dll = AssetDatabase.FindAssets(TypeModel).FirstOrDefault();
                    if (string.IsNullOrEmpty(dll))
                    {
                        return BHRoot.AssetsPath + "/RTSL_Data";
                    }
                    string path = AssetDatabase.GUIDToAssetPath(dll);
                    path =  path.Remove(path.LastIndexOf(TypeModel));
                    if (string.IsNullOrEmpty(path))
                    {
                        return BHRoot.AssetsPath + "/RTSL_Data";
                    }

                    return path.TrimEnd(new[] { '/', '\\' });
                }
                userRoot = userRoot.Trim(new[] { '/', '\\' });
                return userRoot;
            }
            set
            {
                EditorPrefs.SetString("RTSL_Data_RootFolder", value);
            }
        }

        private static string EditorPrefabsPath { get { return SaveLoadRoot + "/Prefabs"; } }
        public static string UserPrefabsPath { get { return UserRoot + "/Mappings/Editor"; } }
        public static string FilePathStoragePath { get { return UserPrefabsPath + @"/FilePathStorage.prefab"; } }
        public static string ClassMappingsStoragePath { get { return UserPrefabsPath + @"/ClassMappingsStorage.prefab"; } }
        public static string SurrogatesMappingsStoragePath { get { return UserPrefabsPath + @"/SurrogatesMappingsStorage.prefab"; } }
        public static IList<string> ClassMappingsTemplatePath = new List<string>
        {
            EditorPrefabsPath + @"/ClassMappingsTemplate.prefab"
        };
        public static readonly IList<string> SurrogatesMappingsTemplatePath = new List<string>
        {            
            EditorPrefabsPath + @"/SurrogatesMappingsTemplate.prefab"
        };

        public const string ScriptsAutoFolder = "Scripts";
        public const string PersistentClassesFolder = "PersistentClasses";
        public const string PersistentCustomImplementationClasessFolder = "CustomImplementation";
        public const string LibrariesFolder = "Libraries";
        public const string TypeModel = "RTSLTypeModel";
    }
}
#endif