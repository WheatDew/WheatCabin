using UnityEngine;

namespace Battlehub.RTEditor
{
    public class ReflectActiveState : MonoBehaviour
    {
        [SerializeField]
        private CanvasGroup[] m_targets = null;

        [SerializeField]
        private bool m_invert = false;

        private void OnEnable()
        {
            foreach(CanvasGroup target in m_targets)
            {
                if (target)
                {
                    target.interactable = !m_invert;
                }
            }
        }

        private void OnDisable()
        {
            foreach (CanvasGroup target in m_targets)
            {
                if (target)
                {
                    target.interactable = m_invert;
                }
            }
        }
    }
}

