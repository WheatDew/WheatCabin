/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Game
{
    using UnityEngine;

    /// <summary>
    /// The Kinematic Object component allows an object to be moved outside of the Simulation Manager loop while still being tracked by the Simulation Manager.
    /// This component should be used with the Move With Object ability:
    /// https://opsive.com/support/documentation/ultimate-character-controller/character/abilities/included-abilities/move-with-object/
    /// </summary>
    public class KinematicObject : MonoBehaviour, IKinematicObject
    {
        private Transform m_Transform;
        private Vector3 m_LastPosition;
        private Quaternion m_LastRotation;

        private int m_SimulationIndex;

        public Transform Transform { get { return m_Transform; } }
        public int SimulationIndex { set {  m_SimulationIndex = value; } }

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        private void Awake()
        {
            m_Transform = transform;
        }

        /// <summary>
        /// Registers the object with the Simulation Manager.
        /// </summary>
        public void OnEnable()
        {
            m_SimulationIndex = SimulationManager.RegisterKinematicObject(this);

            m_LastPosition = m_Transform.position;
            m_LastRotation = m_Transform.rotation;
        }

        /// <summary>
        /// Fixed update for physic synced computations.
        /// </summary>
        public void FixedUpdate()
        {
            m_LastPosition = m_Transform.position;
            m_LastRotation = m_Transform.rotation;
        }

        /// <summary>
        /// Sets up the object to be moved by the Simulation Manager.
        /// </summary>
        public void Move()
        {
            m_Transform.SetPositionAndRotation(m_LastPosition, m_LastRotation);
        }

        /// <summary>
        /// Unregisters the object with the Simulation Manager.
        /// </summary>
        public void OnDisable()
        {
            SimulationManager.UnregisterKinematicObject(m_SimulationIndex);
        }
    }
}