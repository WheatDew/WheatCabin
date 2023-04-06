using Battlehub.RTCommon;
using Battlehub.UIControls;
using Battlehub.UIControls.Dialogs;
using Battlehub.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Battlehub.RTEditor
{
    public interface IAnimationSelectPropertiesDialog
    {
        AnimationPropertiesView View
        {
            get;
            set;
        }

        GameObject Target
        {
            get;
            set;
        }

        void RemoveProperty(RuntimeAnimationProperty propertyItem);
    }

}

namespace Battlehub.RTEditor.Views
{
    public class AnimationSelectPropertiesView : View, IAnimationSelectPropertiesDialog
    {
        [SerializeField]
        private VirtualizingTreeView m_propertiesTreeView = null;
        private Dialog m_parentDialog;

        public AnimationPropertiesView View
        {
            get;
            set;
        }

        public GameObject Target
        {
            get;
            set;
        }

        private VoidComponentEditor m_voidComponentEditor;
        private IRTE m_editor;
        protected override void Awake()
        {
            base.Awake();
        
            IOC.RegisterFallback<IAnimationSelectPropertiesDialog>(this);

            if (m_propertiesTreeView != null)
            {
                m_propertiesTreeView.CanReorder = false;
                m_propertiesTreeView.CanReparent = false;
                m_propertiesTreeView.CanDrag = false;
                m_propertiesTreeView.CanSelectAll = false;
                m_propertiesTreeView.CanMultiSelect = false;
                m_propertiesTreeView.CanEdit = false;

                m_propertiesTreeView.ItemDataBinding += OnItemDatabinding;
                m_propertiesTreeView.ItemExpanding += OnItemExpanding;
            }

            m_voidComponentEditor = gameObject.AddComponent<VoidComponentEditor>();
            m_editor = IOC.Resolve<IRTE>();
            m_editor.Undo.UndoCompleted += OnUndoCompleted;
            m_editor.Undo.RedoCompleted += OnRedoCompleted;

        }

        protected override void Start()
        {
            base.Start();

            m_parentDialog = GetComponentInParent<Dialog>();
            if (m_parentDialog != null)
            {
                m_parentDialog.IsOkVisible = true;
            }

            InitTree();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        
            IOC.UnregisterFallback<IAnimationSelectPropertiesDialog>(this);

            if (m_propertiesTreeView != null)
            {
                m_propertiesTreeView.ItemDataBinding -= OnItemDatabinding;
                m_propertiesTreeView.ItemExpanding -= OnItemExpanding;
            }

            m_voidComponentEditor = null;
            m_editor.Undo.UndoCompleted -= OnUndoCompleted;
            m_editor.Undo.RedoCompleted -= OnRedoCompleted;
            m_editor = null;
        }

        private void OnUndoCompleted()
        {
            InitTree();
        }

        private void OnRedoCompleted()
        {
            InitTree();
        }

        private void OnItemExpanding(object sender, VirtualizingItemExpandingArgs e)
        {
            RuntimeAnimationProperty item = (RuntimeAnimationProperty)e.Item;
            e.Children = item.Children;
        }

        private void OnItemDatabinding(object sender, VirtualizingTreeViewItemDataBindingArgs e)
        {
            AnimationComponentView ui = e.ItemPresenter.GetComponent<AnimationComponentView>();
            RuntimeAnimationProperty item = (RuntimeAnimationProperty)e.Item;
            ui.Item = item;
            ui.View = View;
            ui.Dialog = this;

            e.HasChildren = item.Children != null && item.Children.Count > 0;
        }

        private void InitTree()
        {
            HashSet<string> alreadyAddedHs = new HashSet<string>();
            RuntimeAnimationProperty[] alreadyAddedProperties = View.Props;
            for (int i = 0; i < alreadyAddedProperties.Length; ++i)
            {
                RuntimeAnimationProperty property = alreadyAddedProperties[i];
                alreadyAddedHs.Add(property.ComponentTypeName + " " + property.PropertyName);
            }

            MemberInfo enabledProperty = Strong.MemberInfo((Behaviour x) => x.enabled);
            List<RuntimeAnimationProperty> components = new List<RuntimeAnimationProperty>();
            IEditorsMap editorsMap = IOC.Resolve<IEditorsMap>();
            Type[] editableTypes = editorsMap.GetEditableTypes();
            for (int i = 0; i < editableTypes.Length; ++i)
            {
                Type editableType = editableTypes[i];
                if (!typeof(Component).IsAssignableFrom(editableType) || typeof(Component) == editableType)
                {
                    continue;
                }
                Component targetComponent = Target.GetComponent(editableType);
                if (targetComponent == null)
                {
                    continue;
                }
                m_voidComponentEditor.Components = new[] { targetComponent };

                RuntimeAnimationProperty component = new RuntimeAnimationProperty();
                component.ComponentDisplayName = editableType.Name;
                component.ComponentTypeName = string.Format("{0},{1}", editableType.FullName, editableType.Assembly.FullName.Split(',')[0]);
                component.Children = new List<RuntimeAnimationProperty>();
                component.Component = m_voidComponentEditor.Components[0];

                PropertyDescriptor[] propertyDescriptors = editorsMap.GetPropertyDescriptors(editableType, m_voidComponentEditor);
                for (int j = 0; j < propertyDescriptors.Length; ++j)
                {
                    PropertyDescriptor propertyDescriptor = propertyDescriptors[j];
                    Type memberType = propertyDescriptor.MemberType;
                    if (memberType.IsClass || memberType.IsEnum || typeof(MonoBehaviour).IsAssignableFrom(editableType) && propertyDescriptor.MemberInfo is PropertyInfo)
                    {
                        continue;
                    }

                    if (alreadyAddedHs.Contains(component.ComponentTypeName + " " + propertyDescriptor.MemberInfo.Name))
                    {
                        continue;
                    }

                    RuntimeAnimationProperty property = new RuntimeAnimationProperty();
                    property.Parent = component;
                    property.ComponentTypeName = component.ComponentTypeName;
                    property.ComponentDisplayName = component.ComponentDisplayName;
                    property.PropertyName = propertyDescriptor.MemberInfo.Name;
                    property.PropertyDisplayName = propertyDescriptor.Label;

                    if (propertyDescriptor.MemberInfo.Name == enabledProperty.Name && propertyDescriptor.MemberInfo.DeclaringType == enabledProperty.DeclaringType)
                    {
                        property.AnimationPropertyName = "m_Enabled";
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(propertyDescriptor.AnimationPropertyName))
                        {
                            Type componentType = property.ComponentType;
                            if (typeof(Component).IsAssignableFrom(componentType) && !typeof(MonoBehaviour).IsAssignableFrom(componentType))
                            {
                                //Trying to derive serialized property name
                                string aPropName = propertyDescriptor.MemberInfo.Name;
                                property.AnimationPropertyName = "m_" + char.ToUpper(aPropName[0]) + aPropName.Substring(1);
                            }
                            else
                            {
                                property.AnimationPropertyName = propertyDescriptor.MemberInfo.Name;
                            }
                        }
                        else
                        {
                            property.AnimationPropertyName = propertyDescriptor.AnimationPropertyName;
                        }
                    }

                    property.Component = propertyDescriptor.Target;


                    component.Children.Add(property);
                }

                if (component.Children.Count > 0)
                {
                    components.Add(component);
                }

                m_voidComponentEditor.Components = null;
            }

            m_propertiesTreeView.Items = components;
        }

        public virtual void RemoveProperty(RuntimeAnimationProperty propertyItem)
        {
            RuntimeAnimationProperty parent = propertyItem.Parent;
            m_propertiesTreeView.RemoveChild(propertyItem.Parent, propertyItem);
            if (parent != null && parent.Children != null && parent.Children.Count > 0)
            {
                parent.Children.Remove(propertyItem);
                if (parent.Children.Count == 0)
                {
                    m_propertiesTreeView.RemoveChild(null, parent);
                }
            }
        }
    }

}
