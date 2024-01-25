/// <summary>
/// Project : Easy Build System
/// Class : Demo_FirstPersonController.cs
/// Namespace : EasyBuildSystem.Examples.Bases.Scripts.FirstPerson
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using UnityEngine;

#if EBS_INPUT_SYSTEM_SUPPORT
using UnityEngine.InputSystem;
#endif

namespace EasyBuildSystem.Examples.Bases.Scripts.FirstPerson
{
	[RequireComponent(typeof(CharacterController))]
	public class Demo_FirstPersonController : MonoBehaviour
	{
		[Header("Movement Settings")]
		[SerializeField] float m_MoveSpeed = 4.0f;
		[SerializeField] float m_SprintSpeed = 6.0f;
		[SerializeField] float m_SpeedChangeRate = 10.0f;

		[Header("Jump & Gravity Settings")]
		[SerializeField] float m_JumpHeight = 1.2f;
		[SerializeField] float m_Gravity = -15.0f;
		[SerializeField] float m_JumpTimeout = 0.1f;
		[SerializeField] float m_FallTimeout = 0.15f;

		[Header("Grounded Settings")]
		[SerializeField] bool m_Grounded = true;
		[SerializeField] float m_GroundedOffset = -0.14f;
		[SerializeField] float m_GroundedRadius = 0.5f;
		[SerializeField] LayerMask m_GroundLayers;

		float m_Speed;
		float m_VerticalVelocity;
		readonly float m_TerminalVelocity = 53.0f;

		float m_JumpTimeoutDelta;
		float m_FallTimeoutDelta;

		CharacterController m_Controller;
		GameObject m_Camera;

		void Awake()
		{
			if (m_Camera == null)
			{
				m_Camera = Camera.main.gameObject;
			}
		}

		void Start()
		{
			m_Controller = GetComponent<CharacterController>();

			m_JumpTimeoutDelta = m_JumpTimeout;
			m_FallTimeoutDelta = m_FallTimeout;
		}

		void Update()
		{
			JumpAndGravity();
			GroundedCheck();
			Move();
		}

		void GroundedCheck()
		{
			Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - m_GroundedOffset, transform.position.z);
			m_Grounded = Physics.CheckSphere(spherePosition, m_GroundedRadius, m_GroundLayers, QueryTriggerInteraction.Ignore);
		}

		void Move()
		{
			float targetSpeed = Demo_InputHandler.Instance.Sprint ? m_SprintSpeed : m_MoveSpeed;

			if (Demo_InputHandler.Instance.Move == Vector2.zero)
			{
				targetSpeed = 0.0f;
			}

			float currentHorizontalSpeed = new Vector3(m_Controller.velocity.x, 0.0f, m_Controller.velocity.z).magnitude;

			float speedOffset = 0.1f;
			float inputMagnitude = 1f;//m_InputHandler.IsAnalogMovement ? m_InputHandler.Move.magnitude : 1f;

			if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
			{
				m_Speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * m_SpeedChangeRate);
				m_Speed = Mathf.Round(m_Speed * 1000f) / 1000f;
			}
			else
			{
				m_Speed = targetSpeed;
			}

			Vector3 inputDirection = new Vector3(Demo_InputHandler.Instance.Move.x, 0.0f, Demo_InputHandler.Instance.Move.y).normalized;

			if (Demo_InputHandler.Instance.Move != Vector2.zero)
			{
				inputDirection = transform.right * Demo_InputHandler.Instance.Move.x + transform.forward * Demo_InputHandler.Instance.Move.y;
			}

			m_Controller.Move(inputDirection.normalized * (m_Speed * Time.deltaTime) + new Vector3(0.0f, m_VerticalVelocity, 0.0f) * Time.deltaTime);
		}

		private void JumpAndGravity()
		{
			if (m_Grounded)
			{
				m_FallTimeoutDelta = m_FallTimeout;

				if (m_VerticalVelocity < 0.0f)
				{
					m_VerticalVelocity = -2f;
				}

				if (Demo_InputHandler.Instance.Jump && m_JumpTimeoutDelta <= 0.0f)
				{
					m_VerticalVelocity = Mathf.Sqrt(m_JumpHeight * -2f * m_Gravity);
				}

				if (m_JumpTimeoutDelta >= 0.0f)
				{
					m_JumpTimeoutDelta -= Time.deltaTime;
				}
			}
			else
			{
				m_JumpTimeoutDelta = m_JumpTimeout;

				if (m_FallTimeoutDelta >= 0.0f)
				{
					m_FallTimeoutDelta -= Time.deltaTime;
				}

				Demo_InputHandler.Instance.Jump = false;
			}

			if (m_VerticalVelocity < m_TerminalVelocity)
			{
				m_VerticalVelocity += m_Gravity * Time.deltaTime;
			}
		}

		void OnDrawGizmosSelected()
		{
			Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
			Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

			if (m_Grounded)
			{
				Gizmos.color = transparentGreen;
			}
			else
			{
				Gizmos.color = transparentRed;
			}

			Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - m_GroundedOffset, transform.position.z), m_GroundedRadius);
		}
	}
}