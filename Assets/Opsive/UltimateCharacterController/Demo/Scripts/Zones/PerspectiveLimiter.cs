/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Demo.Zones
{
    using Opsive.Shared.StateSystem;
    using Opsive.UltimateCharacterController.Camera;
    using Opsive.UltimateCharacterController.Game;
    using Opsive.UltimateCharacterController.Utility;
    using UnityEngine;

    /// <summary>
    /// Limits the camera to a single perspective.
    /// </summary>
    public class PerspectiveLimiter : MonoBehaviour, IZoneTrigger
    {
        [Tooltip("The LayerMask that the trigger can set the state of.")]
        [SerializeField] protected LayerMask m_LayerMask = 1 << LayerManager.Character;
        [Tooltip("Should the first person perspective be set?")]
        [SerializeField] protected bool m_FirstPersonPerspective = true;

        private CameraController m_CameraController;

        /// <summary>
        /// Initializes the default values.
        /// </summary>
        private void Awake()
        {
            var demoManager = Object.FindObjectOfType<DemoManager>();
            m_CameraController = Shared.Camera.CameraUtility.FindCamera(demoManager.Character).GetComponent<CameraController>();
        }

        /// <summary>
        /// The other collider has entered the trigger.
        /// </summary>
        /// <param name="other">The collider which entered the trigger.</param>
        private void OnTriggerEnter(Collider other)
        {
            if (!MathUtility.InLayerMask(other.gameObject.layer, m_LayerMask)) {
                return;
            }

            m_CameraController.SetPerspective(m_FirstPersonPerspective);
            var character = other.gameObject.GetComponentInParent<Character.UltimateCharacterLocomotion>().gameObject;
            StateManager.SetState(character, "DisablePerspectiveSwitch", true);
        }

        /// <summary>
        /// The other collider has exited the trigger.
        /// </summary>
        /// <param name="other">The collider which exited the trigger.</param>
        private void OnTriggerExit(Collider other)
        {
            if (!MathUtility.InLayerMask(other.gameObject.layer, m_LayerMask)) {
                return;
            }
            var character = other.gameObject.GetComponentInParent<Character.UltimateCharacterLocomotion>().gameObject;
            ExitZone(character);
        }

        /// <summary>
        /// Resets the zone after the character exits.
        /// </summary>
        /// <param name="character">The character that exited the zone.</param>
        public void ExitZone(GameObject character)
        {
            StateManager.SetState(character, "DisablePerspectiveSwitch", false);
        }
    }
}
