using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Battlehub.RTSL.Internal
{
    public static class MappingsUtility 
    {
        public static PersistentClassMapping[] GetAllMappings()
        {
            List<PersistentClassMapping> mappingsList = new List<PersistentClassMapping>();
            GetMappings(RTSLPath.ClassMappingsStoragePath, RTSLPath.ClassMappingsTemplatePath.ToArray(), mappingsList, out PersistentClassMappingsStorage storage);
            GetMappings(RTSLPath.SurrogatesMappingsStoragePath, RTSLPath.SurrogatesMappingsTemplatePath.ToArray(), mappingsList, out storage);
            return mappingsList.ToArray();
        }

        public static PersistentClassMapping[] GetClassMappings()
        {
            PersistentClassMappingsStorage storage;
            return GetClassMappings(out storage);
        }

        public static PersistentClassMapping[] GetSurrogateMappings()
        {
            PersistentClassMappingsStorage storage;
            return GetSurrogateMappings(out storage);
        }

        public static PersistentClassMapping[] GetClassMappings(out PersistentClassMappingsStorage storage)
        {
            return GetMappings(RTSLPath.ClassMappingsStoragePath, RTSLPath.ClassMappingsTemplatePath.ToArray(), out storage);
        }

        public static PersistentClassMapping[] GetSurrogateMappings(out PersistentClassMappingsStorage storage)
        {
            return GetMappings(RTSLPath.SurrogatesMappingsStoragePath, RTSLPath.SurrogatesMappingsTemplatePath.ToArray(), out storage);
        }

        public static PersistentClassMapping[] GetMappings(string mappingStoragePath, string[] mappingTemplateStoragePath, out PersistentClassMappingsStorage storage)
        {
            List<PersistentClassMapping> mappingsList = new List<PersistentClassMapping>();
            GetMappings(mappingStoragePath, mappingTemplateStoragePath, mappingsList, out storage);
            return mappingsList.ToArray();
        }

        private static void GetMappings(string mappingStoragePath, string[] mappingTemplateStoragePath, List<PersistentClassMapping> mappingsList, out PersistentClassMappingsStorage storage)
        {
            List<GameObject> storageList = new List<GameObject>();
            GameObject storageGO = (GameObject)AssetDatabase.LoadAssetAtPath(mappingStoragePath, typeof(GameObject));
            if (storageGO != null)
            {
                storageList.Add(storageGO);
            }
            else
            {
                for (int i = 0; i < mappingTemplateStoragePath.Length; ++i)
                {
                    storageGO = (GameObject)AssetDatabase.LoadAssetAtPath(mappingTemplateStoragePath[i], typeof(GameObject));
                    if (storageGO != null)
                    {
                        storageList.Add(storageGO);
                    }
                }
            }

            if (storageList != null && storageList.Count > 0)
            {
                for (int i = 0; i < storageList.Count; ++i)
                {
                    PersistentClassMapping[] mappings = storageList[i].GetComponentsInChildren<PersistentClassMapping>(true);
                    mappingsList.AddRange(mappings);
                }

                storage = storageList[0].GetComponent<PersistentClassMappingsStorage>();
            }
            else
            {
                storage = null;
            }
        }

        public static string FixTypeName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return name;
            }

            name = Regex.Replace(name, @", Version=\d+.\d+.\d+.\d+", string.Empty);
            name = Regex.Replace(name, @", Culture=\w+", string.Empty);
            name = Regex.Replace(name, @", PublicKeyToken=\w+", string.Empty);
            return name;
        }

        public class MappingsStorage
        {
            public PersistentClassMapping[] Mappings;
        }

        public static void Export(string path, PersistentClassMapping[] classMappings, PersistentClassMapping[] surrogateMappings)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < classMappings.Length; ++i)
            {
                sb.Append(JsonUtility.ToJson(classMappings[i], true)).Append("#").Append(Environment.NewLine);
            }
            sb.Append("#SurrogateMappings#" + Environment.NewLine);
            for (int i = 0; i < surrogateMappings.Length; ++i)
            {
                sb.Append(JsonUtility.ToJson(surrogateMappings[i], true)).Append("#").Append(Environment.NewLine);
            }
            File.WriteAllText(path, sb.ToString());   
        }

        public static void Import(string path, out PersistentClassMapping[] classMappings, out PersistentClassMapping[] surrogateMappings)
        {
            string text = File.ReadAllText(path);
            string[] parts = text.Split(new[] { "#SurrogateMappings#" }, StringSplitOptions.None);

            classMappings = string.IsNullOrWhiteSpace(parts[0]) ? new PersistentClassMapping[0] : Import(parts[0]);
            surrogateMappings = string.IsNullOrWhiteSpace(parts[1]) ? new PersistentClassMapping[0] : Import(parts[1]);
        }

        private static PersistentClassMapping[] Import(string text)
        {
            string[] json = text.Split(new[] { "#" + Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            List<PersistentClassMapping> mappings = new List<PersistentClassMapping>();
            for (int i = 0; i < json.Length; ++i)
            {
                PersistentClassMapping mapping = new GameObject().AddComponent<PersistentClassMapping>();
                JsonUtility.FromJsonOverwrite(json[i], mapping);
                mapping.name = mapping.MappedFullTypeName;
                mappings.Add(mapping);
            }
            return mappings.ToArray();
        }

        public static void MergeClassMappings(PersistentClassMapping[] newMappings)
        {
            MergeMappings(RTSLPath.ClassMappingsStoragePath, newMappings);
        }

        public static void MergeSurrogateMappings(PersistentClassMapping[] newMappings)
        {
            MergeMappings(RTSLPath.SurrogatesMappingsStoragePath, newMappings);
        }

        private static void MergeMappings(string storagePath, PersistentClassMapping[] newMappings)
        {
            GameObject storageGO = (GameObject)UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath(storagePath, typeof(GameObject)));
            PersistentClassMapping[] mappings = storageGO.GetComponentsInChildren<PersistentClassMapping>(true);

            Dictionary<string, PersistentClassMapping> typeNameToMappings = newMappings.ToDictionary(mapping => mapping.MappedFullTypeName);
            for(int i = 0; i < mappings.Length; ++i)
            {
                PersistentClassMapping newMapping;
                if (typeNameToMappings.TryGetValue(mappings[i].MappedFullTypeName, out newMapping))
                {
                    newMapping.name = mappings[i].name;
                    
                    typeNameToMappings.Remove(mappings[i].MappedFullTypeName);
                    UnityEngine.Object.DestroyImmediate(mappings[i]);

                    newMapping.transform.SetParent(storageGO.transform, false);
                }
            }

            foreach(PersistentClassMapping newMapping in typeNameToMappings.Values)
            {
                newMapping.transform.SetParent(storageGO.transform, false);
            }

            EditorUtility.SetDirty(storageGO);
            PrefabUtility.SaveAsPrefabAsset(storageGO, storagePath);
            UnityEngine.Object.DestroyImmediate(storageGO);
        }

    }
}
