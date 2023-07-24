/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Character.Abilities
{
    using Opsive.Shared.Events;
    using Opsive.Shared.Game;
    using Opsive.UltimateCharacterController.Character.Abilities.Items;
    using Opsive.UltimateCharacterController.Objects.ItemAssist;
    using UnityEngine;
    using System.Collections.Generic;

    /// <summary>
    /// Uses the CharacterIKBase component to look at the target. The target must be enclosed by a trigger collider.
    /// </summary>
    [DefaultObjectDetection(ObjectDetectionMode.Trigger)]
    public class LookAt : DetectObjectAbilityBase
    {
        [Tooltip("The target object should be within the specified field of view (degrees).")]
        [SerializeField] protected float m_FieldOfView = 160f;
        [Tooltip("The transform that the character should look from.")]
        [SerializeField] protected Transform m_Origin;

        public override bool IsConcurrent { get { return true; } }

        private CharacterIKBase m_CharacterIK;
        private HashSet<Ability> m_PreventativeAbilities = new HashSet<Ability>();

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            var modelManager = m_GameObject.GetCachedComponent<ModelManager>();
            if (modelManager == null) {
                m_CharacterIK = m_GameObject.GetComponentInChildren<CharacterIKBase>();
            } else {
                m_CharacterIK = modelManager.ActiveModel.GetCachedComponent<CharacterIKBase>();
            }

            if (m_Origin == null) {
                var animator = m_GameObject.GetCachedComponent<Animator>();
                if (animator != null) {
                    m_Origin = animator.GetBoneTransform(HumanBodyBones.Head);
                }
                if (m_Origin == null) {
                    m_Origin = m_Transform;
                }
            }

            EventHandler.RegisterEvent<Ability, bool>(m_GameObject, "OnCharacterAbilityActive", OnAbilityActive);
            EventHandler.RegisterEvent<ItemAbility, bool>(m_GameObject, "OnCharacterItemAbilityActive", OnItemAbilityActive);
        }

        /// <summary>
        /// Called when the ablity is tried to be started. If false is returned then the ability will not be started.
        /// </summary>
        /// <returns>True if the ability can be started.</returns>
        public override bool CanStartAbility()
        {
            if (m_CharacterIK == null || m_PreventativeAbilities.Count > 0) {
                return false;
            }
            return base.CanStartAbility();
        }

        /// <summary>
        /// Updates the IK target.
        /// </summary>
        public override void Update()
        {
            base.Update();

            // The character should look at the closest object.
            GameObject closestObject = null;
            var closestDistance = float.MaxValue;
            for (int i = 0; i < m_DetectedTriggerObjectsCount; ++i) {
                if (!m_DetectedTriggerObjects[i].activeSelf) {
                    continue;
                }

                // The character shouldn't look at objects behind the forward direction.
                var direction = (m_DetectedTriggerObjects[i].transform.position - m_Transform.position);
                if (Vector3.Angle(direction, m_Transform.forward) > m_FieldOfView * 0.5f) {
                    continue;
                }

                // Determine the closest target.
                var distance = (m_DetectedTriggerObjects[i].transform.position - m_Transform.position).sqrMagnitude;
                if (distance < closestDistance) {
                    closestDistance = distance;
                    closestObject = m_DetectedTriggerObjects[i];
                }
            }

            // If no object exists then the default look position should be used.
            Vector3 targetPosition;
            if (closestObject != null) {
                var pivotOffset = closestObject.GetCachedComponent<PivotOffset>();
                targetPosition = closestObject.transform.TransformPoint(pivotOffset != null ? pivotOffset.Offset : Vector3.zero);
            } else {
                targetPosition = m_CharacterIK.GetDefaultLookAtPosition();
            }

            m_CharacterIK.SetLookAtPosition(closestObject != null, targetPosition);
        }

        /// <summary>
        /// The character's model has switched.
        /// </summary>
        /// <param name="activeModel">The active character model.</param>
        protected override void OnSwitchModels(GameObject activeModel)
        {
            base.OnSwitchModels(activeModel);

            m_CharacterIK = activeModel.GetCachedComponent<CharacterIKBase>();
        }

        /// <summary>
        /// The ability has stopped running.
        /// </summary>
        /// <param name="force">Was the ability force stopped?</param>
        protected override void AbilityStopped(bool force)
        {
            base.AbilityStopped(force);

            m_CharacterIK.SetLookAtPosition(false, Vector3.zero);
        }

        /// <summary>
        /// The ability has been started or stopped.
        /// </summary>
        /// <param name="ability">The ability which was started or stopped.</param>
        /// <param name="active">True if the ability was started, false if it was stopped.</param>
        private void OnAbilityActive(Ability ability, bool active)
        {
            // LookAt should stop if the Interact ability activates.
            if (!(ability is Interact)) {
                return;
            }

            if (active) {
                m_PreventativeAbilities.Add(ability);
                // LookAt should not be active when an ability prevents it.
                if (IsActive) {
                    StopAbility();
                }
            } else {
                m_PreventativeAbilities.Remove(ability);
            }
        }

        /// <summary>
        /// The item ability has been started or stopped.
        /// </summary>
        /// <param name="itemAbility">The item ability which was started or stopped.</param>
        /// <param name="active">True if the ability was started, false if it was stopped.</param>
        private void OnItemAbilityActive(ItemAbility itemAbility, bool active)
        {
            // LookAt should stop if the Aim, Use, or Reload abilities activate.
            if (!(itemAbility is Aim || itemAbility is Use || itemAbility is Reload)) {
                return;
            }

            if (active) {
                m_PreventativeAbilities.Add(itemAbility);
                // LookAt should not be active when an ability prevents it.
                if (IsActive) {
                    StopAbility();
                }
            } else {
                m_PreventativeAbilities.Remove(itemAbility);
            }
        }

        /// <summary>
        /// Called when the character is destroyed.
        /// </summary>
        public override void OnDestroy()
        {
            base.OnDestroy();

            EventHandler.UnregisterEvent<Ability, bool>(m_GameObject, "OnCharacterAbilityActive", OnAbilityActive);
            EventHandler.UnregisterEvent<ItemAbility, bool>(m_GameObject, "OnCharacterItemAbilityActive", OnItemAbilityActive);
        }
    }
}