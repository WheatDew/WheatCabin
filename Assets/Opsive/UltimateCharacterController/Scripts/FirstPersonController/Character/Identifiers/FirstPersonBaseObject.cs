/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.FirstPersonController.Character.Identifiers
{
    using Opsive.UltimateCharacterController.Objects;
    using UnityEngine;

    /// <summary>
    /// Identifier component which identifies the first person base object.
    /// </summary>
    public class FirstPersonBaseObject : ObjectIdentifier
    {
        [Tooltip("Should the base object always stay active? This is useful for first person VR.")]
        [SerializeField] protected bool m_AlwaysActive;
        
        [Opsive.Shared.Utility.NonSerialized] public bool AlwaysActive { get => m_AlwaysActive; set => m_AlwaysActive = value; }

        private Transform m_PivotTransform;
        public Transform PivotTransform { get => m_PivotTransform; }

        /// <summary>
        /// Initialize the Pivot Transform.
        /// </summary>
        private void Awake()
        {
            m_PivotTransform = new GameObject(name + "Pivot", typeof(FirstPersonObjectPivot)).transform;
            m_PivotTransform.parent = transform.parent;
            m_PivotTransform.localPosition = Vector3.zero;
            m_PivotTransform.localRotation = Quaternion.identity;
            transform.parent = m_PivotTransform;
        }
    }
}