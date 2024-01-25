/// <summary>
/// Project : Easy Build System
/// Class : BuildingPlacerInput.cs
/// Namespace : EasyBuildSystem.Features.Runtime.Buildings.Placer.InputHandler
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using System;

using UnityEngine;

using EasyBuildSystem.Features.Runtime.Buildings.Part;
using EasyBuildSystem.Features.Runtime.Buildings.Manager;

using EasyBuildSystem.Features.Runtime.Extensions;

namespace EasyBuildSystem.Features.Runtime.Buildings.Placer.InputHandler
{
    public class StandaloneInputHandler : BaseInputHandler
    {
#if EBS_INPUT_SYSTEM_SUPPORT

        #region Fields

        [Serializable]
        public class InputSettings
        {
            [SerializeField] bool m_BlockWhenCursorOverUI = false;
            public bool BlockWhenCursorOverUI { get { return m_BlockWhenCursorOverUI; } }

            [SerializeField] bool m_CanRotateBuildingPart = true;
            public bool CanRotateBuildingPart { get { return m_CanRotateBuildingPart; } }

            [SerializeField] bool m_CanSelectBuildingPart = true;
            public bool CanSelectBuildingPart { get { return m_CanSelectBuildingPart; } }

            [SerializeField] bool m_UsePlacingModeShortcut = true;
            public bool UsePlacingModeShortcut { get { return m_UsePlacingModeShortcut; } set { m_UsePlacingModeShortcut = value; } }

            [SerializeField] bool m_ResetModeAfterPlacing = false;
            public bool ResetModeAfterPlacing { get { return m_ResetModeAfterPlacing; } }

            [SerializeField] bool m_UseEditingModeShortcut = true;
            public bool UseEditingModeShortcut { get { return m_UseEditingModeShortcut; } set { m_UseEditingModeShortcut = value; } }

            [SerializeField] bool m_ResetModeAfterEditing = false;
            public bool ResetModeAfterEditing { get { return m_ResetModeAfterEditing; } }

            [SerializeField] bool m_UseDestroyingModeShortcut = true;
            public bool UseDestroyingModeShortcut { get { return m_UseDestroyingModeShortcut; } set { m_UseDestroyingModeShortcut = value; } }

            [SerializeField] bool m_ResetModeAfterDestroying = false;
            public bool ResetModeAfterDestroying { get { return m_ResetModeAfterDestroying; } }
        }

        [SerializeField] InputSettings m_InputSettings;

        public InputSettings GetInputSettings { get { return m_InputSettings; } set { m_InputSettings = value; } }

        InputActions m_InputAction;

        [SerializeField] InputActions.BuildingActions m_BuildingInputAction;
        public InputActions.BuildingActions BuildingInputAction { get { return m_BuildingInputAction; } }

        float m_LastActionTime;
        int m_SelectionIndex;

        #endregion

        #region Unity Methods

        void OnEnable()
        {
            m_InputAction.Building.Enable();
        }

        void OnDisable()
        {
            m_InputAction.Building.Disable();
        }

        void OnDestroy()
        {
            m_InputAction.Building.Disable();
        }

        void Awake()
        {
            m_InputAction = new InputActions();
            m_BuildingInputAction = m_InputAction.Building;
        }

        void Update()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (m_InputSettings.BlockWhenCursorOverUI)
            {
                if (UIExtension.IsPointerOverUIElement() && Cursor.lockState != CursorLockMode.Locked)
                {
                    return;
                }
            }

            HandleBuildModes();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Handle the building part selection and update the build modes.
        /// </summary>
        void HandleBuildModes()
        {
            if (m_InputSettings.UsePlacingModeShortcut)
            {
                if (BuildingInputAction.Placement.triggered)
                {
                    Placer.ChangeBuildMode(BuildingPlacer.BuildMode.PLACE);
                }
            }

            if (m_InputSettings.UseDestroyingModeShortcut)
            {
                if (BuildingInputAction.Destruction.triggered)
                {
                    Placer.ChangeBuildMode(BuildingPlacer.BuildMode.DESTROY);
                }
            }

            if (m_InputSettings.UseEditingModeShortcut)
            {
                if (BuildingInputAction.Edition.triggered)
                {
                    Placer.ChangeBuildMode(BuildingPlacer.BuildMode.EDIT);
                }
            }

            if (Placer.GetBuildMode == BuildingPlacer.BuildMode.NONE)
            {
                HandleBuildingPartSelection();
            }
            else
            {
                if (Placer.GetBuildMode == BuildingPlacer.BuildMode.PLACE)
                {
                    HandlePlacingMode();
                }

                if (Placer.GetBuildMode == BuildingPlacer.BuildMode.DESTROY)
                {
                    HandleDestroyMode();
                }

                if (Placer.GetBuildMode == BuildingPlacer.BuildMode.EDIT)
                {
                    HandleEditingMode();
                }
            }
        }

        /// <summary>
        /// Handle placing mode according to the user inputs.
        /// </summary>
        void HandlePlacingMode()
        {
            if (BuildingInputAction.Validate.triggered)
            {
                if (Placer.PlacingBuildingPart())
                {
                    m_LastActionTime = Time.time;

                    if (m_InputSettings.ResetModeAfterPlacing)
                    {
                        Placer.ChangeBuildMode(BuildingPlacer.BuildMode.NONE);
                    }

                    if (m_InputSettings.ResetModeAfterEditing && Placer.LastBuildMode == BuildingPlacer.BuildMode.EDIT)
                    {
                        Placer.ChangeBuildMode(BuildingPlacer.BuildMode.NONE);
                    }
                }
            }

            if (m_InputSettings.CanRotateBuildingPart)
            {
                if (BuildingInputAction.Scroll.triggered)
                {
                    float wheelAxis = BuildingInputAction.Scroll.ReadValue<float>();

                    if (wheelAxis > 0)
                    {
                        Placer.RotatePreview();
                    }
                    else if (wheelAxis < 0)
                    {
                        Placer.RotatePreview(true);
                    }
                }
            }

            if (BuildingInputAction.Cancel.triggered)
            {
                Placer.ChangeBuildMode(BuildingPlacer.BuildMode.NONE);
            }
        }

        /// <summary>
        /// Handle destroy mode according to the user inputs.
        /// </summary>
        void HandleDestroyMode()
        {
            if (BuildingInputAction.Validate.triggered)
            {
                if (Placer.DestroyBuildingPart())
                {
                    m_LastActionTime = Time.time;

                    if (m_InputSettings.ResetModeAfterDestroying)
                    {
                        Placer.ChangeBuildMode(BuildingPlacer.BuildMode.NONE);
                    }
                }
            }

            if (BuildingInputAction.Cancel.triggered)
            {
                Placer.ChangeBuildMode(BuildingPlacer.BuildMode.NONE);
            }
        }

        /// <summary>
        /// Handle editing mode according the user inputs.
        /// </summary>
        void HandleEditingMode()
        {
            if (BuildingInputAction.Validate.triggered)
            {
                Placer.EditingBuildingPart();
            }

            if (BuildingInputAction.Cancel.triggered)
            {
                Placer.ChangeBuildMode(BuildingPlacer.BuildMode.NONE);
            }
        }

        /// <summary>
        /// Handle the building part selection according the user inputs.
        /// </summary>
        void HandleBuildingPartSelection()
        {
            if (m_InputSettings.CanSelectBuildingPart)
            {
                if (BuildingInputAction.Scroll.triggered)
                {
                    float wheelAxis = BuildingInputAction.Scroll.ReadValue<float>();

                    if (wheelAxis > 0)
                    {
                        if (m_SelectionIndex < BuildingManager.Instance.BuildingPartReferences.Count - 1)
                        {
                            m_SelectionIndex++;
                            Placer.SelectBuildingPart(BuildingManager.Instance.BuildingPartReferences[m_SelectionIndex]);
                        }
                        else
                        {
                            m_SelectionIndex = 0;
                            Placer.SelectBuildingPart(BuildingManager.Instance.BuildingPartReferences[m_SelectionIndex]);
                        }
                    }
                    else if (wheelAxis < 0)
                    {
                        if (m_SelectionIndex > 0)
                        {
                            m_SelectionIndex--;
                            Placer.SelectBuildingPart(BuildingManager.Instance.BuildingPartReferences[m_SelectionIndex]);
                        }
                        else
                        {
                            m_SelectionIndex = BuildingManager.Instance.BuildingPartReferences.Count - 1;
                            Placer.SelectBuildingPart(BuildingManager.Instance.BuildingPartReferences[m_SelectionIndex]);
                        }
                    }
                }
            }
        }

        #endregion

#else
        #region Fields

        [Serializable]
        public class InputSettings
        {
            [SerializeField] bool m_BlockWhenCursorOverUI = false;
            public bool BlockWhenCursorOverUI { get { return m_BlockWhenCursorOverUI; } }

            [SerializeField] bool m_CanRotateBuildingPart = true;
            public bool CanRotateBuildingPart { get { return m_CanRotateBuildingPart; } }

            [SerializeField] KeyCode m_RotateActionKey = KeyCode.R;
            public KeyCode RotateActionKey { get { return m_RotateActionKey; } }

            [SerializeField] bool m_CanSelectBuildingPart = true;
            public bool CanSelectBuildingPart { get { return m_CanSelectBuildingPart; } }

            [SerializeField] KeyCode m_ValidateActionKey = KeyCode.Mouse0;
            public KeyCode ValidateActionKey { get { return m_ValidateActionKey; } }

            [SerializeField] KeyCode m_CancelActionKey = KeyCode.Mouse1;
            public KeyCode CancelActionKey { get { return m_CancelActionKey; } }

            [SerializeField] bool m_UsePlacingModeShortcut = true;
            public bool UsePlacingModeShortcut { get { return m_UsePlacingModeShortcut; } set { m_UsePlacingModeShortcut = value; } }

            [SerializeField] KeyCode m_PlacingModeKey = KeyCode.E;
            public KeyCode PlacingModeKey { get { return m_PlacingModeKey; } }

            [SerializeField] bool m_ResetModeAfterPlacing = false;
            public bool ResetModeAfterPlacing { get { return m_ResetModeAfterPlacing; } }

            [SerializeField] bool m_UseEditingModeShortcut = true;
            public bool UseEditingModeShortcut { get { return m_UseEditingModeShortcut; } set { m_UseEditingModeShortcut = value; } }

            [SerializeField] KeyCode m_EditingModeKey = KeyCode.T;
            public KeyCode EditingModeKey { get { return m_EditingModeKey; } }

            [SerializeField] bool m_ResetModeAfterEditing = false;
            public bool ResetModeAfterEditing { get { return m_ResetModeAfterEditing; } }

            [SerializeField] bool m_UseDestroyingModeShortcut = true;
            public bool UseDestroyingModeShortcut { get { return m_UseDestroyingModeShortcut; } set { m_UseDestroyingModeShortcut = value; } }

            [SerializeField] KeyCode m_DestroyingModeKey = KeyCode.R;
            public KeyCode DestroyingModeKey { get { return m_DestroyingModeKey; } }

            [SerializeField] bool m_ResetModeAfterDestroying = false;
            public bool ResetModeAfterDestroying { get { return m_ResetModeAfterDestroying; } }
        }
        [SerializeField] InputSettings m_InputSettings = new InputSettings();
        public InputSettings GetInputSettings { get { return m_InputSettings; } set { m_InputSettings = value; } }

        int m_SelectionIndex;

        #endregion

        #region Unity Methods

        void Start()
        {
#if ENABLE_INPUT_SYSTEM && !EBS_INPUT_SYSTEM_SUPPORT
            Debug.LogWarning("<b>Easy Build System</b> : New Unity Input System support package is required!\n" +
                "Please import the Unity Input System support package from the Package Importer.");
#endif
        }

        void Update()
        {
            if (Placer == null)
            {
                return;
            }

            if (!Application.isPlaying)
            {
                return;
            }

            if (m_InputSettings.BlockWhenCursorOverUI)
            {
                if (UIExtension.IsPointerOverUIElement() && Cursor.lockState != CursorLockMode.Locked)
                {
                    return;
                }
            }

            HandleBuildModes();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Handle the building part selection and update the build modes.
        /// </summary>
        void HandleBuildModes()
        {
#if UNITY_ANDROID
            return;
#else

            if (m_InputSettings.UsePlacingModeShortcut)
            {
                if (Input.GetKeyDown(m_InputSettings.PlacingModeKey))
                {
                    Placer.ChangeBuildMode(BuildingPlacer.BuildMode.PLACE);
                }
            }

            if (m_InputSettings.UseDestroyingModeShortcut)
            {
                if (Input.GetKeyDown(m_InputSettings.DestroyingModeKey))
                {
                    Placer.ChangeBuildMode(BuildingPlacer.BuildMode.DESTROY);
                }
            }

            if (m_InputSettings.UseEditingModeShortcut)
            {
                if (Input.GetKeyDown(m_InputSettings.EditingModeKey))
                {
                    Placer.ChangeBuildMode(BuildingPlacer.BuildMode.EDIT);
                }
            }

            if (Placer.GetBuildMode == BuildingPlacer.BuildMode.NONE)
            {
                HandleBuildingPartSelection();
            }
            else
            {
                if (Placer.GetBuildMode == BuildingPlacer.BuildMode.PLACE)
                {
                    HandlePlacingMode();
                }

                if (Placer.GetBuildMode == BuildingPlacer.BuildMode.DESTROY)
                {
                    HandleDestroyMode();
                }

                if (Placer.GetBuildMode == BuildingPlacer.BuildMode.EDIT)
                {
                    HandleEditingMode();
                }
            }
#endif
        }

        /// <summary>
        /// Handle placing mode according to the user inputs.
        /// </summary>
        void HandlePlacingMode()
        {
            if (Input.GetKeyDown(m_InputSettings.ValidateActionKey))
            {
                if (Placer.PlacingBuildingPart())
                {
                    if (m_InputSettings.ResetModeAfterPlacing)
                    {
                        Placer.ChangeBuildMode(BuildingPlacer.BuildMode.NONE);
                    }

                    if (m_InputSettings.ResetModeAfterEditing && Placer.LastBuildMode == BuildingPlacer.BuildMode.EDIT)
                    {
                        Placer.ChangeBuildMode(BuildingPlacer.BuildMode.NONE);
                    }
                }
            }

            if (m_InputSettings.CanRotateBuildingPart)
            {
                float wheelAxis = Input.GetAxis("Mouse ScrollWheel");

                if (wheelAxis > 0)
                {
                    Placer.RotatePreview();
                }
                else if (wheelAxis < 0)
                {
                    Placer.RotatePreview(true);
                }
            }
            else
            {
                if (Input.GetKeyDown(m_InputSettings.RotateActionKey))
                {
                    Placer.RotatePreview(true);
                }
            }

            if (Input.GetKeyDown(m_InputSettings.CancelActionKey))
            {
                Placer.ChangeBuildMode(BuildingPlacer.BuildMode.NONE);
            }
        }

        /// <summary>
        /// Handle destroy mode according to the user inputs.
        /// </summary>
        void HandleDestroyMode()
        {
            if (Input.GetKeyDown(m_InputSettings.ValidateActionKey))
            {
                if (Placer.DestroyBuildingPart())
                {
                    if (m_InputSettings.ResetModeAfterDestroying)
                    {
                        Placer.ChangeBuildMode(BuildingPlacer.BuildMode.NONE);
                    }
                }
            }

            if (Input.GetKeyDown(m_InputSettings.CancelActionKey))
            {
                Placer.ChangeBuildMode(BuildingPlacer.BuildMode.NONE);
            }
        }

        /// <summary>
        /// Handle editing mode according the user inputs.
        /// </summary>
        void HandleEditingMode()
        {
            if (Input.GetKeyDown(m_InputSettings.ValidateActionKey))
            {
                Placer.EditingBuildingPart();
            }

            if (Input.GetKeyDown(m_InputSettings.CancelActionKey))
            {
                Placer.ChangeBuildMode(BuildingPlacer.BuildMode.NONE);
            }
        }

        /// <summary>
        /// Handle the building part selection according the user inputs.
        /// </summary>
        void HandleBuildingPartSelection()
        {
            if (BuildingManager.Instance == null)
            {
                return;
            }

            if (m_InputSettings.CanSelectBuildingPart)
            {
                float wheelAxis = Input.GetAxis("Mouse ScrollWheel");
                BuildingPart[] buildingParts = BuildingManager.Instance.BuildingPartReferences.ToArray();

                if (wheelAxis > 0)
                {
                    if (m_SelectionIndex < buildingParts.Length - 1)
                    {
                        m_SelectionIndex++;
                        Placer.SelectBuildingPart(buildingParts[m_SelectionIndex]);
                    }
                    else
                    {
                        m_SelectionIndex = 0;
                        Placer.SelectBuildingPart(buildingParts[m_SelectionIndex]);
                    }
                }
                else if (wheelAxis < 0)
                {
                    if (m_SelectionIndex > 0)
                    {
                        m_SelectionIndex--;
                        Placer.SelectBuildingPart(buildingParts[m_SelectionIndex]);
                    }
                    else
                    {
                        m_SelectionIndex = buildingParts.Length - 1;
                        Placer.SelectBuildingPart(buildingParts[m_SelectionIndex]);
                    }
                }
            }
        }

        #endregion
#endif
    }
}