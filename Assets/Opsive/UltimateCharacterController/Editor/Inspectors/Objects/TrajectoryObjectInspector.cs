/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Objects
{
    using Opsive.Shared.Editor.UIElements;
    using Opsive.UltimateCharacterController.Objects;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine.UIElements;

    /// <summary>
    /// Custom inspector for the TrajectoryObject component.
    /// </summary>
    [CustomEditor(typeof(TrajectoryObject), true)]
    public class TrajectoryObjectInspector : UIElementsInspector
    {
        private Foldout m_ImpactFoldout;
        private List<VisualElement> m_ShowOnCollisionModeReflect = new List<VisualElement>();
        private List<VisualElement> m_ShowOnCollisionModeCollide = new List<VisualElement>();
        protected List<string> m_ExcludeFields = new List<string>();
        protected override List<string> ExcludedFields => m_ExcludeFields;

        /// <summary>
        /// Adds the custom UIElements to the top of the container.
        /// </summary>
        /// <param name="container">The parent container.</param>
        protected override void ShowHeaderElements(VisualElement container)
        {
            container.Add(GetPropertyField("m_InitializeOnEnable"));
            m_ExcludeFields.Add("m_InitializeOnEnable");

            PropertiesFoldout(container, "Physics", m_ExcludeFields, new[]
            {
                "m_Mass",
                "m_StartVelocityMultiplier",
                "m_GravityMagnitude",
                "m_Speed",
                "m_RotationSpeed",
                "m_Damping",
                "m_RotationDamping",
                "m_RotateInMoveDirection",
                "m_SettleThreshold",
                "m_SidewaysSettleThreshold",
                "m_StartSidewaysVelocityMagnitude",
                "m_MaxCollisionCount",
            });

            m_ImpactFoldout = PropertiesFoldout(container, "Impact", m_ExcludeFields, new[]
            {
                "m_ImpactLayers",
                "m_SurfaceImpact",
                "m_ForceMultiplier",
            });
            
            // Show certain fields depending on the value of the collision mode.
            var collisionModeField = GetPropertyField("m_CollisionMode");
            m_ImpactFoldout.Add(collisionModeField);
            collisionModeField.RegisterValueChangeCallback(ctx =>
            {
                var newValue = ctx.changedProperty.enumValueIndex;
                OnCollisionModeChange((TrajectoryObject.CollisionMode)newValue);
            });

            m_ShowOnCollisionModeReflect.Clear();
            m_ShowOnCollisionModeCollide.Clear();

            m_ExcludeFields.Add("m_ReflectMultiplier");
            m_ExcludeFields.Add("m_CollisionMode");
            m_ShowOnCollisionModeReflect.Add(GetPropertyField("m_ReflectMultiplier"));
            if (target is ProjectileBase) {
                m_ExcludeFields.Add("m_DisableColliderOnImpact");
                m_ExcludeFields.Add("m_StickyLayers");
                m_ShowOnCollisionModeCollide.Add(GetPropertyField("m_DisableColliderOnImpact"));
                m_ShowOnCollisionModeCollide.Add(GetPropertyField("m_StickyLayers"));
            }

            // Adding the fields is required otherwise they won't be initialized properly.
            for (int i = 0; i < m_ShowOnCollisionModeReflect.Count; i++) {
                m_ImpactFoldout.Add(m_ShowOnCollisionModeReflect[i]);
            }
            for (int i = 0; i < m_ShowOnCollisionModeCollide.Count; i++) {
                m_ImpactFoldout.Add(m_ShowOnCollisionModeCollide[i]);
            }
            if (target is ProjectileBase) {
                m_ImpactFoldout.Add(GetPropertyField("m_InternalImpact"));
                m_ImpactFoldout.Add(GetPropertyField("m_DefaultImpactDamageData"));
                m_ImpactFoldout.Add(GetPropertyField("m_ImpactActionGroup"));
            }

            var audioFoldout = PropertiesFoldout(container, "Audio", m_ExcludeFields, new[]
            {
                "m_ActiveAudioClipSet",
            });

            DrawObjectFields(container);

            var curveFoldout = PropertiesFoldout(container, "Curve", m_ExcludeFields, new[]
            {
                "m_MaxPositionCount",
            });

            OnCollisionModeChange((target as TrajectoryObject)?.Collision ?? TrajectoryObject.CollisionMode.Collide);
        }

        /// <summary>
        /// The collision mode has changed.
        /// </summary>
        /// <param name="newValue">The new Collision mode</param>
        private void OnCollisionModeChange(TrajectoryObject.CollisionMode newValue)
        {
            
            var showReflect = newValue != TrajectoryObject.CollisionMode.Collide && newValue != TrajectoryObject.CollisionMode.Ignore;
            var showCollide = newValue == TrajectoryObject.CollisionMode.Collide;
            
            for (int i = 0; i < m_ShowOnCollisionModeReflect.Count; i++) {
                m_ShowOnCollisionModeReflect[i].style.display = showReflect ?
                    DisplayStyle.Flex :
                    DisplayStyle.None;
            }

            for (int i = 0; i < m_ShowOnCollisionModeCollide.Count; i++) {
                m_ShowOnCollisionModeCollide[i].style.display = showCollide ?
                    DisplayStyle.Flex :
                    DisplayStyle.None;
            }

            Shared.Editor.Utility.EditorUtility.SetDirty(target);
        }

        /// <summary>
        /// Draws the inspector fields for the child object.
        /// </summary>
        protected virtual void DrawObjectFields(VisualElement container) { }
    }
}
