/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Objects
{
    using Opsive.Shared.Editor.Inspectors;
    using Opsive.Shared.Editor.UIElements;
    using Opsive.UltimateCharacterController.Editor.Inspectors.Audio;
    using Opsive.UltimateCharacterController.Objects;
    using UnityEditor;
    using UnityEngine;
    using ReorderableList = UnityEditorInternal.ReorderableList;

    /// <summary>
    /// Custom inspector for the Explosion component.
    /// </summary>
    [CustomEditor(typeof(Explosion))]
    public class ExplosionInspector : UIElementsInspector
    {
        private Explosion m_Explosion;

        /// <summary>
        /// The inspector has been enabled.
        /// </summary>
        public void OnEnable()
        {
            m_Explosion = target as Explosion;
        }
    }
}
