/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Demo
{
    using Opsive.Shared.Game;
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.Game;
    using Opsive.UltimateCharacterController.Utility;
    using UnityEngine;

    /// <summary>
    /// Adjusts the size of the trigger when the character enters. This is useful for movement type triggers so the character doesn't keep switching modes as the controls
    /// change while on the edge of the trigger.
    /// </summary>
    public class TriggerAdjuster : MonoBehaviour
    {
        [Tooltip("Specifies the amount to adjust the BoxCollider center by.")]
        [SerializeField] protected Vector3 m_BoxColliderCenterAdjustment;
        [Tooltip("Specifies the amount to expand the BoxCollider trigger by.")]
        [SerializeField] protected Vector3 m_BoxColliderExpansion;
        [Tooltip("Specifies the amount to inflate the MeshCollider trigger by.")]
        [SerializeField] protected Mesh m_ExpandedMesh;

        private GameObject m_ActiveObject;
        private Collider m_Collider;
        private Mesh m_OriginalMesh;
        private bool m_AllowTriggerExit = true;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        private void Awake()
        {
            m_Collider = GetComponent<Collider>();

            if (m_Collider is MeshCollider) {
                m_OriginalMesh = (m_Collider as MeshCollider).sharedMesh;
            }
        }

        /// <summary>
        /// An object has entered the trigger.
        /// </summary>
        /// <param name="other">The object that entered the trigger.</param>
        private void OnTriggerEnter(Collider other)
        {
            if (m_ActiveObject != null || !MathUtility.InLayerMask(other.gameObject.layer, 1 << LayerManager.Character)) {
                return;
            }

            var characterLocomotion = other.GetComponentInParent<UltimateCharacterLocomotion>();
            if (characterLocomotion == null) {
                return;
            }

            // The object is a character. Expand the trigger.
            if (m_Collider is BoxCollider) {
                AdjustBoxCollider(m_Collider as BoxCollider, m_BoxColliderCenterAdjustment, m_BoxColliderExpansion);
            } else if (m_Collider is MeshCollider) {
                // When the mesh is inflated it'll trigger an OnTriggerExit callback. Prevent this callback from doing anything until 
                // after the inflated mesh has stabalized.
                m_AllowTriggerExit = false;
                Scheduler.ScheduleFixed(Time.fixedDeltaTime * 2, () => { m_AllowTriggerExit = true; });
                (m_Collider as MeshCollider).sharedMesh = m_ExpandedMesh;
            }
            m_ActiveObject = characterLocomotion.gameObject;
        }

        /// <summary>
        /// Adjusts the BoxCollider.
        /// </summary>
        /// <param name="boxCollider">The BoxCollider that should be adjusted.</param>
        /// <param name="centerAdjustment">The amount to adjust the BoxCollider center by.</param>
        /// <param name="sizeAdjustment">The amount to adjust the BoxCollider size by.</param>
        private void AdjustBoxCollider(BoxCollider boxCollider, Vector3 centerAdjustment, Vector3 sizeAdjustment)
        {
            boxCollider.center = boxCollider.center + centerAdjustment;
            boxCollider.size = boxCollider.size + sizeAdjustment;
        }

        /// <summary>
        /// An object has exited the trigger.
        /// </summary>
        /// <param name="other">The collider that exited the trigger.</param>
        private void OnTriggerExit(Collider other)
        {
            if (!m_AllowTriggerExit) {
                return;
            }

            var characterLocomotion = other.GetComponentInParent<UltimateCharacterLocomotion>();
            if (characterLocomotion == null) {
                return;
            }

            if (m_ActiveObject == characterLocomotion.gameObject) {
                if (m_Collider is BoxCollider) {
                    AdjustBoxCollider(m_Collider as BoxCollider, -m_BoxColliderCenterAdjustment, -m_BoxColliderExpansion);
                } else if (m_Collider is MeshCollider) {
                    (m_Collider as MeshCollider).sharedMesh = m_OriginalMesh;
                }
                m_ActiveObject = null;
            }
        }
    }
}