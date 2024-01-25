/// <summary>
/// Project : Easy Build System
/// Class : Demo_ThirdPersonController.cs
/// Namespace : EasyBuildSystem.Examples.Bases.Scripts.ThirdPerson
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using UnityEngine;

namespace EasyBuildSystem.Examples.Bases.Scripts.ThirdPerson
{
	[RequireComponent(typeof(CharacterController))]
	public class Demo_ThirdPersonController : MonoBehaviour
	{
		[Header("Movement Settings")]
		[SerializeField] float m_MoveSpeed = 2.0f;
		[SerializeField] float m_SprintSpeed = 5.335f;
		[Range(0.0f, 0.3f)]
		[SerializeField] float m_RotationSmoothTime = 0.12f;
		[SerializeField] float m_SpeedChangeRate = 10.0f;

		[Header("Jump & Gravity Settings")]
		[SerializeField] float m_JumpHeight = 1.2f;
		[SerializeField] float m_Gravity = -15.0f;
		[SerializeField] float m_JumpTimeout = 0.50f;
		[SerializeField] float m_FallTimeout = 0.15f;

		[Header("Grounded Settings")]
		[SerializeField] bool m_Grounded = true;
		[SerializeField] float m_GroundedOffset = -0.14f;
		[SerializeField] float m_GroundedRadius = 0.28f;
		[SerializeField] LayerMask m_GroundLayers;

		float m_Speed;
		float m_AnimationBlend;
		float m_TargetRotation = 0.0f;
		float m_RotationVelocity;
		float m_VerticalVelocity;
		readonly float m_TerminalVelocity = 53.0f;

		float m_JumpTimeoutDelta;
		float m_FallTimeoutDelta;

		int m_AnimIDSpeed;
		int m_AnimIDGrounded;
		int m_AnimIDJump;
		int m_AnimIDFreeFall;
		int m_AnimIDMotionSpeed;

		Animator m_Animator;
		CharacterController m_Controller;
		GameObject m_Camera;

		bool m_HasAnimator;

		void Awake()
		{
			if (m_Camera == null)
			{
				m_Camera = Camera.main.gameObject;
			}
		}

		void Start()
		{
			m_HasAnimator = TryGetComponent(out m_Animator);
			m_Controller = GetComponent<CharacterController>();

			AssignAnimationIDs();

			m_JumpTimeoutDelta = m_JumpTimeout;
			m_FallTimeoutDelta = m_FallTimeout;
		}

		void Update()
		{
			m_HasAnimator = TryGetComponent(out m_Animator);

			JumpAndGravity();
			GroundedCheck();
			Move();
		}

		void AssignAnimationIDs()
		{
			m_AnimIDSpeed = Animator.StringToHash("Speed");
			m_AnimIDGrounded = Animator.StringToHash("Grounded");
			m_AnimIDJump = Animator.StringToHash("Jump");
			m_AnimIDFreeFall = Animator.StringToHash("FreeFall");
			m_AnimIDMotionSpeed = Animator.StringToHash("MotionSpeed");
		}

		void GroundedCheck()
		{
			Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - m_GroundedOffset, transform.position.z);
			m_Grounded = Physics.CheckSphere(spherePosition, m_GroundedRadius, m_GroundLayers, QueryTriggerInteraction.Ignore);

			if (m_HasAnimator)
			{
				m_Animator.SetBool(m_AnimIDGrounded, m_Grounded);
			}
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
			float inputMagnitude = Demo_InputHandler.Instance.Move.magnitude;//m_AssetsInputs.IsAnalogMovement ? m_AssetsInputs.Move.magnitude : 1f;

			if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
			{
				m_Speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * m_SpeedChangeRate);
				m_Speed = Mathf.Round(m_Speed * 1000f) / 1000f;
			}
			else
			{
				m_Speed = targetSpeed;
			}

			m_AnimationBlend = Mathf.Lerp(m_AnimationBlend, targetSpeed, Time.deltaTime * m_SpeedChangeRate);

			Vector3 inputDirection = new Vector3(Demo_InputHandler.Instance.Move.x, 0.0f, Demo_InputHandler.Instance.Move.y).normalized;

			if (Demo_InputHandler.Instance.Move != Vector2.zero)
			{
				m_TargetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + m_Camera.transform.eulerAngles.y;
				float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, m_TargetRotation, ref m_RotationVelocity, m_RotationSmoothTime);

				transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
			}

			Vector3 targetDirection = Quaternion.Euler(0.0f, m_TargetRotation, 0.0f) * Vector3.forward;

			m_Controller.Move(targetDirection.normalized * (m_Speed * Time.deltaTime) + new Vector3(0.0f, m_VerticalVelocity, 0.0f) * Time.deltaTime);

			if (m_HasAnimator)
			{
				m_Animator.SetFloat(m_AnimIDSpeed, m_AnimationBlend);
				m_Animator.SetFloat(m_AnimIDMotionSpeed, inputMagnitude);
			}
		}

		void JumpAndGravity()
		{
			if (m_Grounded)
			{
				m_FallTimeoutDelta = m_FallTimeout;

				if (m_HasAnimator)
				{
					m_Animator.SetBool(m_AnimIDJump, false);
					m_Animator.SetBool(m_AnimIDFreeFall, false);
				}

				if (m_VerticalVelocity < 0.0f)
				{
					m_VerticalVelocity = -2f;
				}

				if (Demo_InputHandler.Instance.Jump && m_JumpTimeoutDelta <= 0.0f)
				{
					m_VerticalVelocity = Mathf.Sqrt(m_JumpHeight * -2f * m_Gravity);

					if (m_HasAnimator)
					{
						m_Animator.SetBool(m_AnimIDJump, true);
					}
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
				else
				{
					if (m_HasAnimator)
					{
						m_Animator.SetBool(m_AnimIDFreeFall, true);
					}
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