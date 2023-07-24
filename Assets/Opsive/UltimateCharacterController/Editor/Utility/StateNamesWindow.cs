/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Utility
{
    using Opsive.UltimateCharacterController.Utility;
    using System;
    using UnityEditor;
    using UnityEditor.UIElements;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// Implements the DataMapWindow for displaying the state names.
    /// </summary>
    public class StateNamesWindow : DataMapWindow<NameMap, NameMapInspector>
    {
        protected override string HeaderLabel => "States";
        protected override string DefaultFileGUID => c_DefaultFileGUID;
        protected override string DefaultFileKey => c_DefaultFileKey;

        public const string c_DefaultFileGUID = "a9abf18b46876c44c88629bdd773ca5d";
        public const string c_DefaultFileKey = "Opsive.UltimateCharacterController.Editor.Utility.StateNameMappings";

        /// <summary>
        /// Static method allowing the window to be created.
        /// </summary>
        [MenuItem("Tools/Opsive/Ultimate Character Controller/Utility/State Names", false, 100)]
        public static void ShowNewWindow()
        {
            EditorWindow.GetWindow<StateNamesWindow>("States");
        }
    }

    /// <summary>
    /// Implements the DataMapSearchableWindow for the NameMap type.
    /// </summary>
    public class StateNamesSearchableWindow : DataMapSearchableWindow<NameMap, string, StateNamesWindow>
    {
        /// <summary>
        /// Five parameter constructor.
        /// </summary>
        /// <param name="title">The title of the window.</param>
        /// <param name="dataMap">The data source.</param>
        /// <param name="currentName">The current name of the object.</param>
        /// <param name="action">Action to perform when a value is selected.</param>
        /// <param name="closeOnActionComplete">Should the window be closed when the action is performed?</param>
        public StateNamesSearchableWindow(string title, NameMap dataMap, string currentName, Action<string> action, bool closeOnActionComplete) :
                                                base(title, dataMap, currentName, action, closeOnActionComplete)
        {
        }

        /// <summary>
        /// Opens a new StateNamesWindow.
        /// </summary>
        /// <param name="title">The title of the window.</param>
        /// <param name="currentName">The current name of the object.</param>
        /// <param name="action">Action to perform when a value is selected.</param>
        /// <param name="closeOnActionComplete">Should the window be closed when the action is performed?</param>
        public static void OpenWindow(string title, string currentName, Action<string> action, bool closeOnActionComplete)
        {
            var guid = EditorPrefs.GetString(StateNamesWindow.c_DefaultFileKey, StateNamesWindow.c_DefaultFileGUID);
            var dataMap = AssetDatabase.LoadAssetAtPath<NameMap>(AssetDatabase.GUIDToAssetPath(guid));
            if (dataMap == null) {
                Debug.LogError($"Error: Unable to find the file with GUID {guid}.");
                return;
            }

            var searchableWindow = new StateNamesSearchableWindow(title, dataMap, currentName, action, closeOnActionComplete);
            searchableWindow.OpenPopupWindow();
        }
    }
}