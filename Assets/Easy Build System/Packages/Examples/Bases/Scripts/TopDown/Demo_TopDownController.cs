/// <summary>
/// Project : Easy Build System
/// Class : Demo_TopDownController.cs
/// Namespace : EasyBuildSystem.Examples.Bases.Scripts.TopDown
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using UnityEngine;
using UnityEngine.AI;

using EasyBuildSystem.Features.Runtime.Buildings.Placer;

namespace EasyBuildSystem.Examples.Bases.Scripts.TopDown
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class Demo_TopDownController : MonoBehaviour
    {
        [SerializeField] Animator m_Animator;

        NavMeshAgent m_Agent;

        void Awake()
        {
            m_Agent = GetComponent<NavMeshAgent>();

            BuildingPlacer.Instance.OnChangedBuildModeEvent.AddListener((BuildingPlacer.BuildMode mode) => 
            {
                if (mode == BuildingPlacer.BuildMode.NONE)
                {
                    m_Agent.isStopped = false;
                }
                else
                {
                    m_Agent.isStopped = true;
                }
            });
        }

        void Update()
        {
            m_Animator.SetFloat("Speed", m_Agent.velocity.magnitude);

#if EBS_INPUT_SYSTEM_SUPPORT
            if (UnityEngine.InputSystem.Mouse.current.leftButton.isPressed)
            {
                if (Physics.Raycast(Camera.main.ScreenPointToRay(new Vector3(UnityEngine.InputSystem.Mouse.current.position.ReadValue().x,
                    UnityEngine.InputSystem.Mouse.current.position.ReadValue().y, 0f)), out RaycastHit hit, Mathf.Infinity))
                {
                    m_Agent.destination = hit.point;
                }
            }
#else
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                if (Physics.Raycast(Camera.main.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f)), 
                    out RaycastHit hit, Mathf.Infinity))
                {
                    m_Agent.destination = hit.point;
                }
            }
#endif
        }
    }
}