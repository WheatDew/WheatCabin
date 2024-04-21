using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace OurCity
{
    public class PlayerController : TPSController
    {
        public ModelController model;
        private new void Start()
        {
            base.Start();

        }

        private new void Update()
        {
            base.Update();

            AimEvent();
        }

        public void AimEvent()
        {
            

            if (Input.GetKeyDown(KeyCode.F))
            {
                if (m_Animator.GetBool("Aim"))
                {
                    m_Animator.SetBool("Aim", false);
                }
                else
                {
                    m_Animator.SetBool("Aim", true);
                }

            }

            if (Input.GetMouseButtonDown(0))
            {
                m_Animator.SetTrigger("Fire");
            }
            
        }
    }
}

