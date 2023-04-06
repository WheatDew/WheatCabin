using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Battlehub.RTSL.Internal
{
    public static class MappingsExportImportMenu
    {
        [MenuItem("Tools/Runtime SaveLoad/Persistent Classes/Export Mappings")]
        public static void ExportMappings()
        {
            PersistentMappingsExporterWindow prevWindow = EditorWindow.GetWindow<PersistentMappingsExporterWindow>();
            if (prevWindow != null)
            {
                prevWindow.Close();
            }


            PersistentMappingsExporterWindow window = ScriptableObject.CreateInstance<PersistentMappingsExporterWindow>();
            window.titleContent = new GUIContent("Export Mappings");
            window.Show();
            window.position = new Rect(20, 40, 1280, 768);
        }

        [MenuItem("Tools/Runtime SaveLoad/Persistent Classes/Import Mappings")]
        public static void ImportMappings()
        {
            string packagePath = EditorUtility.OpenFilePanel("Import Mappings", "", "unitypackage");
            if (string.IsNullOrEmpty(packagePath))
            {
                return;
            }
            string mappingsPath = Application.dataPath + "/Mappings.txt";
            EditorCoroutine.Start(CoImport(mappingsPath, packagePath));
        }


        [DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            if (EditorPrefs.GetBool("RTSL_ImportMappings"))
            {
                string mappingsPath = Application.dataPath + "/Mappings.txt";
                CompleteImportMappings(mappingsPath);
            }
        }

        private static IEnumerator CoImport(string mappingsPath, string packagePath)
        {
            AssetDatabase.StartAssetEditing();
            //AssetDatabase.DeleteAsset("Assets" + RTSLPath.UserRoot + "/CustomImplementation");
            //AssetDatabase.DeleteAsset("Assets" + RTSLPath.UserRoot + "/Mappings/Editor/FilePathStorage.prefab");
            AssetDatabase.DeleteAsset(RTSLPath.UserRoot + "/Scripts");
            //AssetDatabase.DeleteAsset("Assets" + RTSLPath.UserRoot + "/RTSLTypeModel.dll");
            AssetDatabase.StopAssetEditing();

            EditorPrefs.SetBool("RTSL_ImportMappings", true);
            AssetDatabase.ImportPackage(packagePath, false);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            yield return null;
            CompleteImportMappings(mappingsPath);
        }

        private static void CompleteImportMappings(string mappingsPath)
        {
            EditorPrefs.DeleteKey("RTSL_ImportMappings");

            PersistentClassMapping[] classMappings = null;
            PersistentClassMapping[] surrogateMappings = null;

            try
            {
                MappingsUtility.Import(mappingsPath, out classMappings, out surrogateMappings);
                MappingsUtility.MergeClassMappings(classMappings);
                MappingsUtility.MergeSurrogateMappings(surrogateMappings);
            }
            finally
            {
                Cleanup(classMappings);
                Cleanup(surrogateMappings);
                File.Delete(mappingsPath);
                File.Delete(mappingsPath + ".meta");
            }

            EditorCoroutine.Start(CoBuildAll());
        }

        private static IEnumerator CoBuildAll()
        {
            yield return null;
            PersistentClassMapperWindow.SaveMappings();
            yield return null;
            Menu.BuildAll();
        }

        private static void Cleanup(PersistentClassMapping[] mappings)
        {
            if (mappings != null)
            {
                for (int i = 0; i < mappings.Length; ++i)
                {
                    if (mappings[i] != null)
                    {
                        Object.DestroyImmediate(mappings[i].gameObject);
                    }
                }
            }
        }
    }

}
