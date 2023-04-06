using Battlehub.UIControls;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTEditor
{
    public class AnimationComponentView : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI m_label = null;

        [SerializeField]
        private Button m_addPropertyButton = null;

        [SerializeField]
        private CanvasGroup m_addButtonCanvasGroup = null;

        private RuntimeAnimationProperty m_item;
        public RuntimeAnimationProperty Item
        {
            get { return m_item; }
            set
            {
                m_item = value;

                if (m_item != null)
                {
                    if (m_label != null)
                    {
                        if (m_item.Parent == null)
                        {
                            m_label.text = m_item.ComponentDisplayName;
                        }
                        else
                        {
                            m_label.text = m_item.PropertyDisplayName;
                        }
                    }

                    if(m_addPropertyButton != null)
                    {
                        m_addPropertyButton.interactable = m_item.Parent != null;
                    }

                    if(m_addButtonCanvasGroup != null)
                    {
                        m_addButtonCanvasGroup.alpha = m_item.Parent != null ? 1 : 0;
                    }
                }
            }
        }


        public AnimationPropertiesView View
        {
            get;
            set;
        }

        public IAnimationSelectPropertiesDialog Dialog
        {
            get;
            set;
        }

        private void Awake()
        {
            UnityEventHelper.AddListener(m_addPropertyButton, button => button.onClick, OnAddPropertyButtonClick);
        }

        private void OnDestroy()
        {
            UnityEventHelper.RemoveListener(m_addPropertyButton, button => button.onClick, OnAddPropertyButtonClick);
        }

        private void OnAddPropertyButtonClick()
        {
            View.AddProperty(Item);
            Dialog.RemoveProperty(Item);
        }
    }
}

