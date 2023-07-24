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
    using UnityEngine;

    /// <summary>
    /// Implements the DataMapWindow for displaying the object identifier IDs and names.
    /// </summary>
    public class ObjectIdentifierWindow : UltimateCharacterController.Editor.Utility.DataMapWindow<NameIDMap, NameIDMapInspector>
    {
        protected override string HeaderLabel => "Object Identifiers";
        protected override string DefaultFileGUID => c_DefaultFileGUID;
        protected override string DefaultFileKey => c_DefaultFileKey;

        public const string c_DefaultFileGUID = "eb1dd8af4cb235844b382a80bec47c0f";
        public const string c_DefaultFileKey = "Opsive.UltimateCharacterController.Editor.Utility.ObjectIdentifierMappings";

        /// <summary>
        /// Static method allowing the window to be created.
        /// </summary>
        [MenuItem("Tools/Opsive/Ultimate Character Controller/Utility/Object Identifiers", false, 101)]
        public static void ShowNewWindow()
        {
            EditorWindow.GetWindow<ObjectIdentifierWindow>("Object Identifiers");
        }
    }

    /// <summary>
    /// Implements the DataMapSearchableWindow for the NameIDMap type.
    /// </summary>
    public class ObjectIdentifierSearchableWindow : DataMapSearchableWindow<NameIDMap, NameID, ObjectIdentifierWindow>
    {
        /// <summary>
        /// Five parameter constructor.
        /// </summary>
        /// <param name="title">The title of the window.</param>
        /// <param name="dataMap">The data source.</param>
        /// <param name="currentName">The current name of the object.</param>
        /// <param name="action">Action to perform when a value is selected.</param>
        /// <param name="closeOnActionComplete">Should the window be closed when the action is performed?</param>
        public ObjectIdentifierSearchableWindow(string title, NameIDMap dataMap, string currentName, Action<NameID> action, bool closeOnActionComplete) :
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
        public static void OpenWindow(string title, string currentName, Action<NameID> action, bool closeOnActionComplete)
        {
            var dataMap = GetNameIDMap();
            if (dataMap == null) {
                Debug.LogError($"Error: Unable to find the file with GUID {EditorPrefs.GetString(ObjectIdentifierWindow.c_DefaultFileKey, ObjectIdentifierWindow.c_DefaultFileGUID)}.");
                return;
            }

            var searchableWindow = new ObjectIdentifierSearchableWindow(title, dataMap, currentName, action, closeOnActionComplete);
            searchableWindow.OpenPopupWindow();
        }

        /// <summary>
        /// Returns the default NameIDMap.
        /// </summary>
        /// <returns>The default NameIDMap.</returns>
        public static NameIDMap GetNameIDMap()
        {
            var guid = EditorPrefs.GetString(ObjectIdentifierWindow.c_DefaultFileKey, ObjectIdentifierWindow.c_DefaultFileGUID);
            return AssetDatabase.LoadAssetAtPath<NameIDMap>(AssetDatabase.GUIDToAssetPath(guid));
        }
    }
}