#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine.EventSystems;
using System.IO;
using System.Collections.Generic;
using UnityObject = UnityEngine.Object;
using Battlehub.RTSL;
using Battlehub.RTCommon;
using System.Linq;
using UnityEditor.Callbacks;
using Battlehub.RTEditor.Binding;
using Battlehub.UIControls.Binding;
using Battlehub.RTEditor.Views;
using Battlehub.RTEditor.ViewModels;
using Battlehub.Utils;

namespace Battlehub.RTEditor
{
    public static class RTEditorMenu
    {           
        [MenuItem("Tools/Runtime Editor/Create RTEditor", priority = 0)]
        public static void CreateRuntimeEditor()
        {
            GameObject editor = GameObject.Find("RTEditor");
            if (editor != null)
            {
                Selection.activeGameObject = editor;
                EditorGUIUtility.PingObject(editor);
            }
            else
            {
                editor = InstantiateRuntimeEditor();
                editor.name = "RTEditor";
                Undo.RegisterCreatedObjectUndo(editor, "Battlehub.RTEditor.Create");
            }

            EventSystem eventSystem = UnityObject.FindObjectOfType<EventSystem>();
            if (!eventSystem)
            {
                GameObject es = new GameObject();
                eventSystem = es.AddComponent<EventSystem>();
                es.AddComponent<RTEStandaloneInputModule>();
                es.name = "EventSystem";

                Undo.RegisterCreatedObjectUndo(es, "Battlehub.RTEditor.CreateEventSystem");
            }

            eventSystem.gameObject.AddComponent<RTSLIgnore>();

            GameObject camera = GameObject.Find("Main Camera");
            if (camera != null)
            {
                if (camera.GetComponent<GameViewCamera>() == null)
                {
                    if (EditorUtility.DisplayDialog("Main Camera setup.", "Do you want to add Game View Camera script to Main Camera and render it to Runtime Editors's Game view?", "Yes", "No"))
                    {
                        Undo.AddComponent<GameViewCamera>(camera.gameObject);
                    }
                }
            }
        }

        public static GameObject InstantiateRuntimeEditor()
        {
            UnityObject prefab = AssetDatabase.LoadAssetAtPath(BHRoot.PackageRuntimeContentPath + "/RTEditor/Prefabs/RuntimeEditor.prefab", typeof(GameObject));
            return (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        }

        [MenuItem("Tools/Runtime Editor/Add Mobile support for RTEditor", priority = 2)]
        public static void AddMobileSupport()
        {
            GameObject mobileSupport = GameObject.Find("RTEditor Mobile Support");
            if(mobileSupport != null)
            {
                Selection.activeGameObject = mobileSupport;
                EditorGUIUtility.PingObject(mobileSupport);
            }
            else
            {
                mobileSupport = InstantiateMobileSupport();
                mobileSupport.name = "RTEditor Mobile Support";
                Undo.RegisterCreatedObjectUndo(mobileSupport, "Battlehub.RTEditor.MobileSupport");
            }
           
            EventSystem eventSystem = UnityObject.FindObjectOfType<EventSystem>();
            if (eventSystem)
            {
                StandaloneInputModule standaloneInput = eventSystem.GetComponent<StandaloneInputModule>();
                if(standaloneInput != null && standaloneInput.GetType() == typeof(StandaloneInputModule))
                {
                    Undo.DestroyObjectImmediate(standaloneInput);
                    RTEStandaloneInputModule standaloneInputModule = Undo.AddComponent<RTEStandaloneInputModule>(eventSystem.gameObject);
                    standaloneInputModule.UseMouse = false;
                    Undo.RecordObject(standaloneInputModule, "Battlehub.RTEditor.MobileSupport");
                }
            }
        }

        public static GameObject InstantiateMobileSupport()
        {
            UnityObject prefab = AssetDatabase.LoadAssetAtPath(BHRoot.PackageRuntimeContentPath + "/RTEditor/Prefabs/Mobile/MobileSupport.prefab", typeof(GameObject));
            return (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        }

        public static string GetSelectedPathOrFallback()
        {
            string path = "Assets";

            foreach (UnityObject obj in Selection.GetFiltered(typeof(UnityObject), UnityEditor.SelectionMode.Assets))
            {
                path = AssetDatabase.GetAssetPath(obj);
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    path = Path.GetDirectoryName(path);
                    break;
                }
            }
            return path;
        }

        [MenuItem("Tools/Runtime Editor/Create Custom Window", priority = -100)]
        public static void CreateCustomWindowStep1()
        {
            string path = GetSelectedPathOrFallback();
            string selectedPath = EditorUtility.SaveFilePanelInProject("Create Custom Window", "CustomWindow", "prefab", "Create Prefab", path);
            if (string.IsNullOrEmpty(selectedPath))
            {
                return;
            }

            var ns = EditorInputDialog.Show("Namespace", "Please enter namespace", "");
            if(string.IsNullOrEmpty(ns))
            {
                ns = "Example";
            }
            
            string targetName = Path.GetFileNameWithoutExtension(selectedPath);
            if(targetName.EndsWith("Window"))
            {
                targetName = targetName.Substring(0, targetName.LastIndexOf("Window"));
            }
            string targertDir = Path.GetDirectoryName(selectedPath).Replace('\\', '/');
            string targetPath = Path.Combine(targertDir, targetName).Replace('\\', '/');
            string prefabPath = $"{targetPath}Window.prefab";

            AssetDatabase.CopyAsset(BHRoot.PackageRuntimeContentPath + "/RTEditor/Prefabs/Resources/TemplateWindow.prefab", prefabPath);
            Debug.Log($"Asset copy created {prefabPath}");

            string assetTemplate = BHRoot.PackageRuntimeContentPath + "/RTEditor/Prefabs/Resources/TemplateView.cs.txt";
            string csPath = $"{targetPath}View.cs";
            CreateScriptFromTemplate(csPath, assetTemplate, ("#NAMESPACE#", ns));

            assetTemplate = BHRoot.PackageRuntimeContentPath + "/RTEditor/Prefabs/Resources/TemplateViewModel.cs.txt";
            csPath = $"{targetPath}ViewModel.cs";
            CreateScriptFromTemplate(csPath, assetTemplate, ("#NAMESPACE#", ns));

            assetTemplate = BHRoot.PackageRuntimeContentPath + "/RTEditor/Prefabs/Resources/RegisterTemplateWindow.cs.txt";
            csPath = $"{targertDir}/Register{targetName}Window.cs";
            CreateScriptFromTemplate(csPath, assetTemplate, ("#WINDOWNAME#", targetName), ("#NAMESPACE#", ns));

            EditorPrefs.SetString("RTE_CustomWindowTargetPath", targetPath);
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
        }

        [DidReloadScripts]
        public static void CreateCustomWindowStep2()
        {
            string targetPath = EditorPrefs.GetString("RTE_CustomWindowTargetPath");
            if(string.IsNullOrEmpty(targetPath))
            {
                return;
            }
            EditorPrefs.SetString("RTE_CustomWindowTargetPath", null);

            string prefabPath = $"{targetPath}Window.prefab";
            GameObject prefab = PrefabUtility.LoadPrefabContents(prefabPath);

            string csPath = $"{targetPath}View.cs";
            MonoScript viewScript = (MonoScript)AssetDatabase.LoadAssetAtPath(csPath, typeof(MonoScript));
            prefab.AddComponent(viewScript.GetClass());

            csPath = $"{targetPath}ViewModel.cs";
            MonoScript viewModelScript = (MonoScript)AssetDatabase.LoadAssetAtPath(csPath, typeof(MonoScript));
            prefab.AddComponent(viewModelScript.GetClass());

            string viewTypeName = viewScript.GetClass().FullName;
            string viewModelTypeName = viewModelScript.GetClass().FullName;

            ViewBinding binding = prefab.AddComponent<ViewBinding>();
            binding.ViewModelDragObjectsPropertyName = $"{viewModelTypeName}.{nameof(ViewModel.ExternalDragObjects)}";
            binding.ViewModelCanDropObjectsPropertyName = $"{viewModelTypeName}.{nameof(ViewModel.CanDropExternalObjects)}";
            binding.m_eventBindings = new[]
            {
                new ControlBinding.EventBindingSlim($"{viewTypeName}.{nameof(View.Activated)}", $"{viewModelTypeName}.{nameof(ViewModel.OnActivated)}"),
                new ControlBinding.EventBindingSlim($"{viewTypeName}.{nameof(View.Deactivated)}", $"{viewModelTypeName}.{nameof(ViewModel.OnDeactivated)}"),
                new ControlBinding.EventBindingSlim($"{viewTypeName}.{nameof(View.DragEnter)}", $"{viewModelTypeName}.{nameof(ViewModel.OnExternalObjectEnter)}"),
                new ControlBinding.EventBindingSlim($"{viewTypeName}.{nameof(View.DragLeave)}", $"{viewModelTypeName}.{nameof(ViewModel.OnExternalObjectLeave)}"),
                new ControlBinding.EventBindingSlim($"{viewTypeName}.{nameof(View.Drag)}", $"{viewModelTypeName}.{nameof(ViewModel.OnExternalObjectDrag)}"),
                new ControlBinding.EventBindingSlim($"{viewTypeName}.{nameof(View.Drop)}", $"{viewModelTypeName}.{nameof(ViewModel.OnExternalObjectDrop)}"),
                new ControlBinding.EventBindingSlim($"{viewTypeName}.{nameof(View.SelectAll)}", $"{viewModelTypeName}.{nameof(ViewModel.OnSelectAll)}"),
                new ControlBinding.EventBindingSlim($"{viewTypeName}.{nameof(View.Duplicate)}", $"{viewModelTypeName}.{nameof(ViewModel.OnDuplicate)}"),
                new ControlBinding.EventBindingSlim($"{viewTypeName}.{nameof(View.Delete)}", $"{viewModelTypeName}.{nameof(ViewModel.OnDelete)}"),
            };

            
                
            PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath, out bool success);
            Debug.Log($"{prefabPath} ready");
            PrefabUtility.UnloadPrefabContents(prefab);
        }

        private static void CreateScriptFromTemplate(string path, string assetTemplate, params (string src, string dst)[] replacements)
        {
            if (AssetDatabase.CopyAsset(assetTemplate, path))
            {
                string scriptName = Path.GetFileNameWithoutExtension(path);
                string contents = File.ReadAllText(path);
                contents = contents.Replace("#SCRIPTNAME#", scriptName);
                if (replacements != null)
                {
                    foreach (var kvp in replacements)
                    {
                        contents = contents.Replace(kvp.src, kvp.dst);
                    }
                }

                File.WriteAllText(path, contents);
                Debug.Log($"File Created {path}");
            }
        }
    }
}
#endif