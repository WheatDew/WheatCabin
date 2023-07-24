/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Managers
{
    using System;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// Utility functions for the manager classes.
    /// </summary>
    public static class ManagerUtility
    {
        private static string s_AnimatorControllerGUID = "7a3c9dbb86c183646bfccf5b0a2f9dbc";
        private const string c_ItemCollectionGUID = "5481010ef14c32f4cb7b6661b0c59fb4";
        private const string c_InvisibleShadowCasterGUID = "0a580a5ea04fdab47941095489aa23b7";

        private const string c_LastItemCollectionGUIDString = "LastItemCollectionGUID";
        public static string LastItemCollectionGUIDString { get { return c_LastItemCollectionGUIDString; } }

        private const string c_ItemSetRuleGUID = "a26ce96f0eca43d4fbf76b9d2203da18";
        private const string c_LastItemSetRuleGUIDString = "LastItemSetRuleGUID";
        public static string LastItemSetRuleGUIDString { get { return c_LastItemSetRuleGUIDString; } }

        /// <summary>
        /// Creates a standard box that can be used to show menu content.
        /// </summary>
        /// <param name="title">The title of the box.</param>
        /// <param name="description">The description of the box.</param>
        /// <param name="options">Any additional options (can be null).</param>
        /// <param name="buttonTitle">The title of the action button.</param>
        /// <param name="buttonAction">The action of the button.</param>
        /// <param name="parent">The VisualElement that the box should be added to.</param>
        /// <param name="enableButton">Should the button be enabled?</param>
        /// <param name="topMargin">The top of the box margin.</param>
        /// <returns>The bottom action button (can be null).</returns>
        public static Button ShowControlBox(string title, string description, Action<VisualElement> options, string buttonTitle, Action buttonAction, VisualElement parent, bool enableButton, float topMargin = 20f)
        {
            var container = new VisualElement();
            container.AddToClassList("sub-menu");
            if (EditorGUIUtility.isProSkin) {
                container.AddToClassList("sub-menu-dark");
            } else {
                container.AddToClassList("sub-menu-light");
            }
            container.style.marginTop = topMargin;
            var titleLabel = new Label(title);
            titleLabel.AddToClassList("sub-menu-title");
            container.Add(titleLabel);
            var descriptionLabel = new Label();
            descriptionLabel.text = description;
            descriptionLabel.style.whiteSpace = WhiteSpace.Normal;
            container.Add(descriptionLabel);
            if (options != null) {
                var optionsContainer = new VisualElement();
                container.Add(optionsContainer);
                descriptionLabel.style.marginBottom = 4;
                options(optionsContainer);
            }

            Button button = null;
            if (buttonAction != null) {
                button = new Button();
                button.AddToClassList("sub-menu-button");
                button.style.marginTop = 4;
                button.text = buttonTitle;
                button.clicked += buttonAction;
                button.SetEnabled(enableButton);
                container.Add(button);
            }

            parent.Add(container);
            return button;
        }

        /// <summary>
        /// Draws a control box which allows for an action when the button is pressed.
        /// </summary>
        /// <param name="title">The title of the control box.</param>
        /// <param name="additionalControls">Any additional controls that should appear before the message.</param>
        /// <param name="message">The message within the box.</param>
        /// <param name="enableButton">Is the button enabled?</param>
        /// <param name="button">The name of the button.</param>
        /// <param name="action">The action that is performed when the button is pressed.</param>
        /// <param name="successLog">The message to output to the log upon success.</param>
        public static void DrawControlBox(string title, System.Action additionalControls, string message, bool enableButton, string button, System.Action action, string successLog)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label(title, Shared.Editor.Inspectors.Utility.InspectorStyles.BoldLabel);
            GUILayout.Space(4);
            GUILayout.Label(message, Shared.Editor.Inspectors.Utility.InspectorStyles.WordWrapLabel);
            if (additionalControls != null) {
                additionalControls();
            }
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUI.enabled = enableButton;
            if (!string.IsNullOrEmpty(button) && GUILayout.Button(button, GUILayout.Width(130))) {
                action();
                if (!string.IsNullOrEmpty(successLog)) {
                    Debug.Log(successLog);
                }
            }
            GUI.enabled = true;
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(4);
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Searches for the default animator controller.
        /// </summary>
        public static RuntimeAnimatorController FindAnimatorController(ScriptableObject editorWindow)
        {
            // The GUID should remain consistant.
            var animatorControllerPath = AssetDatabase.GUIDToAssetPath(s_AnimatorControllerGUID);
            if (!string.IsNullOrEmpty(animatorControllerPath)) {
                var animatorController = AssetDatabase.LoadAssetAtPath(animatorControllerPath, typeof(RuntimeAnimatorController)) as RuntimeAnimatorController;
                if (animatorController != null) {
                    return animatorController;
                }
            }

            // The animator controller doesn't have the expected guid. Try to find the asset based on the path.
            animatorControllerPath = System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(editorWindow))).Replace("\\", "/").Replace("Editor/Managers", "Demo/Animator/Characters/Demo.controller");
            if (System.IO.File.Exists(Application.dataPath + "/" + animatorControllerPath.Substring(7))) {
                return AssetDatabase.LoadAssetAtPath(animatorControllerPath, typeof(RuntimeAnimatorController)) as RuntimeAnimatorController;
            }

            return null;
        }

        /// <summary>
        /// Searches for the default item collection.
        /// </summary>
        public static Inventory.ItemCollection FindItemCollection(ScriptableObject editorWindow)
        {
            // Retrieve the last used ItemCollection.
            var lastItemCollectionGUID = EditorPrefs.GetString(LastItemCollectionGUIDString, string.Empty);
            if (!string.IsNullOrEmpty(lastItemCollectionGUID)) {
                var lastItemCollectionPath = AssetDatabase.GUIDToAssetPath(lastItemCollectionGUID);
                if (!string.IsNullOrEmpty(lastItemCollectionPath)) {
                    var itemCollection = AssetDatabase.LoadAssetAtPath(lastItemCollectionPath, typeof(Inventory.ItemCollection)) as Inventory.ItemCollection;
                    if (itemCollection != null) {
                        return itemCollection;
                    }
                }
            }

            // If an ItemCollection asset exists within the scene then use that.
            var itemSetManager = UnityEngine.Object.FindObjectOfType<Inventory.ItemSetManager>();
            if (itemSetManager != null) {
                if (itemSetManager.ItemCollection != null) {
                    return itemSetManager.ItemCollection;
                }
            }

            // The GUID should remain consistant.
            var itemCollectionPath = AssetDatabase.GUIDToAssetPath(c_ItemCollectionGUID);
            if (!string.IsNullOrEmpty(itemCollectionPath)) {
                var itemCollection = AssetDatabase.LoadAssetAtPath(itemCollectionPath, typeof(Inventory.ItemCollection)) as Inventory.ItemCollection;
                if (itemCollection != null) {
                    return itemCollection;
                }
            }

            // The item collection doesn't have the expected guid. Try to find the asset based on the path.
            itemCollectionPath = System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(editorWindow))).Replace("\\", "/").Replace("Editor/Managers", "Demo/Inventory/DemoItemCollection.asset");
            if (System.IO.File.Exists(Application.dataPath + "/" + itemCollectionPath.Substring(7))) {
                return AssetDatabase.LoadAssetAtPath(itemCollectionPath, typeof(Inventory.ItemCollection)) as Inventory.ItemCollection;
            }

            // Last chance: use resources to try to find the ItemCollection.
            var itemCollections = Resources.FindObjectsOfTypeAll<Inventory.ItemCollection>();
            if (itemCollections != null && itemCollections.Length > 0) {
                return itemCollections[0];
            }

            return null;
        }
        
         /// <summary>
        /// Searches for the default item set rule.
        /// </summary>
        public static Inventory.ItemSetRuleBase FindItemSetRule(ScriptableObject editorWindow)
        {
            // Retrieve the last used ItemSetRule.
            var lastItemSetRuleGUID = EditorPrefs.GetString(LastItemSetRuleGUIDString, string.Empty);
            if (!string.IsNullOrEmpty(lastItemSetRuleGUID)) {
                var lastItemSetRulePath = AssetDatabase.GUIDToAssetPath(lastItemSetRuleGUID);
                if (!string.IsNullOrEmpty(lastItemSetRulePath)) {
                    var itemSetRule = AssetDatabase.LoadAssetAtPath(lastItemSetRulePath, typeof(Inventory.ItemSetRuleBase)) as Inventory.ItemSetRuleBase;
                    if (itemSetRule != null) {
                        return itemSetRule;
                    }
                }
            }

            // If an ItemSetRule asset exists within the scene then use that.
            var itemSetManager = UnityEngine.Object.FindObjectOfType<Inventory.ItemSetManager>();
            if (itemSetManager != null) {
                if (itemSetManager.ItemSetGroups != null && itemSetManager.ItemSetGroups.Length != 0) {
                    if (itemSetManager.ItemSetGroups[0].StartingItemSetRules != null && itemSetManager.ItemSetGroups[0].StartingItemSetRules.Length != 0) {
                        if (itemSetManager.ItemSetGroups[0].StartingItemSetRules[0] != null) {
                            return itemSetManager.ItemSetGroups[0].StartingItemSetRules[0];
                        }
                    }
                }
            }

            // The GUID should remain consistant.
            var itemSetRulePath = AssetDatabase.GUIDToAssetPath(c_ItemSetRuleGUID);
            if (!string.IsNullOrEmpty(itemSetRulePath)) {
                var itemSetRule = AssetDatabase.LoadAssetAtPath(itemSetRulePath, typeof(Inventory.ItemSetRuleBase)) as Inventory.ItemSetRuleBase;
                if (itemSetRule != null) {
                    return itemSetRule;
                }
            }

            // The item set rule doesn't have the expected guid. Try to find the asset based on the path.
            if (editorWindow != null) {
                itemSetRulePath = System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(editorWindow))).Replace("\\", "/").Replace("Editor/Managers", "Demo/Inventory/DemoIndividualItemSetRule.asset");
                if (System.IO.File.Exists(Application.dataPath + "/" + itemSetRulePath.Substring(7))) {
                    return AssetDatabase.LoadAssetAtPath(itemSetRulePath, typeof(Inventory.ItemSetRuleBase)) as Inventory.ItemSetRuleBase;
                }
            }

            // Last chance: use resources to try to find the ItemSetRule.
            var itemSetRules = Resources.FindObjectsOfTypeAll<Inventory.ItemSetRuleBase>();
            if (itemSetRules != null && itemSetRules.Length > 0) {
                return itemSetRules[0];
            }

            return null;
        }

        /// <summary>
        /// Searches for the invisible shadow caster material.
        /// </summary>
        public static Material FindInvisibleShadowCaster(ScriptableObject editorWindow)
        {
            // The GUID should remain consistant. 
            var shadowCasterPath = AssetDatabase.GUIDToAssetPath(c_InvisibleShadowCasterGUID);
            if (!string.IsNullOrEmpty(shadowCasterPath)) {
                var invisibleShadowCaster = AssetDatabase.LoadAssetAtPath(shadowCasterPath, typeof(Material)) as Material;
                if (invisibleShadowCaster != null) {
                    return invisibleShadowCaster;
                }
            }

            if (editorWindow != null) {
                // The invisible shadow caster doesn't have the expected guid. Try to find the material based on the path.
                shadowCasterPath = System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(editorWindow))).Replace("\\", "/").Replace("Editor/Managers", "FirstPersonController/Materials/InvisibleShadowCaster.mat");
                if (System.IO.File.Exists(Application.dataPath + "/" + shadowCasterPath.Substring(7))) {
                    return AssetDatabase.LoadAssetAtPath(shadowCasterPath, typeof(Material)) as Material;
                }
            }

            return null;
        }
    }
}