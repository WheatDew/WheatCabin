using Battlehub.UIControls;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Battlehub.RTEditor
{

    public delegate void AnimationPropertyEvent(RuntimeAnimationProperty property);
    public delegate void AnimationPropertyValueChanged(RuntimeAnimationProperty property, object oldValue, object newValue);

    public class RuntimeAnimationProperty
    {
        public event AnimationPropertyEvent BeginEdit;
        public event AnimationPropertyValueChanged ValueChanged;
        public event AnimationPropertyEvent EndEdit;

        public const string k_SpecialAddButton = "Special_AddButton";
        public const string k_SpecialEmptySpace = "Special_EmptySpace";

        public string ComponentTypeName;
        public string ComponentDisplayName;
        public string PropertyName;
        public string AnimationPropertyName;
        public string PropertyDisplayName;
        public RuntimeAnimationProperty Parent;
        public List<RuntimeAnimationProperty> Children;
        public bool HasChildren
        {
            get { return Children != null && Children.Count > 0; }
        }

        public AnimationCurve Curve;
        public object Component;
        public bool ComponentIsNull
        {
            get { return Component == null || (Component is Component) && ((Component)Component) == null; }
        }

        public Type ComponentType
        {
            get { return Type.GetType(ComponentTypeName); }
        }

        public string AnimationPropertyPath
        {
            get
            {
                if(Parent != null)
                {
                    return Parent.AnimationPropertyName + "." + PropertyName;
                }
                return AnimationPropertyName;
            }
        }

        public RuntimeAnimationProperty()
        {
            
            
        }
        public RuntimeAnimationProperty(RuntimeAnimationProperty item) : this()
        {
            ComponentTypeName = item.ComponentTypeName;
            ComponentDisplayName = item.ComponentDisplayName;
            PropertyName = item.PropertyName;
            AnimationPropertyName = item.AnimationPropertyName;
            PropertyDisplayName = item.PropertyDisplayName;
            Parent = item.Parent;
            Children = item.Children;
            Component = item.Component;
        }

            
        public float FloatValue
        {
            get
            {
                if(ComponentIsNull)
                {
                    return 0;
                }

                return Convert.ToSingle(Value);
            }
        }

        public object Value
        {
            get
            {
                if (Parent != null)
                {
                    if(Parent.ComponentIsNull)
                    {
                        return null;
                    }
                    return GetMemberValue(Parent.Value, PropertyName);
                }

                return GetMemberValue(Component, PropertyName);
            }
        }

        public void SetValue(object value, bool raiseValueChangedEvent)
        {
            object oldValue = Value;
            if (Parent != null)
            {
                object v = Parent.Value;
                SetMemberValue(v, PropertyName, value);
                Parent.SetValue(v, raiseValueChangedEvent);
            }
            else
            {
                SetMemberValue(Component, PropertyName, value);
            }

            if (ValueChanged != null && raiseValueChangedEvent)
            {
                object newValue = Value;
                if (oldValue != null || newValue != null)
                {
                    if (oldValue == null || newValue == null)
                    {
                        ValueChanged(this, oldValue, newValue);
                    }
                    else
                    {
                        if (!oldValue.Equals(newValue))
                        {
                            ValueChanged(this, oldValue, newValue);
                        }
                    }
                }
            }
        }

        private static void SetMemberValue(object obj, string path, object value)
        {
            Type propertyType = obj.GetType();
            MemberInfo[] members = propertyType.GetMember(path);
            if (members[0].MemberType == MemberTypes.Field)
            {
                FieldInfo fieldInfo = (FieldInfo)members[0];
                fieldInfo.SetValue(obj, value);

            }
            else if (members[0].MemberType == MemberTypes.Property)
            {
                PropertyInfo propInfo = (PropertyInfo)members[0];
                propInfo.SetValue(obj, value);
            }
        }

        private static object GetMemberValue(object obj, string path)
        {
            Type propertyType = obj.GetType();
            MemberInfo[] members = propertyType.GetMember(path);
            if (members[0].MemberType == MemberTypes.Field)
            {
                FieldInfo fieldInfo = (FieldInfo)members[0];
                return fieldInfo.GetValue(obj);
            }
            else if (members[0].MemberType == MemberTypes.Property)
            {
                PropertyInfo propInfo = (PropertyInfo)members[0];
                return propInfo.GetValue(obj);
            }

            throw new InvalidOperationException("wrong property path " + path);
        }

        public bool TryToCreateChildren()
        {
            if(ComponentTypeName == k_SpecialEmptySpace || ComponentTypeName == k_SpecialAddButton)
            {
                return false;
            }

            Type type = Value.GetType();
            if (Reflection.IsPrimitive(type))
            {
                return false;
            }

            if (!Reflection.IsValueType(type))
            {
                return false;
            }

            List<RuntimeAnimationProperty> children = new List<RuntimeAnimationProperty>();
            FieldInfo[] fields = type.GetSerializableFields();
            for (int i = 0; i < fields.Length; ++i)
            {
                FieldInfo field = fields[i];
                if(!Reflection.IsPrimitive(field.FieldType))
                {
                    continue;
                }

                RuntimeAnimationProperty child = new RuntimeAnimationProperty
                {
                    PropertyName = field.Name,
                    AnimationPropertyName = field.Name,
                    PropertyDisplayName = field.Name,
                    ComponentTypeName = ComponentTypeName,
                    Parent = this,
                    Component = Component,
                    Curve = new AnimationCurve(),
                };
                child.SetValue(GetMemberValue(Value, field.Name), false);
                children.Add(child);
            }

            PropertyInfo[] properties = type.GetSerializableProperties();
            for (int i = 0; i < properties.Length; ++i)
            {
                PropertyInfo property = properties[i];
                if (!Reflection.IsPrimitive(property.PropertyType))
                {
                    continue;
                }

                RuntimeAnimationProperty child = new RuntimeAnimationProperty
                {
                    PropertyName = property.Name,
                    AnimationPropertyName = property.Name,
                    PropertyDisplayName = property.Name,
                    ComponentTypeName = ComponentTypeName,
                    Parent = this,
                    Component = Component,
                    Curve = new AnimationCurve(),
                };
                child.SetValue(GetMemberValue(Value, property.Name), false);
                children.Add(child);
            }

            Children = children;
            return true;
        }

        public void AddKey(float time, float value)
        {
            if(Curve != null)
            {
                Curve.AddKey(time, value);
            }
        }

        public void RemoveKey(int index)
        {
            if(Curve != null)
            {
                Curve.RemoveKey(index);
            }
        }

        public void RaiseBeginEdit()
        {
            if(BeginEdit != null)
            {
                BeginEdit(this);
            }
        }

        public void RaiseEndEdit()
        {
            if(EndEdit != null)
            {
                EndEdit(this);
            }
        }
    }

    public class AnimationPropertyView : MonoBehaviour
    {        
        [SerializeField]
        private TextMeshProUGUI m_label = null;

        [SerializeField]
        private TMP_InputField m_inputField = null;

        [SerializeField]
        private Toggle m_toggle = null;

        [SerializeField]
        private Button m_addPropertyButton = null;

        [SerializeField]
        private DragField m_dragField = null;

        private RuntimeAnimationProperty m_item;
        public RuntimeAnimationProperty Item
        {
            get { return m_item; }
            set
            {
                m_item = value;

                if (m_started)
                {
                    OnItemChanged();
                }
            }
        }

        private void OnItemChanged()
        {
            if (m_item != null && m_item.ComponentTypeName != RuntimeAnimationProperty.k_SpecialAddButton && m_item.ComponentTypeName != RuntimeAnimationProperty.k_SpecialEmptySpace)
            {
                if (!m_item.ComponentIsNull)
                {
                    bool isBool = m_item.Value is bool;
                    bool hasChildren = m_item.Children != null && m_item.Children.Count > 0;

                    if (m_toggle != null)
                    {
                        m_toggle.gameObject.SetActive(isBool && !hasChildren);
                        if (isBool)
                        {
                            m_toggle.isOn = (bool)m_item.Value;
                        }
                    }

                    if (m_dragField != null)
                    {
                        m_dragField.enabled = !isBool && !hasChildren;
                    }

                    if (m_inputField != null)
                    {
                        if (!hasChildren)
                        {
                            m_inputField.transform.parent.gameObject.SetActive(!isBool);
                            m_inputField.DeactivateInputField();

                            if (!isBool && m_item.Value != null)
                            {
                                Type type = m_item.Value.GetType();
                                if (type == typeof(int) || type == typeof(long) || type == typeof(short) || type == typeof(uint) || type == typeof(ulong) || type == typeof(ushort) || type == typeof(byte))
                                {
                                    m_inputField.contentType = TMP_InputField.ContentType.IntegerNumber;
                                    if (m_dragField != null)
                                    {
                                        m_dragField.IncrementFactor = 1.0f;
                                    }
                                }
                                else if (type == typeof(float) || type == typeof(double) || type == typeof(decimal))
                                {
                                    m_inputField.contentType = TMP_InputField.ContentType.DecimalNumber;
                                    if (m_dragField != null)
                                    {
                                        m_dragField.IncrementFactor = 0.1f;
                                    }
                                }
                                else
                                {
                                    m_inputField.contentType = TMP_InputField.ContentType.Standard;
                                    if (m_dragField != null)
                                    {
                                        m_dragField.IncrementFactor = 1.0f;
                                    }
                                }

                                m_inputField.text = m_item.Value + "";
                            }
                            else
                            {
                                m_inputField.contentType = TMP_InputField.ContentType.Standard;
                                m_inputField.text = "";
                            }
                        }
                        else
                        {
                            m_inputField.transform.parent.gameObject.SetActive(false);
                            m_inputField.text = "";
                        }
                    }
                }
                else
                {
                    if (m_inputField != null)
                    {
                        m_inputField.transform.parent.gameObject.SetActive(false);
                    }
                    if (m_toggle != null)
                    {
                        m_toggle.gameObject.SetActive(false);
                    }
                }


                if (m_label != null)
                {
                    if (m_item.Parent == null)
                    {
                        m_label.text = string.Format("{0} : {1}", m_item.ComponentDisplayName, m_item.PropertyDisplayName);
                    }
                    else
                    {
                        m_label.text = string.Format("{0} : {1}", m_item.Parent.PropertyDisplayName, m_item.PropertyDisplayName);
                    }

                    m_label.gameObject.SetActive(true);
                }

                if (m_addPropertyButton != null)
                {
                    m_addPropertyButton.gameObject.SetActive(false);
                }
            }
            else
            {
                if (m_inputField != null)
                {
                    m_inputField.transform.parent.gameObject.SetActive(false);
                }
                if (m_toggle != null)
                {
                    m_toggle.gameObject.SetActive(false);
                }
                if (m_label != null)
                {
                    m_label.gameObject.SetActive(false);
                }

                if (m_addPropertyButton != null)
                {
                    m_addPropertyButton.gameObject.SetActive(m_item.ComponentTypeName == RuntimeAnimationProperty.k_SpecialAddButton);
                }

            }
        }

        public AnimationPropertiesView View
        {
            get;
            set;
        }

        private bool m_isDragging;
        private bool m_started;

        private void Awake()
        {
            UnityEventHelper.AddListener(m_inputField, input => input.onEndEdit, OnInputFieldEndEdit);
            UnityEventHelper.AddListener(m_addPropertyButton, button => button.onClick, OnAddPropertyButtonClick);
            UnityEventHelper.AddListener(m_toggle, toggle => toggle.onValueChanged, OnToggleValueChange);
            UnityEventHelper.AddListener(m_dragField, dragField => dragField.BeginDrag, OnDragFieldBeginDrag);
            UnityEventHelper.AddListener(m_dragField, dragField => dragField.EndDrag, OnDragFieldEndDrag);
        }

        private void Start()
        {
            m_started = true;
            OnItemChanged();
        }

        private void OnDestroy()
        {
            UnityEventHelper.RemoveListener(m_inputField, input => input.onEndEdit, OnInputFieldEndEdit);
            UnityEventHelper.RemoveListener(m_addPropertyButton, button => button.onClick, OnAddPropertyButtonClick);
            UnityEventHelper.RemoveListener(m_toggle, toggle => toggle.onValueChanged, OnToggleValueChange);
            UnityEventHelper.RemoveListener(m_dragField, dragField => dragField.BeginDrag, OnDragFieldBeginDrag);
            UnityEventHelper.RemoveListener(m_dragField, dragField => dragField.EndDrag, OnDragFieldEndDrag);
        }

        private void OnAddPropertyButtonClick()
        {
            View.AddProperty(Item);
        }

        private void OnInputFieldEndEdit(string value)
        {
            RaiseBeginEdit();
            UpdateValue(value, true);
            RaiseEndEdit();
        }

        private void UpdateValue(string value, bool raiseValueChangedEvent)
        {
            if (m_item != null && !m_item.ComponentIsNull)
            {
                Type type = m_item.Value.GetType();

                object result;
                if (Reflection.TryConvert(value, type, out result))
                {
                    m_item.SetValue(result, raiseValueChangedEvent);
                }
            }
        }

        private void OnDragFieldBeginDrag()
        {
            m_isDragging = true;
            RaiseBeginEdit();
        }

        private void OnDragFieldEndDrag()
        {
            if(m_isDragging)
            {
                m_isDragging = false;
                UpdateValue(m_inputField.text, true);
                RaiseEndEdit();
            }
        }

        private void OnToggleValueChange(bool value)
        {
            if(!m_item.ComponentIsNull)
            {
                m_item.RaiseBeginEdit();
                m_item.SetValue(value, true);
                m_item.RaiseEndEdit();
            }
        }

        private void RaiseBeginEdit()
        {
            if (m_item != null && !m_item.ComponentIsNull)
            {
                m_item.RaiseBeginEdit();
            }
        }

        private void RaiseEndEdit()
        {
            if (m_item != null && !m_item.ComponentIsNull)
            {
                m_item.RaiseEndEdit();
            }
        }

        private float m_nextUpdate = 0.0f;
        private void Update()
        {
            if (m_isDragging)
            {
                UpdateValue(m_inputField.text, true);
                return;
            }

            if (EventSystem.current.currentSelectedGameObject == m_inputField.gameObject)
            {
                return;
            }

            if (m_nextUpdate > Time.time)
            {
                return;
            }

            object component = m_item.Component;
            if (component != null && m_item.ComponentIsNull)
            {
                m_item.Component = null;
                if (m_inputField != null)
                {
                    m_inputField.transform.parent.gameObject.SetActive(false);
                }
                if (m_toggle != null)
                {
                    m_toggle.gameObject.SetActive(false);
                }
            }

            m_nextUpdate = Time.time + 0.2f;
            if(m_item == null || m_item.ComponentIsNull ||
               m_item.ComponentTypeName == RuntimeAnimationProperty.k_SpecialAddButton ||
               m_item.ComponentTypeName == RuntimeAnimationProperty.k_SpecialEmptySpace )
            {
                return;
            }

            bool hasChildren = m_item.Children != null && m_item.Children.Count > 0;
            if(hasChildren)
            {
                return;
            }

            bool isBool = m_item.Value is bool;
            if (isBool)
            {
                if (m_toggle != null)
                {
                    m_toggle.isOn = (bool)m_item.Value;
                }
            }
            else
            {
                if (m_inputField != null)
                {
                    if (m_item.Value is float)
                    {
                        float val = (float)m_item.Value;
                        m_inputField.text = val.ToString(CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        m_inputField.text = m_item.Value + "";
                    }
                }
            }
        }
    }
}

