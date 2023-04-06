using System;
using UnityEditor;
using UnityEngine;

namespace Battlehub.RTSL
{
    [CustomEditor(typeof(AssetLibraryAsset))]
    public class AssetLibraryEditor : Editor
    {
        private AssetLibraryProjectGUI m_projectGUI;
        private AssetLibraryAssetsGUI m_assetsGUI;
        private AssetLibraryAsset Asset
        {
            get { return (AssetLibraryAsset)target; }
        }

        private bool m_isSyncRequired;
        private void OnEnable()
        {
            if (m_assetsGUI == null)
            {
                m_assetsGUI = new AssetLibraryAssetsGUI();
                m_assetsGUI.SetTreeAsset(Asset);      
            }

            if (m_projectGUI == null)
            {
                m_projectGUI = new AssetLibraryProjectGUI(m_assetsGUI);
                m_projectGUI.SetTreeAsset(Asset);
                m_projectGUI.SelectedFoldersChanged += OnSelectedFoldersChanged;

                m_isSyncRequired = Asset.IsSyncRequired();
            }

            m_assetsGUI.SetSelectedFolders(m_projectGUI.SelectedFolders);
            m_projectGUI.OnEnable();
            m_assetsGUI.OnEnable();

            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

   
        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;

            if (Asset != null)
            {
                if(m_lastPlayModeStateChange != PlayModeStateChange.ExitingEditMode && m_lastPlayModeStateChange != PlayModeStateChange.ExitingPlayMode)
                {
                    SaveAsset();
                }
            }

            m_projectGUI.OnDisable();
            m_assetsGUI.OnDisable();
        }

        private PlayModeStateChange m_lastPlayModeStateChange = PlayModeStateChange.EnteredEditMode;
        private void OnPlayModeStateChanged(PlayModeStateChange change)
        {
            m_lastPlayModeStateChange = change;
        }

        private void SaveAsset()
        {
            EditorUtility.SetDirty(Asset);
            AssetDatabase.SaveAssets();
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.BeginVertical();

            bool click = false;

            if (m_isSyncRequired)
            {
                EditorGUILayout.HelpBox("One or more prefabs have been changed. AssetLibrary need to be synchronized.", MessageType.Warning);
                click = GUILayout.Button("Synchronize");
            }


            m_projectGUI.OnGUI();
            EditorGUILayout.Space();
            m_assetsGUI.OnGUI();
            EditorGUILayout.Space();

            if (click)
            {
                Asset.Sync();
                m_assetsGUI = new AssetLibraryAssetsGUI();
                m_assetsGUI.InitIfNeeded();
                m_assetsGUI.SetSelectedFolders(m_projectGUI.SelectedFolders);
                m_assetsGUI.OnEnable();
                m_isSyncRequired = false;
                SaveAsset();
            }

            EditorGUILayout.EndVertical();


        }

        private void OnSelectedFoldersChanged(object sender, EventArgs e)
        {
            m_assetsGUI.SetSelectedFolders(m_projectGUI.SelectedFolders);
        }
       
    }
}
