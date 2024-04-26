using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace OurCity
{
    public class PlayerController : TPSController
    {
        public ModelController model;
        public GameObject normalCamera;
        public Camera realAimCamera,realFreeCamera;
        public WeaponCamera aimCamera;

        private bool isAim=false;
        private float aimAngleY = 0;
        private Quaternion aimCameraOriginRotation=Quaternion.identity;

        private AnimatorStateInfo[] animatorStateInfos = new AnimatorStateInfo[4];

        private int ammoMax = 8;
        private int ammoCurrent = 8;

        private bool isReload = true;

        public UnityEvent onDeath;

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
                    m_MoveSpeed = 1;
                }
                else if(animatorStateInfos[1].IsTag("Relaxed"))
                {
                    m_Animator.SetBool("Relaxed", false);
                    m_MoveSpeed = 2;
                }

            }

            if (Input.GetMouseButtonDown(0)&& animatorStateInfos[1].IsTag("Relaxed"))
            {
                if (ammoCurrent > 0)
                {
                    ammoCurrent--;
                    m_Animator.SetTrigger("Fire");
                    Ray ray = realAimCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
                    RaycastHit[] result = Physics.RaycastAll(ray);

                    for (int i = 0; i < result.Length; i++)
                    {
                        if (i >= 2)
                            break;
                        if (result[i].collider.tag == "Character" && result[i].collider.gameObject != gameObject)
                        {
                            result[i].collider.GetComponent<OurCityEnemy>().Death();
                        }
                    }
                    model.SetGunFireDisplay();
                    aimAngleY += 5;
                    aimCamera.beadFixed = 1f;
                }

            }

            if (Input.GetMouseButtonDown(1)&& animatorStateInfos[1].IsTag("Relaxed"))
            {
                aimCamera.transform.position = realFreeCamera.transform.position;
                aimCamera.transform.rotation = realFreeCamera.transform.rotation;
                m_Animator.SetBool("Aim", true);
                normalCamera.gameObject.SetActive(false);
                aimCamera.gameObject.SetActive(true);
                isMove = false;
                isAim = true;
                m_Animator.SetFloat("Speed", 0);

            }
            if (Input.GetMouseButtonUp(1)&& animatorStateInfos[1].IsTag("Relaxed"))
            {
                m_Animator.SetBool("Aim", false);
                normalCamera.SetActive(true);
                aimCamera.gameObject.SetActive(false);
                isMove = true;
                isAim = false;
            }
            if (Input.GetKeyDown(KeyCode.R)&& animatorStateInfos[1].IsTag("Relaxed")&&animatorStateInfos[2].IsTag("Relaxed"))
            {
                m_Animator.SetTrigger("Reload");
                isReload = true;
            }

            if (isReload&& animatorStateInfos[1].IsTag("Relaxed"))
            {
                isReload = false;
                ammoCurrent = ammoMax;
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

            GamePlaySystem.s.AmmoDisplay(ammoCurrent, ammoMax);
        }

        public void Death()
        {
            m_Animator.SetTrigger("Death");
            gameObject.layer = 0;
            onDeath.Invoke();
            normalCamera.SetActive(true);
            aimCamera.gameObject.SetActive(false);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            this.enabled = false;
        }

    }

}

