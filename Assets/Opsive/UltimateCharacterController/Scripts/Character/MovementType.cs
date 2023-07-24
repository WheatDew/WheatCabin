/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Character.MovementTypes
{
    using Opsive.UltimateCharacterController.Character;

    using Opsive.Shared.Events;
    using Opsive.Shared.StateSystem;
    using UnityEngine;

    /// <summary>
    /// MovementType is an abstract class which tells the character controller how to move and rotate. This allows for different movement types such as combat and adventure.
    /// </summary>
    [System.Serializable]
    [UnityEngine.Scripting.Preserve]
    public abstract class MovementType : StateObject
    {
        protected GameObject m_GameObject;
        protected Rigidbody m_Rigidbody;
        protected UltimateCharacterLocomotion m_CharacterLocomotion;
        protected ILookSource m_LookSource;

        private bool m_ForceIndependentLook;
        public abstract bool FirstPersonPerspective { get; }

        /// <summary>
        /// Initializes the MovementType.
        /// </summary>
        /// <param name="CharacterLocomotion">The reference to the character motor component.</param>
        public virtual void Initialize(UltimateCharacterLocomotion characterLocomotion)
        {
            m_CharacterLocomotion = characterLocomotion;
            m_GameObject = m_CharacterLocomotion.gameObject;
            m_Rigidbody = m_CharacterLocomotion.GetComponent<Rigidbody>();
            m_LookSource = m_CharacterLocomotion.LookSource;

            // The StateObject class needs to initialize itself.
            Initialize(m_GameObject);
        }

        /// <summary>
        /// Register for any interested events.
        /// </summary>
        public virtual void Awake()
        {
            EventHandler.RegisterEvent<ILookSource>(m_GameObject, "OnCharacterAttachLookSource", OnAttachLookSource);
            EventHandler.RegisterEvent<bool>(m_GameObject, "OnCharacterForceIndependentLook", OnForceIndependentLook);
        }

        /// <summary>
        /// A new ILookSource object has been attached to the character.
        /// </summary>
        /// <param name="lookSource">The ILookSource object attached to the character.</param>
        protected virtual void OnAttachLookSource(ILookSource lookSource)
        {
            m_LookSource = lookSource;
        }

        /// <summary>
        /// The movement type has changed.
        /// </summary>
        /// <param name="activate">Should the current movement type be activated?</param>
        public virtual void ChangeMovementType(bool activate) { }

        /// <summary>
        /// Returns the delta yaw rotation of the character.
        /// </summary>
        /// <param name="characterHorizontalMovement">The character's horizontal movement.</param>
        /// <param name="characterForwardMovement">The character's forward movement.</param>
        /// <param name="cameraHorizontalMovement">The camera's horizontal movement.</param>
        /// <param name="cameraVerticalMovement">The camera's vertical movement.</param>
        /// <returns>The delta yaw rotation of the character.</returns>
        public abstract float GetDeltaYawRotation(float characterHorizontalMovement, float characterForwardMovement, float cameraHorizontalMovement, float cameraVerticalMovement);

        /// <summary>
        /// Gets the controller's input vector relative to the movement type.
        /// </summary>
        /// <param name="inputVector">The current input vector.</param>
        /// <returns>The updated input vector.</returns>
        public abstract Vector2 GetInputVector(Vector2 inputVector);

        /// <summary>
        /// Can the character look independently of the transform rotation?
        /// </summary>
        /// <param name="characterLookDirection">Is the character look direction being retrieved?</param>
        /// <returns>True if the character should look independently of the transform rotation.</returns>
        public virtual bool UseIndependentLook(bool characterLookDirection) { return m_ForceIndependentLook; }

        /// <summary>
        /// Event received when the independent look state should change.
        /// </summary>
        /// <param name="forceIndependentLook">Can the character look independently of the transform rotation?</param>
        private void OnForceIndependentLook(bool forceIndependentLook)
        {
            m_ForceIndependentLook = forceIndependentLook;
        }

        /// <summary>
        /// The GameObject has been destroyed.
        /// </summary>
        public virtual void OnDestroy()
        {
            EventHandler.UnregisterEvent<ILookSource>(m_GameObject, "OnCharacterAttachLookSource", OnAttachLookSource);
            EventHandler.UnregisterEvent<bool>(m_GameObject, "OnCharacterForceIndependentLook", OnForceIndependentLook);
        }
    }
}