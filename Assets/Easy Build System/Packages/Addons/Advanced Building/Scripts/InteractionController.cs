using System;

using UnityEngine;
using UnityEngine.Events;

using EasyBuildSystem.Features.Runtime.Bases;

namespace EasyBuildSystem.Packages.Addons.AdvancedBuilding
{
    public class InteractionController : Singleton<InteractionController>
    {
        [Header("Interaction Settings")]

        [SerializeField] KeyCode m_InteractionKey = KeyCode.E;
        [SerializeField] float m_InteractionRange = 2f;
        [SerializeField] float m_InteractionAngleThreshold = 45f;
        [SerializeField] LayerMask m_InteractionLayer;

        IInteractable m_Interactable;

        public IInteractable Interactable { get { return m_Interactable; } }

        Collider m_LastHitCollider;

        public class InteractedEvent : UnityEvent<IInteractable> { }
        public InteractedEvent OnInteractedEvent = new InteractedEvent();

        void Update()
        {
            CheckForInteractables();
            HandleInput();
        }

        void CheckForInteractables()
        {
            m_Interactable = null;

            if (m_LastHitCollider != null && Vector3.Distance(transform.position, m_LastHitCollider.transform.position) > m_InteractionRange)
            {
                m_LastHitCollider = null;
            }

            Collider[] hitColliders = Physics.OverlapSphere(transform.position, m_InteractionRange, m_InteractionLayer);

            float closestAngle = Mathf.Infinity;

            foreach (Collider hitCollider in hitColliders)
            {
                IInteractable interactable = hitCollider.GetComponentInParent<IInteractable>();

                if (interactable != null)
                {
                    Vector3 direction = ((Component)interactable).transform.position - transform.position;
                    float angle = Vector3.Angle(direction, transform.forward);

                    if (angle < closestAngle && angle < m_InteractionAngleThreshold)
                    {
                        m_Interactable = interactable;
                        m_LastHitCollider = hitCollider;
                        closestAngle = angle;
                    }
                }
            }
        }

        void HandleInput()
        {
            if (m_Interactable != null && Input.GetKeyDown(m_InteractionKey))
            {
                OnInteractedEvent.Invoke(m_Interactable);
            }
        }
    }
}