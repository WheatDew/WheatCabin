using Battlehub.RTSL;
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Battlehub.RTSL
{
}
public class PatchUtility : MonoBehaviour
{
    /// <summary>
    /// This method should be used make persistent classes to be compatible with saved files after CodeGen.cs fix from 10/23/2019.
    /// </summary>
    public static void PatchPersistentClassMappings()
    {
        //2.05 -> 2.10
        //UpdateTags(RTSLPath.ClassMappingsTemplatePath[0]);
        UpdateTags(RTSLPath.ClassMappingsStoragePath);

        //UpdateTags(RTSLPath.SurrogatesMappingsTemplatePath[0]);
        UpdateTags(RTSLPath.SurrogatesMappingsStoragePath);

        //2.10 -> 2.20
        //foreach (string path in RTSLPath.ClassMappingsTemplatePath)
        //{
        //    UpdateMappedAssemblyNames(path);
        //}
        UpdateMappedAssemblyNames(RTSLPath.ClassMappingsStoragePath);
        //foreach (string path in RTSLPath.SurrogatesMappingsTemplatePath)
        //{
        //    UpdateMappedAssemblyNames(path);
        //}
        UpdateMappedAssemblyNames(RTSLPath.SurrogatesMappingsStoragePath);

        //2.20 -> 2.21
        //foreach (string path in RTSLPath.ClassMappingsTemplatePath)
        //{
        //    UpdateMappedAssemblyNames2(path);
        //}
        UpdateMappedAssemblyNames2(RTSLPath.ClassMappingsStoragePath);
        //foreach (string path in RTSLPath.SurrogatesMappingsTemplatePath)
        //{
        //    UpdateMappedAssemblyNames2(path);
        //}
        UpdateMappedAssemblyNames2(RTSLPath.SurrogatesMappingsStoragePath);

        //2.21 -> 2.26
        //foreach (string path in RTSLPath.ClassMappingsTemplatePath)
        //{
        //    UpdateMappedAssemblyNames3(path);
        //}
        UpdateMappedAssemblyNames3(RTSLPath.ClassMappingsStoragePath);
        //foreach (string path in RTSLPath.SurrogatesMappingsTemplatePath)
        //{
        //    UpdateMappedAssemblyNames3(path);
        //}
        UpdateMappedAssemblyNames3(RTSLPath.SurrogatesMappingsStoragePath);
    }

    private static bool IsUpdateRequired(GameObject storageGo, int patchCounter)
    {
        PersistentClassMappingsStorage storage = storageGo.GetComponent<PersistentClassMappingsStorage>();
        if (storage == null)
        {
            return false;
        }
        return storage.PatchCounter < patchCounter;
    }

    //2.05 -> 2.1
    private static void UpdateTags(string storagePath)
    {
        GameObject storageGO = (GameObject)AssetDatabase.LoadAssetAtPath(storagePath, typeof(GameObject));
        if (storageGO == null)
        {
            return;
        }

        if (!IsUpdateRequired(storageGO, 1))
        {
            return;
        }

        storageGO = Instantiate(storageGO);
        PersistentClassMappingsStorage storage = storageGO.GetComponent<PersistentClassMappingsStorage>();
        if (storage == null)
        {
            storage = storageGO.AddComponent<PersistentClassMappingsStorage>();
        }
        storage.Version = new Version(2, 1).ToString();
        storage.PatchCounter = 1;

        PersistentClassMapping[] mappings = storageGO.GetComponentsInChildren<PersistentClassMapping>(true);
        for (int i = 0; i < mappings.Length; ++i)
        {
            PersistentClassMapping mapping = mappings[i];
            PersistentPropertyMapping[] properties = mapping.PropertyMappings;
            for (int j = 0; j < properties.Length; ++j)
            {
                PersistentPropertyMapping property = properties[j];
                property.PersistentTag = j + 1;
                mapping.PersistentPropertyTag = j + 1;
            }
        }

        EditorUtility.SetDirty(storageGO);
        PrefabUtility.SaveAsPrefabAsset(storageGO, storagePath);
        DestroyImmediate(storageGO);
    }

    //2.1.1 -> 2.2
    private static void UpdateMappedAssemblyNames(string storagePath)
    {
        GameObject storageGO = (GameObject)AssetDatabase.LoadAssetAtPath(storagePath, typeof(GameObject));
        if (storageGO == null)
        {
            return;
        }

        if (!IsUpdateRequired(storageGO, 2))
        {
            return;
        }

        storageGO = Instantiate(storageGO);
        PersistentClassMappingsStorage storage = storageGO.GetComponent<PersistentClassMappingsStorage>();
        if (storage == null)
        {
            storage = storageGO.AddComponent<PersistentClassMappingsStorage>();
        }
        storage.Version = RTSLVersion.Version.ToString();
        storage.PatchCounter = 2;

        PersistentClassMapping[] mappings = storageGO.GetComponentsInChildren<PersistentClassMapping>(true);
        for (int i = 0; i < mappings.Length; ++i)
        {
            PersistentClassMapping mapping = mappings[i];
            mapping.Version = RTSLVersion.Version.ToString();
            mapping.MappedTypeName = FixTypeName(mapping.MappedTypeName);
            mapping.MappedAssemblyName = FixAssemblyName(mapping.MappedAssemblyQualifiedName, mapping.MappedNamespace, mapping.MappedAssemblyName);

            PersistentPropertyMapping[] properties = mapping.PropertyMappings;
            for (int j = 0; j < properties.Length; ++j)
            {
                PersistentPropertyMapping property = properties[j];
                property.MappedTypeName = FixTypeName(property.MappedTypeName);
                property.MappedAssemblyName = FixAssemblyName(property.MappedAssemblyQualifiedName, property.MappedNamespace, property.MappedAssemblyName);
            }
        }

        EditorUtility.SetDirty(storageGO);
        PrefabUtility.SaveAsPrefabAsset(storageGO, storagePath);
        DestroyImmediate(storageGO);
    }

    private static string FixTypeName(string typeName)
    {
        if (typeName.Contains("Battlehub.RTEditor"))
        {
            return typeName.Replace(", Assembly-CSharp]", ", Battlehub.RTEditor]");
        }
        else if (typeName.Contains("Battlehub.UIControls"))
        {
            return typeName.Replace(", Assembly-CSharp]", ", Battlehub.UIControls]");
        }
        else if (typeName.Contains("Battlehub.Utils"))
        {
            return typeName.Replace(", Assembly-CSharp]", ", Battlehub.Utils]");
        }
        else if (typeName.Contains("Battlehub.RTCommon"))
        {
            return typeName.Replace(", Assembly-CSharp]", ", Battlehub.RTCommon]");
        }
        else if (typeName.Contains("Battlehub.RTSL.Interface"))
        {
            return typeName.Replace(", Assembly-CSharp]", ", Battlehub.RTSL.Interface]");
        }
        else if (typeName.Contains("Battlehub.RTSL.RuntimeShaderInfo"))
        {
            return typeName.Replace(", Assembly-CSharp]", ", Battlehub.RTSL.Interface]");
        }

        return typeName;
    }

    private static string FixAssemblyName(string assemblyQualifiedName, string ns, string mappedAssemblyName)
    {
        if (!ns.Contains("Battlehub") || ns.Contains("ProBuilderIntegration") || ns.Contains("Battlehub.RTSaveLoad"))
        {
            return mappedAssemblyName;
        }

        if (ns.Contains("Battlehub.Spline3"))
        {
            return "Battlehub.RTDeformer";
        }

        if (ns.Contains("Battlehub.RTSL"))
        {
            return "Battlehub.RTSL.Interface";
        }

        if (ns.Contains("Battlehub.Cubeman"))
        {
            return "Battlehub.RTDemoGame";
        }

        if (assemblyQualifiedName.Contains("Battlehub.Utils.ObjectToTexture"))
        {
            return "Battlehub.ObjectToTexture";
        }

        if (assemblyQualifiedName.Contains("ColorPicker,Assembly-CSharp"))
        {
            return "HSVPicker";
        }

        if (assemblyQualifiedName.Contains("Battlehub.Utils.ObjectToTexture,Assembly-CSharp"))
        {
            return "Battlehub.Utils.ObjectToTexture,Battlehub.Tools";
        }

        return string.Join(".", ns.Split('.').Take(2));
    }

    //2.2 -> 2.2.1
    private static void UpdateMappedAssemblyNames2(string storagePath)
    {
        GameObject storageGO = (GameObject)AssetDatabase.LoadAssetAtPath(storagePath, typeof(GameObject));
        if (storageGO == null)
        {
            return;
        }

        if (!IsUpdateRequired(storageGO, 3))
        {
            return;
        }

        storageGO = Instantiate(storageGO);
        PersistentClassMappingsStorage storage = storageGO.GetComponent<PersistentClassMappingsStorage>();
        if (storage == null)
        {
            storage = storageGO.AddComponent<PersistentClassMappingsStorage>();
        }
        storage.Version = RTSLVersion.Version.ToString();
        storage.PatchCounter = 3;

        PersistentClassMapping[] mappings = storageGO.GetComponentsInChildren<PersistentClassMapping>(true);
        for (int i = 0; i < mappings.Length; ++i)
        {
            PersistentClassMapping mapping = mappings[i];
            mapping.Version = RTSLVersion.Version.ToString();
            mapping.MappedTypeName = FixTypeName2(mapping.MappedTypeName);
            mapping.MappedAssemblyName = FixAssemblyName2(mapping.MappedAssemblyQualifiedName, mapping.MappedNamespace, mapping.MappedAssemblyName);

            PersistentPropertyMapping[] properties = mapping.PropertyMappings;
            for (int j = 0; j < properties.Length; ++j)
            {
                PersistentPropertyMapping property = properties[j];
                property.MappedTypeName = FixTypeName2(property.MappedTypeName);
                property.MappedAssemblyName = FixAssemblyName2(property.MappedAssemblyQualifiedName, property.MappedNamespace, property.MappedAssemblyName);
            }
        }

        EditorUtility.SetDirty(storageGO);
        PrefabUtility.SaveAsPrefabAsset(storageGO, storagePath);
        DestroyImmediate(storageGO);
    }

    private static string FixTypeName2(string typeName)
    {
        if (typeName.Contains("Battlehub.RTEditor"))
        {
            return typeName.Replace(", Assembly-CSharp]", ", Battlehub.RTEditor]");
        }
        else if (typeName.Contains("Battlehub.UIControls"))
        {
            return typeName.Replace(", Battlehub.UIControls]", ",  Battlehub.RTEditor]");
        }
        else if (typeName.Contains("Battlehub.Utils"))
        {
            return typeName.Replace(", Battlehub.Utils]", ", Battlehub.RTEditor]");
        }
        else if (typeName.Contains("Battlehub.RTCommon"))
        {
            return typeName.Replace(", Battlehub.RTCommon]", ", Battlehub.RTEditor]");
        }
        else if (typeName.Contains("Battlehub.RTSL.Interface"))
        {
            return typeName.Replace(", Battlehub.RTSL.Interface]", ", Battlehub.RTEditor]");
        }
        else if (typeName.Contains("Battlehub.RTSL.RuntimeShaderInfo"))
        {
            return typeName.Replace(", Battlehub.RTSL.Interface]", ", Battlehub.RTEditor]");
        }

        return typeName;
    }

    private static string FixAssemblyName2(string assemblyQualifiedName, string ns, string mappedAssemblyName)
    {
        if (!ns.Contains("Battlehub") || ns.Contains("Battlehub.RTSaveLoad") || ns.Contains("Battlehub.Cubeman"))
        {
            return mappedAssemblyName;
        }

        return "Battlehub.RTEditor";
    }

    public static void UpdateMappedAssemblyNames3(string storagePath)
    {
        GameObject storageGO = (GameObject)AssetDatabase.LoadAssetAtPath(storagePath, typeof(GameObject));
        if (storageGO == null)
        {
            return;
        }

        if (!IsUpdateRequired(storageGO, 4))
        {
            return;
        }

        storageGO = Instantiate(storageGO);
        PersistentClassMappingsStorage storage = storageGO.GetComponent<PersistentClassMappingsStorage>();
        if (storage == null)
        {
            storage = storageGO.AddComponent<PersistentClassMappingsStorage>();
        }
        storage.Version = RTSLVersion.Version.ToString();
        storage.PatchCounter = 4;

        PersistentClassMapping[] mappings = storageGO.GetComponentsInChildren<PersistentClassMapping>(true);
        for (int i = 0; i < mappings.Length; ++i)
        {
            PersistentClassMapping mapping = mappings[i];
            mapping.Version = RTSLVersion.Version.ToString();
            mapping.MappedAssemblyName = FixAssemblyName3(mapping.MappedNamespace, mapping.PersistentFullTypeName, mapping.MappedAssemblyName);

            PersistentPropertyMapping[] properties = mapping.PropertyMappings;
            for (int j = 0; j < properties.Length; ++j)
            {
                PersistentPropertyMapping property = properties[j];
                property.MappedAssemblyName = FixAssemblyName3(property.MappedNamespace, property.PersistentFullTypeName, property.MappedAssemblyName);
            }
        }

        EditorUtility.SetDirty(storageGO);
        PrefabUtility.SaveAsPrefabAsset(storageGO, storagePath);
        DestroyImmediate(storageGO);
    }

    private static string FixAssemblyName3(string ns, string persistentFullTypeName, string mappedAssemblyName)
    {
        if(ns.Contains("Battlehub.Cubeman"))
        {
            return "Battlehub.RTEditor.Demo";
        }

        if(persistentFullTypeName == "Battlehub.RTBuilder.Battlehub.SL2.PersistentMaterialPalette" ||
           persistentFullTypeName == "Battlehub.ProBuilderIntegration.Battlehub.SL2.PersistentPBMesh" ||
           persistentFullTypeName == "Battlehub.ProBuilderIntegration.Battlehub.SL2.PersistentPBPolyShape" ||
           persistentFullTypeName == "Battlehub.ProBuilderIntegration.Battlehub.SL2.PersistentPBAutoUnwrapSettings" ||
           persistentFullTypeName == "Battlehub.ProBuilderIntegration.Battlehub.SL2.PersistentPBFace" ||
           persistentFullTypeName == "Battlehub.RTTerrain.Battlehub.SL2.PersistentTerrainBrushSource" ||
           persistentFullTypeName == "Battlehub.RTTerrain.Battlehub.SL2.PersistentTerrainToolState")
        {
            return "Battlehub.RTExtensions";
        }

        return mappedAssemblyName;
    }
}
