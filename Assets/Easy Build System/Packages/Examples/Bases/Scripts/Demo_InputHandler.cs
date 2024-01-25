/// <summary>
/// Project : Easy Build System
/// Class : Demo_InputHandler.cs
/// Namespace : EasyBuildSystem.Demos.Bases.Scripts
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using UnityEngine;

using EasyBuildSystem.Features.Runtime.Bases;
#if EBS_INPUT_SYSTEM_SUPPORT
using UnityEngine.InputSystem;
#endif
namespace EasyBuildSystem.Examples.Bases.Scripts
{
	public class Demo_InputHandler : Singleton<Demo_InputHandler>
	{
#if ENABLE_INPUT_SYSTEM && EBS_INPUT_SYSTEM_SUPPORT

		public Vector2 Move { get; set; }

		public Vector2 Look { get; set; }

		public bool Jump { get; set; }

		public bool Sprint { get; set; }

		InputActions.PlayerActions m_PlayerAction;
		InputActions m_InputAction;

		void OnEnable()
		{
			m_InputAction.Player.Enable();
		}

		void OnDisable()
		{
			m_InputAction.Player.Disable();
		}

		void OnDestroy()
		{
			m_InputAction.Player.Disable();
		}

		void Awake()
		{
			m_InputAction = new InputActions();
			m_PlayerAction = m_InputAction.Player;
		}

		void Update()
		{
#if !UNITY_ANDROID
			MoveInput(new Vector2(m_PlayerAction.Move.ReadValue<Vector2>().x, m_PlayerAction.Move.ReadValue<Vector2>().y));
			LookInput(new Vector2(m_PlayerAction.Look.ReadValue<Vector2>().x, -m_PlayerAction.Look.ReadValue<Vector2>().y));
			JumpInput(m_PlayerAction.Jump.triggered);
#endif

			Sprint = true;
        }

		public void MoveInput(Vector2 newMoveDirection)
		{
#if !UNITY_ANDROID
			if (Cursor.lockState == CursorLockMode.None)
			{
				Move = Vector2.zero;
				return;
			}
#endif

			Move = newMoveDirection;
		}

		public void LookInput(Vector2 newLookDirection)
		{
#if !UNITY_ANDROID
			if (Cursor.lockState == CursorLockMode.None)
			{
				Look = Vector2.zero;
				return;
			}
#endif

#if UNITY_ANDROID
		Look = newLookDirection / 160f;
#else
			Look = newLookDirection;
#endif
		}

		public void JumpInput(bool newJumpState)
		{
#if !UNITY_ANDROID
			if (Cursor.lockState == CursorLockMode.None)
			{
				Jump = false;
				return;
			}
#endif

			Jump = newJumpState;
		}

#else
		public Vector2 Move { get; set; }

	public Vector2 Look { get; set; }

	public bool Jump { get; set; }

	public bool Sprint { get; set; }


	void Update()
	{
#if !UNITY_ANDROID
		MoveInput(new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")));
		LookInput(new Vector2(Input.GetAxis("Mouse X"), -Input.GetAxis("Mouse Y")));
		JumpInput(Input.GetButton("Jump"));
#endif
		Sprint = true;
	}

	public void MoveInput(Vector2 newMoveDirection)
	{
#if !UNITY_ANDROID
		if (Cursor.lockState == CursorLockMode.None)
		{
			Move = Vector2.zero;
			return;
		}
#endif

		Move = newMoveDirection;
	}

	public void LookInput(Vector2 newLookDirection)
	{
#if !UNITY_ANDROID
		if (Cursor.lockState == CursorLockMode.None)
		{
			Look = Vector2.zero;
			return;
		}
#endif

#if UNITY_ANDROID
		Look = newLookDirection / 160f;
#else
		Look = newLookDirection;
#endif
	}

	public void JumpInput(bool newJumpState)
	{
#if !UNITY_ANDROID
		if (Cursor.lockState == CursorLockMode.None)
		{
			Jump = false;
			return;
		}
#endif

		Jump = newJumpState;
	}

#endif
		}
	}