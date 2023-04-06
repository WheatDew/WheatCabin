using UnityEngine;

namespace Battlehub.RTEditor
{
    public class BusyIndicator : MonoBehaviour
    {
        [SerializeField]
        private Transform m_graphics = null;

        [SerializeField]
        private GameObject[] m_overlays = null;

        [SerializeField]
        private float m_interval = 0.2f;

        private float m_nextT;

        private void Awake()
        {
            m_graphics = transform;
        }

        private void OnEnable()
        {
            foreach(GameObject overlay in m_overlays)
            {
                overlay.gameObject.SetActive(true);
            }
        }

        private void OnDisable()
        {
            foreach (GameObject overlay in m_overlays)
            {
                overlay.gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            if(m_nextT <= Time.time)
            {
                m_nextT = Time.time + m_interval;
                m_graphics.Rotate(Vector3.forward, -30);
            }
        }
    }
}

