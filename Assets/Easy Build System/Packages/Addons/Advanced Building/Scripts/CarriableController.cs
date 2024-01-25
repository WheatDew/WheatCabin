using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using EasyBuildSystem.Features.Runtime.Bases;

namespace EasyBuildSystem.Packages.Addons.AdvancedBuilding
{
    public class CarriableController : Singleton<CarriableController>
    {
        [Serializable]
        public class CarriableSettings
        {
            [Serializable]
            public class CarriableType
            {
                public Transform Parent;
                public CarriableObject CarriableObject;
                public Vector3 OffsetPosition;
                public Vector3 OffsetRotation;
            }

            public CarriableObject.ResourceType ResourceType;
            public CarriableType[] CarriableTypes;
        }

        [Header("Carriable Controller Settings")]
        [SerializeField] CarriableSettings[] m_Carriables;

        [SerializeField] KeyCode m_DropKey = KeyCode.G;
        [SerializeField] float m_DropForce = 10f;

        readonly List<CarriableObject> m_CurrentCarriables = new List<CarriableObject>();
        public List<CarriableObject> CurrentCarriables => m_CurrentCarriables;

        public int CarriableCount { get; set; }

        CarriableObject.ResourceType m_CurrentType;

        void Start()
        {
            InteractionController.Instance.OnInteractedEvent.AddListener(OnInteracted);
        }

        void Update()
        {
            if (Input.GetKeyDown(m_DropKey) && CarriableCount > 0)
            {
                Drop(m_DropForce);
            }
        }

        void OnInteracted(IInteractable interactable)
        {
            if (interactable.InteractableType != InteractableType.CARRIABLE)
                return;

            CarriableObject carriableObject = interactable as CarriableObject;

            if (!IsPickable(carriableObject))
                return;

            if (IsFull(carriableObject))
                return;

            if (m_CurrentType == CarriableObject.ResourceType.NONE)
            {
                m_CurrentType = carriableObject.CarriableType;
            }
            else if (m_CurrentType != carriableObject.CarriableType)
            {
                return;
            }

            m_CurrentCarriables.Add(carriableObject);

            CarriableSettings.CarriableType nextPosition = GetNextPosition(carriableObject.CarriableType);

            if (nextPosition == null)
            {
                return;
            }

            carriableObject.PickUp(nextPosition);

            CarriableCount++;
        }

        public void Remove(CarriableObject carriableObject)
        {
            CarriableCount--;

            CurrentCarriables.Remove(carriableObject);

            if (CarriableCount == 0)
            {
                m_CurrentType = CarriableObject.ResourceType.NONE;
            }

            Destroy(carriableObject.gameObject);
        }

        bool IsPickable(CarriableObject carriableObject)
        {
            return m_Carriables.Any(carriable => carriable.CarriableTypes.Any(type => type.CarriableObject.CarriableType == carriableObject.CarriableType));
        }

        bool IsFull(CarriableObject carriableObject)
        {
            foreach (CarriableSettings carriable in m_Carriables)
            {
                if (carriable.ResourceType == carriableObject.CarriableType && carriable.CarriableTypes.Length <= CarriableCount)
                {
                    return true;
                }
            }

            return false;
        }

        CarriableSettings.CarriableType GetNextPosition(CarriableObject.ResourceType type)
        {
            foreach (CarriableSettings carriable in m_Carriables)
            {
                if (carriable.ResourceType == type && carriable.CarriableTypes.Length > CarriableCount)
                {
                    return carriable.CarriableTypes[CarriableCount];
                }
            }

            return null;
        }

        void Drop(float force)
        {
            int index = CarriableCount - 1;

            m_CurrentCarriables[index].Drop(transform.TransformDirection(Vector3.up + Vector3.forward * force));
            m_CurrentCarriables.RemoveAt(index);

            CarriableCount--;

            if (CarriableCount == 0)
            {
                m_CurrentType = CarriableObject.ResourceType.NONE;
            }
        }
    }
}