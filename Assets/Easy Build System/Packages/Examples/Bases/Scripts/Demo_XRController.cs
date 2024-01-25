/// <summary>
/// Project : Easy Build System
/// Class : Demo_XRController.cs
/// Namespace : EasyBuildSystem.Examples.Bases.Scripts
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using UnityEngine;

#if EBS_XR
using Unity.XR.CoreUtils;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
#endif

namespace EasyBuildSystem.Examples.Bases.Scripts
{
    public class Demo_XRController : MonoBehaviour
    {
#if EBS_XR
    public DeviceBasedSnapTurnProvider DeviceBasedSnapTurnProvider;

    public float Speed = 1f;
    public XRNode InputSource;

    public float OffsetHeight = 0.3f;

    public float Gravity = -9.81f;
    private float FallingSpeed;
    public LayerMask GroundLayer;

    private XROrigin Rig;
    private Vector2 InputAxis;

    private CharacterController Controller;

    private void Awake()
    {
        Controller = GetComponent<CharacterController>();
        Rig = GetComponent<XROrigin>();
    }

    private void Update()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        InputDevice Device = InputDevices.GetDeviceAtXRNode(InputSource);
        Device.TryGetFeatureValue(CommonUsages.primary2DAxis, out InputAxis);
    }

    private void FixedUpdate()
    {
        CapsuleFollowHeadset();

        if (Packages.Addons.CircularBuildingMenu.UICircularBuildingMenu.Instance.IsOpened)
        {
            DeviceBasedSnapTurnProvider.enabled = false;
            return;
        }
        else
        {
            DeviceBasedSnapTurnProvider.enabled = true;
        }

        Quaternion HeadYaw = Quaternion.Euler(0, Rig.Camera.transform.eulerAngles.y, 0);
        Vector3 Direction = HeadYaw * new Vector3(InputAxis.x, 0f, InputAxis.y);
        Controller.Move(Direction * Speed * Time.fixedDeltaTime);

        if (IsGrounded())
            FallingSpeed = 0f;
        else
            FallingSpeed += Gravity * Time.fixedDeltaTime;

        Controller.Move(Vector3.up * FallingSpeed * Time.fixedDeltaTime);
    }

    private void CapsuleFollowHeadset()
    {
        Controller.height = Rig.CameraInOriginSpaceHeight + OffsetHeight * 0.9f;
        Vector3 CapsuleCenter = transform.InverseTransformPoint(Rig.Camera.transform.position);
        Controller.center = new Vector3(CapsuleCenter.x, Controller.height / 2f + Controller.skinWidth, CapsuleCenter.z);
    }

    private bool IsGrounded()
    {
        Vector3 Start = transform.TransformPoint(Controller.center);
        float Length = Controller.center.y + 0.01f;
        return Physics.SphereCast(Start, Controller.radius, Vector3.down, out RaycastHit Hit, Length, GroundLayer);
    }
#endif
    }
}