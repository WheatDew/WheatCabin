using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Battlehub.RTSL
{
    [InitializeOnLoad]
    public class ConfigWindow : EditorWindow
    {
        private static bool AllowAutoShow
        {
            get { return EditorPrefs.GetBool("RTSL_ConfigAllowAutoOpen", true); }
            set { EditorPrefs.SetBool("RTSL_ConfigAllowAutoOpen", value); }
        }

        [MenuItem("Tools/Runtime SaveLoad/Config")]
        public static void ShowWindow()
        {
            ConfigWindow prevWindow = GetWindow<ConfigWindow>();
            if (prevWindow != null)
            {
                prevWindow.Close();
            }

            ConfigWindow window = CreateInstance<ConfigWindow>();
            window.titleContent = new GUIContent("RT Save & Load Config");
            window.Show();
            window.position = new Rect(20, 40, 380, 105);
        }

        static ConfigWindow()
        {
            EditorApplication.update += OnFirstUpdate;
        }

        private static void OnFirstUpdate()
        {
            EditorApplication.update -= OnFirstUpdate;
            if (!AllowAutoShow)
            {
                return;
            }

            bool typeModelExists = !string.IsNullOrEmpty(AssetDatabase.FindAssets(RTSLPath.TypeModel).FirstOrDefault());
            bool saveLoadDataFolderExists = AssetDatabase.IsValidFolder(RTSLPath.UserRoot);
            if (!typeModelExists || !saveLoadDataFolderExists)
            {
                ShowWindow();
            }
        }

        private bool m_doNotShowItAgain;
        private string m_path;
        private void OnEnable()
        {
            m_doNotShowItAgain = !AllowAutoShow;
            m_path = RTSLPath.UserRoot;
            //EditorPrefs.DeleteAll();
        }

        private void OnGUI()
        {
            EditorGUILayout.Separator();

            EditorGUILayout.LabelField("Runtime Save & Load configuration:");

            EditorGUILayout.Separator();

            //m_path = EditorGUILayout.TextField("Data Path:", m_path);
            EditorGUILayout.LabelField("Data Path: " + m_path);
            if (m_path.EndsWith("/") || m_path.EndsWith("\\") || m_path.StartsWith("/") || m_path.StartsWith("\\"))
            {
                m_path = m_path.Trim(new[] { '/', '\\' });
            }

            EditorGUILayout.Separator();

            EditorGUI.BeginChangeCheck();

            m_doNotShowItAgain = GUILayout.Toggle(m_doNotShowItAgain, "Do not show this window again");

            if (EditorGUI.EndChangeCheck())
            {
                AllowAutoShow = !m_doNotShowItAgain;
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Build All"))
            {
                if (RTSLPath.UserRoot != m_path && Directory.Exists(Path.GetFullPath(m_path)))
                {
                    EditorUtility.DisplayDialog("Directory already exists", "Unable to copy files. Directory " + Path.GetFullPath(m_path) + " already exists", "OK");
                    RTSLPath.UserRoot = m_path;
                }
                else
                {
                    if (Directory.Exists(Path.GetFullPath(RTSLPath.UserRoot)))
                    {
                        AssetDatabase.MoveAsset(RTSLPath.UserRoot, m_path);
                    }
                    RTSLPath.UserRoot = m_path;
                    Menu.BuildAll();
                }
            }

            if (GUILayout.Button("Cancel"))
            {
                Close();
            }

            EditorGUILayout.EndHorizontal();
        }
    }
}



