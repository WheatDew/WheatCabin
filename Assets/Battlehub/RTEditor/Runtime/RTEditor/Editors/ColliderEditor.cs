using Battlehub.RTCommon;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTEditor
{
    public class ColliderEditor : ComponentEditor
    {
        [SerializeField]
        private GameObject ToggleButton = null;

        private Toggle m_editColliderButton;

        private bool m_isEditing;

        private RuntimeTool m_lastTool;

        protected override void Awake()
        {
            base.Awake();
            m_lastTool = Editor.Tools.Current;
            Editor.Tools.ToolChanged += OnToolChanged;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (m_editColliderButton != null)
            {
                m_editColliderButton.onValueChanged.RemoveListener(OnEditCollider);
            }

            if (Editor != null)
            {
                Editor.Tools.ToolChanged -= OnToolChanged;
                Editor.Tools.Current = m_lastTool;
            }
        }

        private void OnToolChanged()
        {
            if(Editor.Tools.Current != RuntimeTool.None)
            {
                m_lastTool = Editor.Tools.Current;
                m_isEditing = false;
                if (m_editColliderButton != null)
                {
                    m_editColliderButton.isOn = false;
                }
                
            }
        }

        protected override void BuildEditor(IComponentDescriptor componentDescriptor, PropertyDescriptor[] descriptors)
        {
            base.BuildEditor(componentDescriptor, descriptors);

            if(NotNullComponents.Any() && NotNullComponents.All(component => (component.gameObject.hideFlags & HideFlags.HideInHierarchy) == 0))
            {
                m_editColliderButton = Instantiate(ToggleButton).GetComponent<Toggle>();
                m_editColliderButton.transform.SetParent(EditorsPanel, false);
                m_editColliderButton.onValueChanged.RemoveListener(OnEditCollider);
                m_editColliderButton.isOn = m_isEditing;
                m_editColliderButton.onValueChanged.AddListener(OnEditCollider);
            }
        }

        protected override void DestroyEditor()
        {
            base.DestroyEditor();
            if(m_editColliderButton != null)
            {
                m_editColliderButton.onValueChanged.RemoveListener(OnEditCollider);
                Destroy(m_editColliderButton.gameObject);
            }
        }

      
        private void OnEditCollider(bool edit)
        {
            m_isEditing = edit;
            if(m_isEditing)
            {
                m_lastTool = Editor.Tools.Current;
                Editor.Tools.Current = RuntimeTool.None;
                TryCreateGizmos(GetComponentDescriptor());
            }
            else
            {
                Editor.Tools.Current = m_lastTool;
                if (EndEditCallback != null)
                {
                    EndEditCallback();
                }
                DestroyGizmos();
            }
        }

        protected override void TryCreateGizmos(IComponentDescriptor componentDescriptor)
        {
            if(m_isEditing)
            {
                base.TryCreateGizmos(componentDescriptor);
            }   
        }

        protected override void TryCreateGizmos(IComponentDescriptor componentDescriptor, List<Component> gizmos, RuntimeWindow window)
        {
            if(m_isEditing)
            {
                base.TryCreateGizmos(componentDescriptor, gizmos, window);
            }
        }

        protected override void DestroyGizmos()
        {
            base.DestroyGizmos();
        }
    }
}

