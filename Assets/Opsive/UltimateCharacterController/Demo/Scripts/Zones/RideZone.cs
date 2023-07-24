/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Demo.Zones
{
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.Character.Abilities;
    using UnityEngine;

    /// <summary>
    /// Ensures the zone is cleaned up when the character leaves the zone.
    /// </summary>
    public class RideZone : MonoBehaviour, IZoneTrigger
    {
        [Tooltip("A reference to Blitz.")]
        [SerializeField] protected Transform m_Blitz;

        private Vector3 m_StartPosition;
        private Quaternion m_StartRotation;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        private void Awake()
        {
            m_StartPosition = m_Blitz.position;
            m_StartRotation = m_Blitz.rotation;
        }

        /// <summary>
        /// Resets the zone after the character exits.
        /// </summary>
        /// <param name="character">The character that exited the zone.</param>
        public void ExitZone(GameObject character)
        {
            var characterLocomotion = character.GetComponent<UltimateCharacterLocomotion>();
            if (characterLocomotion.IsAbilityTypeActive<Ride>()) {
                characterLocomotion.TryStopAbility(characterLocomotion.GetAbility<Ride>(), true);
            }

            var blitzCharacterLocomotion = m_Blitz.GetComponent<UltimateCharacterLocomotion>();
            blitzCharacterLocomotion.SetPositionAndRotation(m_StartPosition, m_StartRotation, true, true);
        }
    }
}