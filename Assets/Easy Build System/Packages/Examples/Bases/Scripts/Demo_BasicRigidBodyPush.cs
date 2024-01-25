/// <summary>
/// Project : Easy Build System
/// Class : Demo_BasicRigidBodyPush.cs
/// Namespace : EasyBuildSystem.Examples.Bases.Scripts
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using UnityEngine;

namespace EasyBuildSystem.Examples.Bases.Scripts
{
    public class Demo_BasicRigidBodyPush : MonoBehaviour
    {
        [SerializeField] LayerMask m_PushLayers;
        [SerializeField] bool m_CanPush;
        [Range(0.5f, 5f)] public float m_Strength = 1.1f;

        void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (m_CanPush)
            {
                PushRigidBodies(hit);
            }
        }

        void PushRigidBodies(ControllerColliderHit hit)
        {
            Rigidbody body = hit.collider.attachedRigidbody;

            if (body == null || body.isKinematic)
            {
                return;
            }

            int bodyLayerMask = 1 << body.gameObject.layer;

            if ((bodyLayerMask & m_PushLayers.value) == 0)
            {
                return;
            }

            if (hit.moveDirection.y < -0.3f)
            {
                return;
            }

            Vector3 pushDir = new Vector3(hit.moveDirection.x, 0.0f, hit.moveDirection.z);
            body.AddForce(pushDir * m_Strength, ForceMode.Impulse);
        }
    }
}