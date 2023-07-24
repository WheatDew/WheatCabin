/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Character.Identifiers
{
    using Opsive.Shared.StateSystem;
#if THIRD_PERSON_CONTROLLER
    using Opsive.UltimateCharacterController.ThirdPersonController.Character;
#endif
    using UnityEngine;

    /// <summary>
    /// Identifying component which specifies the object should be hidden while in first person view.
    /// </summary>
    public class ThirdPersonObject : StateBehavior
    {
        [Tooltip("Should the object be forced visible even if it is in a first person view?")]
        [SerializeField] protected bool m_ForceVisible;
#if !THIRD_PERSON_CONTROLLER
        [Tooltip("The materials that should be used when the object is visible.")]
        [SerializeField] protected Material[] m_VisibleMaterials;
#endif
        [Tooltip("Should the object be visible when the character dies? This value will only be checked if the PerspectiveMonitor.ObjectDeathVisiblity is set to ThirdPersonObjectDetermined.")]
        [SerializeField] protected bool m_FirstPersonVisibleOnDeath;

        public bool ForceVisible { get { return m_ForceVisible; }
#if THIRD_PERSON_CONTROLLER
            set { if (m_ForceVisible != value) { m_ForceVisible = value; if (m_PerspectiveMonitor != null) { m_PerspectiveMonitor.UpdateThirdPersonMaterials(false); } } }
#else
            set { m_ForceVisible = value; if (m_Renderer == null) { return; } m_Renderer.materials = m_ForceVisible ? m_VisibleMaterials : m_InvisibleMaterials;  }
#endif
        }
        public bool FirstPersonVisibleOnDeath { get { return m_FirstPersonVisibleOnDeath; } }

#if THIRD_PERSON_CONTROLLER
        private PerspectiveMonitor m_PerspectiveMonitor;
#else
        private Renderer m_Renderer;
        private Material[] m_InvisibleMaterials;
#endif

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

#if THIRD_PERSON_CONTROLLER
            m_PerspectiveMonitor = gameObject.GetComponentInParent<PerspectiveMonitor>();
#else
            m_Renderer = gameObject.GetComponent<Renderer>();
            if (m_Renderer == null) {
                return;
            }

            m_InvisibleMaterials = m_Renderer.materials;
#endif
        }
    }
}