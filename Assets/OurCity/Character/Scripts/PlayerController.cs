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
        private float aimAngleY = 0;
        private Quaternion aimCameraOriginRotation=Quaternion.identity;

        private AnimatorStateInfo[] animatorStateInfos = new AnimatorStateInfo[4]; 

        private new void Start()
        {
            base.Start();
            aimCameraOriginRotation = aimCamera.transform.localRotation;
        }

        private new void Update()
        {
            base.Update();

            AimEvent();
        }

        public void AimEvent()
        {
            animatorStateInfos[1] = m_Animator.GetCurrentAnimatorStateInfo(1);
            animatorStateInfos[2] = m_Animator.GetCurrentAnimatorStateInfo(2);


            if (Input.GetKeyDown(KeyCode.F))
            {
                if (animatorStateInfos[1].IsTag("Unrelaxed"))
                {
                    m_Animator.SetBool("Relaxed", true);
                }
                else if(animatorStateInfos[1].IsTag("Relaxed"))
                {
                    m_Animator.SetBool("Relaxed", false);
                }

            }

            if (Input.GetMouseButtonDown(0)&& animatorStateInfos[1].IsTag("Relaxed"))
            {
                m_Animator.SetTrigger("Fire");
                model.SetGunFireDisplay();
                aimAngleY += 5;
            }

            if (Input.GetMouseButtonDown(1)&& animatorStateInfos[1].IsTag("Relaxed"))
            {
                m_Animator.SetBool("Aim", true);
                normalCamera.gameObject.SetActive(false);
                aimCamera.gameObject.SetActive(true);
                isAim = true;
            }
            if (Input.GetMouseButtonUp(1)&& animatorStateInfos[1].IsTag("Relaxed"))
            {
                m_Animator.SetBool("Aim", false);
                normalCamera.SetActive(true);
                aimCamera.SetActive(false);
                isAim = false;
            }
            if (Input.GetKeyDown(KeyCode.R)&& animatorStateInfos[1].IsTag("Relaxed")&&animatorStateInfos[2].IsTag("Relaxed"))
            {
                m_Animator.SetTrigger("Reload");
            }

            if(isAim)
            {
                float m = 2f;
                aimAngleY += Input.GetAxisRaw("Mouse Y")*5;
                if (aimAngleY > 90f / m)
                    aimAngleY = 90f / m;
                if (aimAngleY < -90f / m)
                    aimAngleY = -90f / m;
                //aimCamera.transform.localRotation =aimCameraOriginRotation * Quaternion.AngleAxis(aimAngleY, Vector3.left);
                m_Animator.SetFloat("AimAngle", aimAngleY * m);
                transform.localRotation *= Quaternion.AngleAxis(Input.GetAxis("Mouse X")*5f, Vector3.up);

            }
            
        }

    }

}

