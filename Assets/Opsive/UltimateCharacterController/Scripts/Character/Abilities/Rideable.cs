/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Character.Abilities
{
    using Opsive.Shared.Utility;
    using Opsive.Shared.Events;
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.Utility;
    using UnityEngine;

    /// <summary>
    /// Added to a Ultimate Character Controller character that can be ridden.
    /// </summary>
    [DefaultAbilityIndex(12)]
    [DefaultStartType(AbilityStartType.Manual)]
    [DefaultStopType(AbilityStopType.Manual)]
    public class Rideable : Ability
    {
        [Tooltip("The Transform that the character should be located to after mounting on the object.")]
        [SerializeField] protected Transform m_RideLocation;
        [Tooltip("The collider area that should be checked to determine if the character can dismount on the left side.")]
        [SerializeField] protected Collider m_LeftDismountCollider;
        [Tooltip("The collider area that should be checked to determine if the character can dismount on the right side.")]
        [SerializeField] protected Collider m_RightDismountCollider;

        public Transform RideLocation { get { return m_RideLocation; } }
        [NonSerialized] public Collider LeftDismountCollider { get { return m_LeftDismountCollider; } set { m_LeftDismountCollider = value; } }
        [NonSerialized] public Collider RightDismountCollider { get { return m_RightDismountCollider; } set { m_RightDismountCollider = value; } }

        private Collider[] m_OriginalColliders;
        private Ride m_Ride;
        private Collider[] m_OverlapColliders;

        public override int AbilityIntData
        {
            get
            {
                // The rideable character should stay synchronized with the ride character.
                return m_Ride.AbilityIntData;
            }
        }
        public override bool IsConcurrent { get { return true; } }

        public UltimateCharacterLocomotion CharacterLocomotion { get { return m_CharacterLocomotion; } }
        public GameObject GameObject { get { return m_GameObject; } }
        public Rigidbody Rigidbody { get { return m_Rigidbody; } }

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            if (m_RideLocation == null) {
                Debug.LogWarning("Warning: The RideLocation is null. This should be set to the Transform that the Ride character is parented to.");
                m_RideLocation = m_Transform;
            }
            m_OverlapColliders = new Collider[1];

            // The colliders do not need to be enabled for the physics check and they should not interfere with any other objects.
            if (m_LeftDismountCollider != null) {
                m_LeftDismountCollider.enabled = false;
            }
            if (m_RightDismountCollider != null) {
                m_RightDismountCollider.enabled = false;
            }

            // The RideableObject should start off not responding to input.
            EventHandler.ExecuteEvent<bool>(m_GameObject, "OnEnableGameplayInput", false);
        }

        /// <summary>
        /// The character has mounted on the RideableObject.
        /// </summary>
        /// <param name="ride">The character's Ride ability that mounted on the RideableObject.</param>
        public void Mount(Ride ride)
        {
            m_Ride = ride;

            StartAbility();
        }

        /// <summary>
        /// The ability has started.
        /// </summary>
        protected override void AbilityStarted()
        {
            base.AbilityStarted();

            // Set the parent of the character so it moves with the rideable object.
            var characterLocomotion = m_Ride.CharacterLocomotion;

            if (m_OriginalColliders == null) {
                m_OriginalColliders = new Collider[m_CharacterLocomotion.ColliderCount];
            } else if (m_OriginalColliders.Length < m_CharacterLocomotion.ColliderCount) {
                System.Array.Resize(ref m_OriginalColliders, m_CharacterLocomotion.ColliderCount);
            }
            for (int i = 0; i < m_CharacterLocomotion.ColliderCount; ++i) {
                m_OriginalColliders[i] = m_CharacterLocomotion.Colliders[i];
            }

            // The rideable object should ignore the character's colliders.
            m_CharacterLocomotion.AddIgnoredColliders(characterLocomotion.Colliders);
            m_CharacterLocomotion.AddIgnoredColliders(characterLocomotion.IgnoredColliders);

            // The character should ignore the rideable object's layers. This will prevent the character from detecting the rideable colliders.
            characterLocomotion.AddIgnoredColliders(m_OriginalColliders);
        }

        /// <summary>
        /// The character has mounted on the Rideable character.
        /// </summary>
        public void OnCharacterMount()
        {
            // Enable input so the RideableObject can move.
            EventHandler.ExecuteEvent<bool>(m_GameObject, "OnEnableGameplayInput", true);
            UpdateAbilityAnimatorParameters();

            // The rideable object should use the character's colliders to prevent clipping.
            var characterLocomotion = m_Ride.CharacterLocomotion;
            m_CharacterLocomotion.RemoveIgnoredColliders(characterLocomotion.Colliders);
            m_CharacterLocomotion.AddColliders(characterLocomotion.Colliders);
        }

        /// <summary>
        /// Can the character dismount? The MoveTowardsLocation dismount colliders must be clear for the charcter to be able to dismount.
        /// </summary>
        /// <param name="leftDismount">Should the character dismount from the left side?</param>
        /// <returns>True if the character can dismount.</returns>
        public bool CanDismount(ref bool leftDismount)
        {
            var dismountCollider = leftDismount ? m_LeftDismountCollider : m_RightDismountCollider;
            if (!DismountColliderOverlap(dismountCollider)) {
                return true;
            }
            // If the original direction can't be used then try the other side.
            dismountCollider = leftDismount ? m_RightDismountCollider : m_LeftDismountCollider;
            if (!DismountColliderOverlap(dismountCollider)) {
                leftDismount = !leftDismount;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Is the collider overlapping with any other objects?
        /// </summary>
        /// <param name="dismountCollider">The collider to determine if it is overlapping with another object.</param>
        /// <returns>True if the collider is overlapping.</returns>
        private bool DismountColliderOverlap(Collider dismountCollider)
        {
            if (dismountCollider == null) {
                return true;
            }

            int hitCount;
            var dismountTransform = dismountCollider.transform;
            if (dismountCollider is CapsuleCollider) {
                Vector3 startEndCap, endEndCap;
                var capsuleCollider = dismountCollider as CapsuleCollider;
                MathUtility.CapsuleColliderEndCaps(capsuleCollider, dismountTransform.TransformPoint(capsuleCollider.center), dismountTransform.rotation, out startEndCap, out endEndCap);
                hitCount = Physics.OverlapCapsuleNonAlloc(startEndCap, endEndCap, capsuleCollider.radius * MathUtility.ColliderScaleMultiplier(capsuleCollider), m_OverlapColliders, 
                                m_CharacterLayerManager.SolidObjectLayers, QueryTriggerInteraction.Ignore);
            } else if (dismountCollider is BoxCollider) {
                var boxCollider = dismountCollider as BoxCollider;
                hitCount = Physics.OverlapBoxNonAlloc(dismountTransform.TransformPoint(boxCollider.center), Vector3.Scale(boxCollider.size, dismountTransform.lossyScale) / 2, 
                                    m_OverlapColliders, dismountTransform.rotation, m_CharacterLayerManager.SolidObjectLayers, QueryTriggerInteraction.Ignore);
            } else { // SphereCollider.
                var sphereCollider = dismountCollider as SphereCollider;
                hitCount = Physics.OverlapSphereNonAlloc(dismountTransform.TransformPoint(sphereCollider.center), sphereCollider.radius * MathUtility.ColliderScaleMultiplier(sphereCollider), 
                                        m_OverlapColliders, m_CharacterLayerManager.SolidObjectLayers, QueryTriggerInteraction.Ignore);
            }

            // Any overlap occurs anytime there is more one collider intersecting the dismount colliders.
            return hitCount > 0;
        }

        /// <summary>
        /// The character has started to dismount from the Rideable object.
        /// </summary>
        public void StartDismount()
        {
            UpdateAbilityAnimatorParameters();

            // The character should not use the ride object's colliders anymore.
            var characterLocomotion = m_Ride.CharacterLocomotion;
            m_CharacterLocomotion.RemoveColliders(characterLocomotion.Colliders);
            m_CharacterLocomotion.AddIgnoredColliders(characterLocomotion.Colliders);

            EventHandler.ExecuteEvent<bool>(m_GameObject, "OnEnableGameplayInput", false);
        }

        /// <summary>
        /// The character has dismounted from the Rideable object.
        /// </summary>
        public void Dismounted()
        {
            StopAbility();
        }

        /// <summary>
        /// The ability has stopped running.
        /// </summary>
        /// <param name="force">Was the ability force stopped?</param>
        protected override void AbilityStopped(bool force)
        {
            base.AbilityStopped(force);

            // Revert the rideable colliders on the ride character.
            var characterLocomotion = m_Ride.CharacterLocomotion;
            characterLocomotion.RemoveIgnoredColliders(m_OriginalColliders);

            // Revert the added colliders.
            m_CharacterLocomotion.RemoveIgnoredColliders(characterLocomotion.Colliders);
            m_CharacterLocomotion.RemoveIgnoredColliders(characterLocomotion.IgnoredColliders);

            // The RideableObject is no longer in control of the input.
            EventHandler.ExecuteEvent<bool>(m_GameObject, "OnEnableGameplayInput", false);
        }
    }
}