using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace OurCity
{
    public class PlayerController : TPSController
    {
        public ModelController model;
        public GameObject normalCamera, aimCamera;

        private bool isAim=false;
        private bool isRelaxed = false;

        private Vector3 aimAngle=Vector3.zero;
        private Quaternion startAimAngle=Quaternion.identity;

        private new void Start()
        {
            base.Start();
            startAimAngle = aimCamera.transform.localRotation;
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
                if (!isRelaxed&&!m_Animator.GetBool("Relaxed"))
                {
                    m_Animator.SetBool("Relaxed", true);
                }
                else if(isRelaxed && m_Animator.GetBool("Relaxed"))
                {
                    m_Animator.SetBool("Relaxed", false);
                }

            }

            if (Input.GetMouseButtonDown(0)&&isRelaxed)
            {
                m_Animator.SetTrigger("Fire");
            }

            if (Input.GetMouseButtonDown(1)&&isRelaxed)
            {
                m_Animator.SetBool("Aim", true);
                normalCamera.gameObject.SetActive(false);
                aimCamera.gameObject.SetActive(true);
                isAim = true;
            }
            if (Input.GetMouseButtonUp(1)&&isRelaxed)
            {
                m_Animator.SetBool("Aim", false);
                normalCamera.SetActive(true);
                aimCamera.SetActive(false);
                isAim = false;
            }

            if(isAim)
            {
                float m = 1.7f;
                aimAngle += new Vector3(Input.GetAxisRaw("Mouse X") * 5, Input.GetAxisRaw("Mouse Y") * 5, 0);
                if (aimAngle.y > 90f/m)
                    aimAngle.y = 90f / m;
                if (aimAngle.y < -90f / m)
                    aimAngle.y = -90f / m;
                aimCamera.transform.localRotation = startAimAngle * Quaternion.AngleAxis(aimAngle.y, Vector3.left);
                m_Animator.SetFloat("AimAngle", aimAngle.y*m);
                //transform.rotation*= Quaternion.AngleAxis(aimAngle.x, Vector3.up);

            }
            
        }

        public void SetRelaxed()
        {
            isRelaxed = true;
        }

        public void SetNormal()
        {
            isRelaxed = false;
        }
    }
}

