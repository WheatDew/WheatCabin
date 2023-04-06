using Battlehub.RTCommon;
using UnityEngine;

namespace Battlehub.RTEditor
{
   
    [DefaultExecutionOrder(-100)]
    public class BuiltInWindows : MonoBehaviour
    {
        [SerializeField]
        private CustomWindowDescriptor[] m_windows = null;
        private void Awake()
        {
            IRTEAppearance appearance = GetComponent<IRTEAppearance>();
            WindowManager wm = GetComponent<WindowManager>();
            if(wm.UseLegacyBuiltInWindows)
            {
                return;
            }

            for(int i = 0; i < m_windows.Length; ++i)
            {
                CustomWindowDescriptor desc = m_windows[i];
                if(desc == null)
                {
                    Debug.LogWarning($"CustomWindowDescriptor is null. Index: {i}");
                    continue;
                }

                if(!wm.RegisterWindow(desc))
                {
                    Debug.LogWarning($"Window of type {desc.TypeName} already registered");
                }

                if (desc.Descriptor.ContentPrefab != null)
                {
                    appearance.RegisterPrefab(desc.Descriptor.ContentPrefab);
                }
            }
        }
    }

    public static class BuiltInWindowNames
    {
        public readonly static string Game = RuntimeWindowType.Game.ToString().ToLower();
        public readonly static string Scene = RuntimeWindowType.Scene.ToString().ToLower();
        public readonly static string Hierarchy = RuntimeWindowType.Hierarchy.ToString().ToLower();
        public readonly static string Project = RuntimeWindowType.Project.ToString().ToLower();
        public readonly static string ProjectTree = RuntimeWindowType.ProjectTree.ToString().ToLower();
        public readonly static string ProjectFolder = RuntimeWindowType.ProjectFolder.ToString().ToLower();
        public readonly static string Inspector = RuntimeWindowType.Inspector.ToString().ToLower();
        public readonly static string Console = RuntimeWindowType.Console.ToString().ToLower();
        public readonly static string Animation = RuntimeWindowType.Animation.ToString().ToLower();

        public readonly static string ToolsPanel = RuntimeWindowType.ToolsPanel.ToString().ToLower();

        public readonly static string ImportFile = RuntimeWindowType.ImportFile.ToString().ToLower();
        public readonly static string OpenProject = RuntimeWindowType.OpenProject.ToString().ToLower();
        public readonly static string SelectAssetLibrary = RuntimeWindowType.SelectAssetLibrary.ToString().ToLower();
        public readonly static string ImportAssets = RuntimeWindowType.ImportAssets.ToString().ToLower();
        public readonly static string SaveScene = RuntimeWindowType.SaveScene.ToString().ToLower();
        public readonly static string About = RuntimeWindowType.About.ToString().ToLower();
        public readonly static string SaveAsset = RuntimeWindowType.SaveAsset.ToString().ToLower();
        public readonly static string SaveFile = RuntimeWindowType.SaveFile.ToString().ToLower();
        public readonly static string OpenFile = RuntimeWindowType.OpenFile.ToString().ToLower();

        public readonly static string SelectObject = RuntimeWindowType.SelectObject.ToString().ToLower();
        public readonly static string SelectColor = RuntimeWindowType.SelectColor.ToString().ToLower();
        public readonly static string SelectAnimationProperties = RuntimeWindowType.SelectAnimationProperties.ToString().ToLower();

        public readonly static string Settings = RuntimeWindowType.Settings.ToString().ToLower();
    }

}

