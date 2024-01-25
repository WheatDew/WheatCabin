using UnityEngine;

namespace EasyBuildSystem.Packages.Addons.AdvancedBuilding
{
    [RequireComponent(typeof(Rigidbody), typeof(Collider))]
    public class CarriableObject : MonoBehaviour, IInteractable
    {
        public enum ResourceType { NONE, LOG, STICK, ROCK }

        [SerializeField] private ResourceType m_CarriableType;
        public ResourceType CarriableType => m_CarriableType;

        [SerializeField] private InteractableType m_InteractableType;
        public InteractableType InteractableType => m_InteractableType;

        public bool IsCarried { get; private set; } = false;

        public Transform CarriedBy { get; private set; } = null;

        public Bounds MeshBounds => GetComponent<Renderer>().bounds;

        private Rigidbody m_Rigidbody;
        private Collider m_Collider;

        private void Awake()
        {
            m_Rigidbody = GetComponent<Rigidbody>();
            m_Collider = GetComponent<Collider>();
        }

        public void PickUp(CarriableController.CarriableSettings.CarriableType carriablePosition)
        {
            m_Rigidbody.isKinematic = true;
            m_Collider.enabled = false;

            CarriedBy = carriablePosition.Parent;
            IsCarried = true;

            transform.SetParent(CarriedBy, worldPositionStays: false);
            transform.localPosition = carriablePosition.OffsetPosition;
            transform.localEulerAngles = carriablePosition.OffsetRotation;
        }

        public void Drop(Vector3 dropForce)
        {
            m_Rigidbody.isKinematic = false;
            m_Collider.enabled = true;

            m_Rigidbody.AddForce(dropForce, ForceMode.VelocityChange);

            transform.SetParent(null);

            CarriedBy = null;
            IsCarried = false;
        }
    }
}