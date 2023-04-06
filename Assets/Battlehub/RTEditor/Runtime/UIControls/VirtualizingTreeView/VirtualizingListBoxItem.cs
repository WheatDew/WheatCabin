using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Battlehub.UIControls
{
    public class VirtualizingListBoxItem : VirtualizingItemContainer
    {
        [SerializeField]
        private GameObject m_root;

        private Toggle m_toggle;

        public override bool IsSelected
        {
            get { return base.IsSelected; }
            set
            {
                if (base.IsSelected != value)
                {
                    if (m_toggle != null)
                    {
                        m_toggle.SetIsOnWithoutNotify(value);
                    }
                    base.IsSelected = value;
                }
            }
        }

        public override object Item 
        { 
            get { return base.Item; }
            set 
            {
                base.Item = value;
                name = value != null ? value.ToString() : "Null";
            }
        }

        public override bool CanSelect
        { 
            get { return base.CanSelect; }
            set
            {
                if(CanSelect != value)
                {
                    base.CanSelect = value;

                    if (CanSelect)
                    {
                        if (m_toggle != null)
                        {
                            m_toggle.isOn = false;
                        }
                    }
                }
            }
        }

        protected override void AwakeOverride()
        {
            base.AwakeOverride();
            m_toggle = GetComponent<Toggle>();
            m_toggle.onValueChanged.AddListener(OnToggleValueChanged);
                        
            if(CanSelect)
            {
                m_toggle.isOn = false;
            }
            
            if(m_root == null)
            {
                m_root = transform.GetChild(0).gameObject;
            }
        }


        private void OnDestroy()
        {
            if(m_toggle != null)
            {
                m_toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
                m_toggle = null;
            }
        }

        private void OnToggleValueChanged(bool value)
        {
            if(!CanSelect)
            {
                m_toggle.SetIsOnWithoutNotify(false);
            }
            else
            {
                m_toggle.SetIsOnWithoutNotify(IsSelected);
            }
        }


        private bool m_wasInteractable;
        private bool m_canHandlePointerClick;
        private protected override void OnPointerDownOverride(PointerEventData eventData)
        {
            base.OnPointerDownOverride(eventData);
            m_canHandlePointerClick = true;
        }

        private protected override void OnBeginDragOverride(PointerEventData eventData)
        {
            base.OnBeginDragOverride(eventData);

            m_wasInteractable = m_toggle.interactable;
            m_toggle.interactable = false;

            m_canHandlePointerClick = false;
        }

        private protected override void OnEndDragOverride(PointerEventData eventData)
        {
            base.OnEndDragOverride(eventData);
            m_toggle.interactable = m_wasInteractable;
        }

        private protected override void OnPointerClickOverride(PointerEventData eventData)
        {
            if(m_canHandlePointerClick)
            {
                base.OnPointerClickOverride(eventData);
            }
        }

    }
}
