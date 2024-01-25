/// <summary>
/// Project : Easy Build System
/// Class : Demo_UIAndroidControls.cs
/// Namespace : EasyBuildSystem.Examples.Bases.Scripts.UI.Mobile
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using UnityEngine;

using EasyBuildSystem.Features.Runtime.Buildings.Placer;
using EasyBuildSystem.Features.Runtime.Buildings.Placer.InputHandler;

namespace EasyBuildSystem.Examples.Bases.Scripts.UI.Mobile
{
    public class Demo_UIAndroidControls : MonoBehaviour
    {
        [SerializeField] GameObject m_UIValidateButton;
        [SerializeField] GameObject m_UIRotateButton;
        [SerializeField] GameObject m_UICancelButton;

        AndroidInputHandler m_AndroidInputHandler;

        void Awake()
        {
            m_AndroidInputHandler = FindObjectOfType<AndroidInputHandler>();
        }

        void Update()
        {
            if (BuildingPlacer.Instance.GetBuildMode == BuildingPlacer.BuildMode.PLACE)
            {
                m_UIValidateButton.SetActive(true);
                m_UIRotateButton.SetActive(true);
                m_UICancelButton.SetActive(true);
            }
            else if (BuildingPlacer.Instance.GetBuildMode == BuildingPlacer.BuildMode.DESTROY ||
                BuildingPlacer.Instance.GetBuildMode == BuildingPlacer.BuildMode.EDIT)
            {
                m_UIValidateButton.SetActive(true);
                m_UIRotateButton.SetActive(false);
                m_UICancelButton.SetActive(true);
            }
            else
            {
                m_UIValidateButton.SetActive(false);
                m_UIRotateButton.SetActive(false);
                m_UICancelButton.SetActive(false);
            }
        }

        public void VirtualMoveInput(Vector2 virtualMoveDirection)
        {
            Demo_InputHandler.Instance.MoveInput(virtualMoveDirection);
        }

        public void VirtualLookInput(Vector2 virtualLookDirection)
        {
            Demo_InputHandler.Instance.LookInput(virtualLookDirection);
        }

        public void VirtualJumpInput(bool virtualJumpState)
        {
            Demo_InputHandler.Instance.JumpInput(virtualJumpState);
        }

        public void VirtualValidateBuildingAction()
        {
            m_AndroidInputHandler.HandleBuildModes();
        }

        public void VirtualRotateBuildingAction()
        {
            m_AndroidInputHandler.RotatePreview();
        }

        public void VirtualCancelBuildingAction()
        {
            m_AndroidInputHandler.CancelBuildMode();
        }
    }
}
