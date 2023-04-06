using UnityEngine;
using UnityEngine.AI;

namespace Battlehub.RTNavigation
{
    public class NavMeshDebugController : MonoBehaviour
    {
        public NavMeshAgent Agent;
        public Camera Camera;

        private void Start()
        {
            Agent = GetComponent<NavMeshAgent>();
            if(Camera == null)
            {
                Camera = Camera.main;
            }
        }

        private void Update()
        {
            if(Camera == null || Agent == null)
            {
                return;
            }

            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    Agent.SetDestination(hit.point);
                }
            }
        }
    }

}
