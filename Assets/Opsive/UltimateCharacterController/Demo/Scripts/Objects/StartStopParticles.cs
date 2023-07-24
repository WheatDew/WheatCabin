/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Demo.Objects
{
    using UnityEngine;

    /// <summary>
    /// Starts or stop the ParticleSystem when the state is entered.
    /// </summary>
    public class StartStopParticles : StateMachineBehaviour
    {
        [Tooltip("Should the particles stop playing?")]
        [SerializeField] protected bool m_Stop;

        private ParticleSystem m_SmokeParticles;

        /// <summary>
        /// OnStateEnter is called when a transition starts and the state machine starts to evaluate this state. 
        /// </summary>
        /// <param name="animator"></param>
        /// <param name="stateInfo"></param>
        /// <param name="layerIndex"></param>
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (m_SmokeParticles == null) {
                m_SmokeParticles = animator.GetComponentInChildren<ParticleSystem>();
            }

            if (m_Stop) {
                m_SmokeParticles.Stop();
            } else {
                m_SmokeParticles.Play();
            }
        }
    }
}