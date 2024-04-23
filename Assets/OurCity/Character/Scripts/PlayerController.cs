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
        private float aimAngleY = 0;
        private Quaternion aimCameraOriginRotation=Quaternion.identity;

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
                float m = 2.5f;
                aimAngleY += Input.GetAxisRaw("Mouse Y")*5;
                if (aimAngleY > 90f / m)
                    aimAngleY = 90f / m;
                if (aimAngleY < -90f / m)
                    aimAngleY = -90f / m;
                aimCamera.transform.localRotation =aimCameraOriginRotation * Quaternion.AngleAxis(aimAngleY, Vector3.left);
                m_Animator.SetFloat("AimAngle", aimAngleY * m);
                transform.localRotation *= Quaternion.AngleAxis(Input.GetAxis("Mouse X")*5f, Vector3.up);

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
        public Transform lookAtTarget;  // 你想让角色上半身朝向的目标
        public float lookAtWeight = 1.0f;  // 朝向的强度，可以在运行时调整
        //void OnAnimatorIK(int layerIndex)
        //{
        //    if (lookAtTarget != null)
        //    {
        //        Debug.Log("执行朝向");
        //        m_Animator.SetLookAtWeight(lookAtWeight);
        //        m_Animator.SetLookAtPosition(lookAtTarget.position);
        //    }
        //}

    }

}

