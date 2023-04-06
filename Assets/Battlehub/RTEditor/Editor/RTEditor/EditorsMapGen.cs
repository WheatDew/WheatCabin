#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Battlehub.RTEditor
{
    public static class EditorsMapGen
    {
        private static string ScriptsPath
        {
            get { return BHRoot.AssetsPath + @"/RTEditor_Data/Scripts/Editors"; }
        }

        private static string PrefabsPath
        {
            get { return BHRoot.AssetsPath + @"/RTEditor_Data/Prefabs/Editors"; }
        }

        public const string ScriptName = "EditorsMapCreator.cs";

        public static void Generate(EditorDescriptor[] descriptors, MaterialEditorDescriptor[] materialDescriptors)
        {
            Dictionary<GameObject, int> editors = CreateComponentEditorMap(descriptors);

            if (!Directory.Exists(Path.GetFullPath(PrefabsPath + "/Resources")))
            {
                Directory.CreateDirectory(Path.GetFullPath(PrefabsPath + "/Resources"));
            }

            GameObject go = new GameObject();
            EditorsMapStorage editorsMap = go.AddComponent<EditorsMapStorage>();
            editorsMap.Editors = editors.OrderBy(k => k.Value).Select(k => k.Key).ToArray();

            MaterialEditorDescriptor defaultDescriptor = materialDescriptors.Where(d => d.Shader == null).FirstOrDefault();
            if(defaultDescriptor != null)
            {
                editorsMap.DefaultMaterialEditor = defaultDescriptor.Editor;
                editorsMap.IsDefaultMaterialEditorEnabled = defaultDescriptor.Enabled;
            }
            else
            {
                editorsMap.DefaultMaterialEditor = null;
                editorsMap.IsDefaultMaterialEditorEnabled = false;
            }

            materialDescriptors = materialDescriptors.Where(d => d != defaultDescriptor).ToArray();
            List<Shader> shaders = new List<Shader>();
            List<GameObject> materialEditors = new List<GameObject>();
            List<bool> materialEditorsEnabled = new List<bool>();
            for(int i = 0; i < materialDescriptors.Length; ++i)
            {
                MaterialEditorDescriptor descriptor = materialDescriptors[i];
                if(descriptor.Editor != null)
                {
                    shaders.Add(descriptor.Shader);
                    materialEditors.Add(descriptor.Editor);
                    materialEditorsEnabled.Add(descriptor.Enabled);
                }
            }

            editorsMap.Shaders = shaders.ToArray();
            editorsMap.MaterialEditors = materialEditors.ToArray();
            editorsMap.IsMaterialEditorEnabled = materialEditorsEnabled.ToArray();

            string path = PrefabsPath + "/Resources/" + EditorsMapStorage.EditorsMapPrefabName + ".prefab";
            PrefabUtility.SaveAsPrefabAsset(go, path);
            UnityEngine.Object.DestroyImmediate(go);

            AssetDatabase.ImportAsset(ScriptsPath, ImportAssetOptions.ImportRecursive);
        }

        private static Dictionary<GameObject, int> CreateComponentEditorMap(EditorDescriptor[] descriptors)
        {
            Type type = typeof(EditorsMap);

            string fullPath = Path.GetFullPath(ScriptsPath);
            Directory.CreateDirectory(fullPath);

            
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("using Battlehub.RTCommon;");
            builder.AppendLine("namespace " + type.Namespace);
            builder.AppendLine("{");
            builder.AppendLine("\tpublic class EditorsMapCreator : IEditorsMapCreator");
            builder.AppendLine("\t{");
            builder.AppendLine("\t\t#if UNITY_EDITOR");
            builder.AppendLine("\t\t[UnityEditor.InitializeOnLoadMethod]");
            builder.AppendLine("\t\t#endif");
            builder.AppendLine("\t\t[UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]");
            builder.AppendLine("\t\tstatic void Register()");
            builder.AppendLine("\t\t{");
            builder.AppendLine("\t\t\tIOC.UnregisterFallback<IEditorsMapCreator>();");
            builder.AppendLine("\t\t\tIOC.RegisterFallback<IEditorsMapCreator>(() => new EditorsMapCreator());");
            builder.AppendLine("\t\t}");
            builder.AppendLine("\t\t");
            builder.AppendLine("\t\tvoid IEditorsMapCreator.Create(IEditorsMap map)");
            builder.AppendLine("\t\t{");
            Dictionary<GameObject, int> editors = new Dictionary<GameObject, int>();
            int editorIndex = -1;
            for (int i = 0; i < descriptors.Length; ++i)
            {
                EditorDescriptor descriptor = descriptors[i];
                if (descriptor.Editor == null)
                {
                    continue;
                }

                string fullTypeName = descriptor.Type.FullName;
                fullTypeName = fullTypeName.Replace("Battlehub.RTEditor.", "");
                fullTypeName = fullTypeName.Replace("Battlehub.", "");
                if (editors.ContainsKey(descriptor.Editor))
                {
                    builder.AppendLine(
                        string.Format(
                            "\t\t\tmap.AddMapping(typeof({0}), {1}, {2}, {3});",
                            fullTypeName.Replace("`1", "<>"),
                            editors[descriptor.Editor],
                            descriptor.Enabled ? "true" : "false",
                            descriptor.IsPropertyEditor ? "true" : "false"));
                }
                else
                {
                    editorIndex++;
                    editors.Add(descriptor.Editor, editorIndex);

                    builder.AppendLine(
                        string.Format(
                            "\t\t\tmap.AddMapping(typeof({0}), {1}, {2}, {3});",
                            fullTypeName.Replace("`1", "<>"),
                            editorIndex,
                            descriptor.Enabled ? "true" : "false",
                            descriptor.IsPropertyEditor ? "true" : "false"));
                }               
            }

            builder.AppendLine("\t\t}");
            builder.AppendLine("\t}");
            builder.AppendLine("}");

            //RTE 2.1 version files removal
            File.Delete(Path.Combine(fullPath, "EditorsMapAuto.cs"));
            File.Delete(Path.Combine(fullPath, "EditorsMapAuto.cs.meta"));

            string content = builder.ToString();
            File.WriteAllText(Path.Combine(fullPath, ScriptName), content);
            return editors;
        }
    }
}
#endif
