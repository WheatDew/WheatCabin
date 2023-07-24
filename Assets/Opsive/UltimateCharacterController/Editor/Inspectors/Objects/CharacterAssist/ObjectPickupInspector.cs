/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Objects.CharacterAssist
{
    using System.Collections.Generic;
    using Opsive.Shared.Editor.Inspectors;
    using Opsive.Shared.Editor.UIElements;
    using Opsive.Shared.Utility;
    using Opsive.UltimateCharacterController.Editor.Inspectors.Audio;
    using Opsive.UltimateCharacterController.Objects.CharacterAssist;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;
    using ReorderableList = UnityEditorInternal.ReorderableList;

    /// <summary>
    /// Custom inspector for the ObjectPickup component.
    /// </summary>
    [CustomEditor(typeof(ObjectPickup), true)]
    public class ObjectPickupInspector : UIElementsInspector
    {
        private ObjectPickup m_ObjectPickup;

        protected List<string> m_ObjectPickupExcludeField = new List<string>()
            { "m_PickupAudioClipSet", "m_PickupMessageText", "m_PickupMessageIcon" };

        protected override List<string> ExcludedFields => m_ObjectPickupExcludeField;

        /// <summary>
        /// The inspector has been enabled.
        /// </summary>
        protected virtual void OnEnable()
        {
            m_ObjectPickup = target as ObjectPickup;
        }

        protected override void ShowFooterElements(VisualElement container)
        {
            base.ShowFooterElements(container);

            // Audio
            FieldInspectorView.AddField(target, target, "m_PickupAudioClipSet", container, (object obj) =>
            {
                Shared.Editor.Utility.EditorUtility.SetDirty(target);
            }, null);
            
            // UI
            var uiFoldout = new Foldout();
            uiFoldout.text = "UI";

            FieldInspectorView.AddField(target, target, "m_PickupMessageText", uiFoldout, (object obj) =>
            {
                Shared.Editor.Utility.EditorUtility.SetDirty(target);
            }, null);
            
            FieldInspectorView.AddField(target, target, "m_PickupMessageIcon", uiFoldout, (object obj) =>
            {
                Shared.Editor.Utility.EditorUtility.SetDirty(target);
            }, null);
            
            container.Add(uiFoldout);
        }
    }
}