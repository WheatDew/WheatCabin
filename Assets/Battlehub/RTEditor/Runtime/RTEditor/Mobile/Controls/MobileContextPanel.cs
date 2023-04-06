using System.Collections;
using UnityEngine;

namespace Battlehub.RTEditor.Mobile.Controls
{
    public class MobileContextPanel : MonoBehaviour
    {
        [SerializeField]
        private Transform m_background = null;

        [SerializeField]
        private Transform m_gameObjectSection = null;

        private IEnumerator m_coSetSiblingIndex;

        private bool m_isInView;
        public bool IsInView
        {
            get { return m_isInView; }
            set
            {
                if(m_isInView != value)
                {
                    m_isInView = value;
                    UpdateIsActive();
                }
            }
        }

        private bool m_isActive = true;
        public bool IsActive
        {
            get { return m_isActive; }
            set
            {
                if (m_isActive != value)
                {
                    m_isActive = value;
                    UpdateIsActive();
                }
            }
        }

        private void OnTransformChildrenChanged()
        {
            if(gameObject.activeInHierarchy)
            {
                if (m_coSetSiblingIndex == null)
                {
                    m_coSetSiblingIndex = CoSetSiblingIndex();
                    StartCoroutine(m_coSetSiblingIndex);
                }
            }
        }


        private void OnEnable()
        {
            if (m_coSetSiblingIndex == null)
            {
                m_coSetSiblingIndex = CoSetSiblingIndex();
                StartCoroutine(m_coSetSiblingIndex);
            }
        }

        private void Start()
        {
            UpdateIsActive();
        }

        private void OnDisable()
        {
            if(m_coSetSiblingIndex != null)
            {
                StopCoroutine(m_coSetSiblingIndex);
                m_coSetSiblingIndex = null;
            }    
        }

        private IEnumerator CoSetSiblingIndex()
        {
            yield return null;

            if(m_background != null)
            {
                m_background.SetSiblingIndex(0);
                if(m_gameObjectSection != null)
                {
                    m_gameObjectSection.SetSiblingIndex(1);
                }
            }
            else
            {
                if (m_gameObjectSection != null)
                {
                    m_gameObjectSection.SetSiblingIndex(0);
                }
            }
            

            m_coSetSiblingIndex = null;
        }

        private void UpdateIsActive()
        {
            gameObject.SetActive(IsInView && IsActive && enabled);
        }
    }

}
