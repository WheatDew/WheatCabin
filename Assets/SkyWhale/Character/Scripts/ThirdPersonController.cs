using UnityEngine;
using UnityEngine.EventSystems;

namespace SkyWhale
{

    [AddComponentMenu("")] // Don't display in add component menu
    [RequireComponent(typeof(AnimatorAddon))]
    public class ThirdPersonController : MonoBehaviour
    {
        public bool useCharacterForward = false;
        public bool lockToCameraForward = false;
        public float turnSpeed = 10f;
        public KeyCode sprintJoystick = KeyCode.JoystickButton2;
        public KeyCode sprintKeyboard = KeyCode.LeftShift;
        public KeyCode attackKeyboard = KeyCode.Mouse0;
        public KeyCode equipKeyboard = KeyCode.F;

        private float turnSpeedMultiplier;
        private float speed = 0f;
        private float direction = 0f;
        private bool isSprinting = false;
        private Animator anim;
        private Vector3 targetDirection;
        private Vector2 input;
        private Quaternion freeRotation;
        private Camera mainCamera;
        private float velocity;

        //状态指示
        [HideInInspector] public bool isBattling = false;
        AnimatorStateInfo stateInfo;

        // Use this for initialization
        void Start()
        {
            anim = GetComponent<Animator>();
            mainCamera = Camera.main;
        }

        private void Update()
        {
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                stateInfo = anim.GetCurrentAnimatorStateInfo(0);

                if (Input.GetKeyDown(equipKeyboard))
                {
                    if (isBattling)
                    {
                        anim.SetTrigger("Unarm");
                        isBattling = false;
                    }
                    else
                    {
                        anim.SetTrigger("Equip");
                        isBattling = true;
                    }
                }
                if (Input.GetKeyDown(attackKeyboard))
                {
                    if (isBattling)
                    {
                        anim.SetTrigger("Attack");
                    }
                }
            }

        }


        // Update is called once per frame
        void FixedUpdate()
        {
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                input.x = Input.GetAxis("Horizontal");
                input.y = Input.GetAxis("Vertical");

                // set speed to both vertical and horizontal inputs
                if (useCharacterForward)
                    speed = Mathf.Abs(input.x) + input.y;
                else
                    speed = Mathf.Abs(input.x) + Mathf.Abs(input.y);

                speed = Mathf.Clamp(speed, 0f, 1f);
                speed = Mathf.SmoothDamp(anim.GetFloat("Speed"), speed, ref velocity, 0.1f);
                anim.SetFloat("Speed", speed);

                if (input.y < 0f && useCharacterForward)
                    direction = input.y;
                else
                    direction = 0f;

                anim.SetFloat("Direction", direction);

                // set sprinting
                isSprinting = ((Input.GetKey(sprintJoystick) || Input.GetKey(sprintKeyboard)) && input != Vector2.zero && direction >= 0f);
                anim.SetBool("isSprinting", isSprinting);

                // Update target direction relative to the camera view (or not if the Keep Direction option is checked)
                UpdateTargetDirection();
                if (input != Vector2.zero && targetDirection.magnitude > 0.1f&&anim.GetCurrentAnimatorStateInfo(0).IsTag("Move"))
                {
                    Vector3 lookDirection = targetDirection.normalized;
                    freeRotation = Quaternion.LookRotation(lookDirection, transform.up);
                    var diferenceRotation = freeRotation.eulerAngles.y - transform.eulerAngles.y;
                    var eulerY = transform.eulerAngles.y;

                    if (diferenceRotation < 0 || diferenceRotation > 0) eulerY = freeRotation.eulerAngles.y;
                    var euler = new Vector3(0, eulerY, 0);

                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(euler), turnSpeed * turnSpeedMultiplier * Time.deltaTime);
                }
            }

            
        }

        //更新目标方向
        public virtual void UpdateTargetDirection()
        {
            if (!useCharacterForward)
            {
                turnSpeedMultiplier = 1f;
                var forward = mainCamera.transform.TransformDirection(Vector3.forward);
                forward.y = 0;

                //get the right-facing direction of the referenceTransform
                var right = mainCamera.transform.TransformDirection(Vector3.right);

                // determine the direction the player will face based on input and the referenceTransform's right and forward directions
                targetDirection = input.x * right + input.y * forward;
            }
            else
            {
                turnSpeedMultiplier = 0.2f;
                var forward = transform.TransformDirection(Vector3.forward);
                forward.y = 0;

                //get the right-facing direction of the referenceTransform
                var right = transform.TransformDirection(Vector3.right);
                targetDirection = input.x * right + Mathf.Abs(input.y) * forward;
            }
        }
    }

}