using UnityEngine;
using Battlehub.Cubeman;
using Battlehub.RTCommon;
namespace Battlehub.Cubeman
{ 
    [DisallowMultipleComponent]
    public class GameCharacter : MonoBehaviour
    {
        public CubemenGame Game;
        private Rigidbody m_rigidBody;
        private CubemanUserControl m_userControl;
        private Transform m_soul;
        private SkinnedMeshRenderer m_skinnedMeshRenderer;

        public Transform Camera
        {
            get { return m_userControl.Cam; }
            set { m_userControl.Cam = value; }
        }

        public bool HandleInput
        {
            get { return m_userControl.HandleInput; }
            set
            {
                m_userControl.HandleInput = value;
            }
        }

        private bool m_isActive;
        public bool IsActive
        {
            get { return m_isActive; }
            set
            {
                m_isActive = value;
                Rigidbody rig = gameObject.GetComponent<Rigidbody>();
                if (rig)
                {
                    rig.isKinematic = !m_isActive;
                }

                CubemanCharacter cubemanCharacter = gameObject.GetComponent<CubemanCharacter>();
                if (cubemanCharacter)
                {
                    cubemanCharacter.Enabled = m_isActive;
                }
            }
        }

        private void Awake()
        {
            m_userControl = GetComponent<CubemanUserControl>();
        }

        private void Start()
        {
            m_soul = transform.Find("Soul");
            m_skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
            m_rigidBody = GetComponent<Rigidbody>();

            if (Game == null)
            {
                Game = FindObjectOfType<CubemenGame>();
            }
        }

        private void Update()
        {
            if (Game == null)
            {
                return;
            }

            if (!Game.IsGameRunning)
            {
                return;
            }

            if (transform.position.y < -25.0f)
            {
                Die();
            }

            if(Input.GetKeyDown(KeyCode.K))
            {
                Die();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if(!Game.IsGameRunning)
            {
                return;
            }

            if(other.tag == "Finish")
            {
                Game.OnPlayerFinish(this);
                if (m_skinnedMeshRenderer != null)
                {
                    m_skinnedMeshRenderer.enabled = false;
                }
                if (m_soul != null)
                {
                    m_soul.gameObject.SetActive(true);
                }
                Destroy(gameObject, 2);
            }
        }

        private void Die()
        {
            enabled = false;
            Game.OnPlayerDie(this);
            if(m_skinnedMeshRenderer != null)
            {
                m_skinnedMeshRenderer.enabled = false;
            }
            if(m_rigidBody != null)
            {
                m_rigidBody.isKinematic = true;
            }
            if(m_userControl != null)
            {
                m_userControl.HandleInput = false;
            }
            if(m_soul != null)
            {
                m_soul.gameObject.SetActive(true);
            }
            Destroy(gameObject, 2);
        }
    }
}

