using UnityEngine;
using System;
using System.Reflection;
using System.Linq;
using UnityEngine.UI;
using Battlehub.RTCommon;
using Battlehub.RTSL.Interface;
using TMPro;
using System.Collections.Generic;
using Battlehub.RTGizmos;
using UnityEngine.Serialization;
using Battlehub.RTEditor.Models;

namespace Battlehub.RTEditor
{
    public struct PropertyDescriptor
    {
        public string Label;
        public string AnimationPropertyName;
        public MemberInfo MemberInfo;
        public MemberInfo ComponentMemberInfo;
        public PropertyEditorCallback ValueChangedCallback;
        public PropertyEditorCallback EndEditCallback;
        public object PropertyMetadata;
        public bool? ConvertUnits;

        [Obsolete("Use PropertyMetadata instead")]
        public Range Range
        {
            get { return PropertyMetadata as Range; }
            set { PropertyMetadata = value; }
        }
        
        public PropertyDescriptor[] ChildDesciptors;
        public Type MemberType
        {
            get
            {
                if(PropertyMetadata != null)
                {
                    return PropertyMetadata.GetType();
                }

                if (MemberInfo is PropertyInfo)
                {
                    PropertyInfo prop = (PropertyInfo)MemberInfo;
                    return prop.PropertyType;
                }
                else if (MemberInfo is FieldInfo)
                {
                    FieldInfo field = (FieldInfo)MemberInfo;
                    return field.FieldType;
                }

                return null;
            }
        }

        public Type ComponentMemberType
        {
            get
            {
                if (ComponentMemberInfo is PropertyInfo)
                {
                    PropertyInfo prop = (PropertyInfo)ComponentMemberInfo;
                    return prop.PropertyType;
                }
                else if (ComponentMemberInfo is FieldInfo)
                {
                    FieldInfo field = (FieldInfo)ComponentMemberInfo;
                    return field.FieldType;
                }

                return null;
            }
        }

        public object Target
        {
            get { return Targets != null && Targets.Length > 0 ? Targets[0] : null; }
            set
            {
                if (value == null)
                {
                    Targets = null;
                }
                else
                {
                    Targets = new[] { value };
                }
            }
        }

        public object[] Targets;
        
        public PropertyDescriptor(string label, object[] targets, MemberInfo memberInfo) : this(label, targets, memberInfo, memberInfo.Name) {}

        public PropertyDescriptor(string label, object[] targets, MemberInfo memberInfo, string animationPropertyName)
            : this(label, targets, memberInfo, memberInfo)
        {
            AnimationPropertyName = animationPropertyName;
        }

        public PropertyDescriptor(string label, object[] targets, MemberInfo memberInfo, MemberInfo componentMemberInfo) 
            : this(label, targets, memberInfo, componentMemberInfo, null)
        {
        }

        public PropertyDescriptor(string label, object[] targets, MemberInfo memberInfo, MemberInfo componentMemberInfo, bool convertUnits)
            : this(label, targets, memberInfo, componentMemberInfo, null)
        {
            ConvertUnits = convertUnits;
        }

        public PropertyDescriptor(string label, object[] targets, MemberInfo memberInfo, MemberInfo componentMemberInfo, PropertyEditorCallback valueChangedCallback)
            : this(label, targets, memberInfo, componentMemberInfo, valueChangedCallback, null)
        { 
        }

        public PropertyDescriptor(string label, object[] targets, MemberInfo memberInfo, MemberInfo componentMemberInfo, PropertyEditorCallback valueChangedCallback, object settings)
             : this(label, targets, memberInfo, componentMemberInfo, valueChangedCallback, null)
        {
            PropertyMetadata = settings;
        }

        public PropertyDescriptor(string label, object[] targets, MemberInfo memberInfo, MemberInfo componentMemberInfo, PropertyEditorCallback valueChangedCallback, PropertyEditorCallback endEditCallback)
        {
            MemberInfo = memberInfo;
            ComponentMemberInfo = componentMemberInfo;
            Label = label;
            Targets = targets;
            ValueChangedCallback = valueChangedCallback;
            EndEditCallback = endEditCallback;
            PropertyMetadata = TryGetRange(memberInfo);
            ChildDesciptors = null;
            AnimationPropertyName = null;
            ConvertUnits = null;
        }

        public PropertyDescriptor(string label, object target, MemberInfo memberInfo) 
            : this(label, new[] { target }, memberInfo, memberInfo.Name) { }

        public PropertyDescriptor(string label, object target, MemberInfo memberInfo, string animationPropertyName)
            : this(label, new[] { target }, memberInfo, memberInfo) 
        {
            AnimationPropertyName = animationPropertyName;
        }

        public PropertyDescriptor(string label, object target, MemberInfo memberInfo, MemberInfo componentMemberInfo)
            : this(label, new[] { target }, memberInfo, componentMemberInfo, null) { }

        public PropertyDescriptor(string label, object target, MemberInfo memberInfo, MemberInfo componentMemberInfo, PropertyEditorCallback valueChangedCallback)
            : this(label, new[] { target }, memberInfo, componentMemberInfo, valueChangedCallback, null) { }

        public PropertyDescriptor(string label, object target, MemberInfo memberInfo, MemberInfo componentMemberInfo, PropertyEditorCallback valueChangedCallback, object settings)
             : this(label, new[] { target }, memberInfo, componentMemberInfo, valueChangedCallback, null) 
        {
            PropertyMetadata = settings;
        }

        public PropertyDescriptor(string label, object target, MemberInfo memberInfo, MemberInfo componentMemberInfo, PropertyEditorCallback valueChangedCallback, PropertyEditorCallback endEditCallback)
            : this(label, new[] { target }, memberInfo, componentMemberInfo, valueChangedCallback, endEditCallback) { }

        private static Range TryGetRange(MemberInfo memberInfo)
        {
            RangeAttribute range = memberInfo.GetCustomAttribute<RangeAttribute>();
            if (range != null)
            {
                if (memberInfo.GetUnderlyingType() == typeof(int))
                {
                    return new RangeInt((int)range.min, (int)range.max);
                }
                else if (memberInfo.GetUnderlyingType() == typeof(float))
                {
                    return new Range(range.min, range.max);
                }
            }
            return null;
        }
    }

    public class VoidComponentEditor : ComponentEditor
    {
        public override Component[] Components
        {
            get { return m_components; }
            set { m_components = value; }
        }

        protected override void Update()
        {
        }
    }

    public class ComponentEditor : MonoBehaviour
    {
        /// <summary>
        /// Used to update previews
        /// </summary>
        public PropertyEditorCallback EndEditCallback;

        [SerializeField, FormerlySerializedAs("HeaderPanel")]
        protected Transform m_headerPanel = null;
        protected internal Transform HeaderPanel
        {
            get { return m_headerPanel; }
            set { m_headerPanel = value; }
        }
        
        [SerializeField, FormerlySerializedAs("EditorsPanel")]
        protected Transform m_editorsPanel = null;
        protected internal Transform EditorsPanel
        {
            get { return m_editorsPanel; }
            set { m_editorsPanel = value; }
        }
        
        [SerializeField, FormerlySerializedAs("EnabledEditor")]
        private BoolEditor m_enabledEditor = null;
        protected internal BoolEditor EnabledEditor
        {
            get { return m_enabledEditor; }
            set { m_enabledEditor = value; }
        }

        [SerializeField, FormerlySerializedAs("Header")]
        private TextMeshProUGUI m_header = null;
        protected internal TextMeshProUGUI Header
        {
            get { return m_header; }
            set { m_header = value; }
        }

        [SerializeField, FormerlySerializedAs("Expander")]
        private Toggle m_expander = null;
        protected internal Toggle ExpanderToggle
        {
            get { return m_expander; }
            set { m_expander = value; }
        }

        [SerializeField, FormerlySerializedAs("ExpanderGraphics")]
        private GameObject m_expanderGraphics = null;
        protected internal GameObject ExpanderGraphics
        {
            get { return m_expanderGraphics; }
            set { m_expanderGraphics = value; }
        }

        [SerializeField, FormerlySerializedAs("ResetButton")]
        private Button m_resetButton = null;
        protected internal Button ResetButton
        {
            get { return m_resetButton; }
            set { m_resetButton = value; }
        }

        [SerializeField, FormerlySerializedAs("RemoveButton")]
        private Button m_removeButton = null;
        protected internal Button RemoveButton
        {
            get { return m_removeButton; }
            set { m_removeButton = value; }
        }
        
        [SerializeField, FormerlySerializedAs("Icon")]
        protected Image m_iconImage = null;
        protected internal Image IconImage
        {
            get { return m_iconImage; }
            set { m_iconImage = value; }
        }

        protected bool IsComponentExpanded
        {
            get
            {
                if(m_expander == null)
                {
                    return true;
                }

                ISettingsComponent settingsComponent = IOC.Resolve<ISettingsComponent>();
                var componentEditorSettings = settingsComponent.BuiltInWindowsSettings.Inspector.ComponentEditor;

                int defaultValue = 1;
                if(componentEditorSettings.IsExpandedByDefault != null)
                {
                    defaultValue = componentEditorSettings.IsExpandedByDefault.Value ? 1 : 0;
                }
               
                string componentName = "BH_CE_EXP_" + ComponentType.AssemblyQualifiedName;
                return PlayerPrefs.GetInt(componentName, defaultValue) == 1;
            }
            set
            {
                string componentName = "BH_CE_EXP_" + ComponentType.AssemblyQualifiedName;
                PlayerPrefs.SetInt(componentName, value ? 1 : 0);
            }
        }

        public virtual Component Component
        {
            get { return m_components != null && m_components.Length > 0 ? m_components[0] : null; }
            set
            {
                if(value == null)
                {
                    throw new ArgumentNullException("value");
                }
                Components = new[] { value };    
            }
        }

        protected Type ComponentType
        {
            get { return m_components[0].GetType(); }
        }

        public IEnumerable<Component> NotNullComponents
        {
            get
            {
                if(Components == null)
                {
                    yield break;
                }

                foreach(Component component in Components)
                {
                    if(component != null)
                    {
                        yield return component;
                    }
                }
            }
        }

        protected Component[] m_components;
        public virtual Component[] Components
        {
            get { return m_components; }
            set
            {
                m_components = value;

                if(m_isStarted)
                {
                    OnComponentsChanged();
                }
            }
        }

        private void OnComponentsChanged()
        {
            if (m_components == null || m_components.Length == 0)
            {
                throw new ArgumentNullException("value");
            }

            IComponentDescriptor componentDescriptor = GetComponentDescriptor();
            if (EnabledEditor != null)
            {
                PropertyInfo enabledProperty = EnabledProperty;
                if (enabledProperty != null && (componentDescriptor == null || componentDescriptor.GetHeaderDescriptor(m_editor).ShowEnableButton))
                {
                    EnabledEditor.gameObject.SetActive(true);
                    EnabledEditor.Init(Components, Components, enabledProperty, null, string.Empty, () => { },
                        () => CreateOrDestroyGizmos(componentDescriptor),
                        () =>
                        {
                            if (EndEditCallback != null)
                            {
                                EndEditCallback();
                            }
                        },
                        true, null, null, null,
                        () => CreateOrDestroyGizmos(componentDescriptor), () => CreateOrDestroyGizmos(componentDescriptor));
                }
                else
                {
                    EnabledEditor.gameObject.SetActive(false);
                }
            }

            if (Header != null)
            {
                if (componentDescriptor != null)
                {
                    Header.text = componentDescriptor.GetHeaderDescriptor(m_editor).DisplayName;
                }
                else
                {
                    string typeName = ComponentType.Name;
                    ILocalization localization = IOC.Resolve<ILocalization>();
                    Header.text = localization.GetString("ID_RTEditor_CD_" + typeName, typeName);
                }
            }

            if (ExpanderToggle != null)
            {
                ExpanderToggle.isOn = IsComponentExpanded;
            }

            BuildEditor();
        }

        private bool IsComponentEnabled
        {
            get
            {
                if (EnabledProperty == null)
                {
                    return true;
                }

                //TODO: Handle mixed values
                object v = EnabledProperty.GetValue(Components[0], null); 
                if (v is bool)
                {
                    bool isEnabled = (bool)v;
                    return isEnabled;
                }
                return true;
            }   
        }

        protected PropertyInfo EnabledProperty
        {
            get
            {
                Type type = ComponentType;

                while(type != typeof(UnityEngine.Object))
                {
                    PropertyInfo prop = type.GetProperty("enabled", BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance);
                    if (prop != null && prop.PropertyType == typeof(bool) && prop.CanRead && prop.CanWrite)
                    {
                        return prop;
                    }
                    type = type.BaseType();
                }

                return null;
            }
        }

        private IRTE m_editor;
        public IRTE Editor
        {
            get { return m_editor; }
        }

        private IProjectAsync m_project;
        private IEditorsMap m_editorsMap;
        private object m_converter;
        private Dictionary<RuntimeWindow, Component[]> m_gizmos = new Dictionary<RuntimeWindow, Component[]>();
        private bool m_isStarted;

        protected virtual void Awake()
        {
            m_editor = IOC.Resolve<IRTE>();
            if(m_editor.Object != null)
            {
                m_editor.Object.ReloadComponentEditor += OnReloadComponentEditor;
            }
            m_project = IOC.Resolve<IProjectAsync>();
            m_editorsMap = IOC.Resolve<IEditorsMap>();

#pragma warning disable CS0612
            AwakeOverride();
#pragma warning restore CS0612
        }

        protected virtual void Start()
        {
            if (Components == null || Components.Length == 0)
            {
                return;
            }

            if(ExpanderToggle != null)
            {
                ExpanderToggle.onValueChanged.AddListener(OnExpanded);
            }
            
            if(ResetButton != null)
            {
                ResetButton.onClick.AddListener(OnResetClick);
            }

            if(RemoveButton != null)
            {
                RemoveButton.onClick.AddListener(OnRemove);
            }

            m_editor.Object.ReloadComponentEditor -= OnReloadComponentEditor;
            m_editor.Object.ReloadComponentEditor += OnReloadComponentEditor;
            m_editor.Undo.UndoCompleted += OnUndoCompleted;
            m_editor.Undo.RedoCompleted += OnRedoCompleted;
            m_editor.WindowRegistered += OnWindowRegistered;
            m_editor.WindowUnregistered += OnWindowUnregistered;
            m_editor.BeforePlaymodeStateChange += OnBeforePlayModeStateChange;
            
#pragma warning disable CS0612
            StartOverride();
#pragma warning restore CS0612

            m_isStarted = true;
            OnComponentsChanged();
        }

        protected virtual void OnDestroy()
        {
            m_isStarted = false;

            if(m_editor != null)
            {
                m_editor.Undo.UndoCompleted -= OnUndoCompleted;
                m_editor.Undo.RedoCompleted -= OnRedoCompleted;
                m_editor.WindowRegistered -= OnWindowRegistered;
                m_editor.WindowUnregistered -= OnWindowUnregistered;
                m_editor.BeforePlaymodeStateChange -= OnBeforePlayModeStateChange;

                if (m_editor.Object != null)
                {
                    m_editor.Object.ReloadComponentEditor -= OnReloadComponentEditor;
                }
            }

            if (ExpanderToggle != null)
            {
                ExpanderToggle.onValueChanged.RemoveListener(OnExpanded);
            }

            if(ResetButton != null)
            {
                ResetButton.onClick.RemoveListener(OnResetClick);
            }

            if (RemoveButton != null)
            {
                RemoveButton.onClick.RemoveListener(OnRemove);
            }

            foreach (Component[] gizmos in m_gizmos.Values)
            {
                for (int i = 0; i < gizmos.Length; ++i)
                {
                    Destroy(gizmos[i]);
                }
            }
            m_gizmos.Clear();

#pragma warning disable CS0612
            OnDestroyOverride();
#pragma warning restore CS0612
        }

        protected virtual void Update()
        {
            if (Components == null || Components.Length == 0 || Components[0] == null)
            {
                Destroy(gameObject);
            }

#pragma warning disable CS0612
            UpdateOverride();
#pragma warning restore CS0612
        }

        public virtual void BuildEditor()
        {
            IComponentDescriptor componentDescriptor = GetComponentDescriptor();
            if (componentDescriptor != null)
            {
                m_converter = componentDescriptor.CreateConverter(this);
            }

            PropertyDescriptor[] descriptors = GetPropertyDescriptors(ComponentType, this, m_converter);
            if (descriptors == null || descriptors.Length == 0)
            {
                if(ExpanderGraphics != null)
                {
                    ExpanderGraphics.SetActive(false);
                }
                
                return;
            }

            ISettingsComponent settingsComponent = IOC.Resolve<ISettingsComponent>();
            BuiltInWindowsSettings settings;
            if (settingsComponent == null)
            {
                settings = BuiltInWindowsSettings.Default;
            }
            else
            {
                settings = settingsComponent.BuiltInWindowsSettings;
            }

            InspectorSettings.ComponentEditorSettings componentEditorSettings = settings.Inspector.ComponentEditor;
            HeaderDescriptor headerDescriptor = componentDescriptor != null ? componentDescriptor.GetHeaderDescriptor(m_editor) : new HeaderDescriptor();
            if (IconImage != null)
            {
                bool showIcon = componentDescriptor != null ?
                   headerDescriptor.ShowIcon:
                   componentEditorSettings.ShowIcon;

                if(showIcon)
                {
                    Sprite icon = componentDescriptor != null ?
                        headerDescriptor.Icon :
                        componentEditorSettings.Icon;

                    if (icon == null && settingsComponent.SelectedTheme != null)
                    {
                        icon = settingsComponent.SelectedTheme.GetIcon($"{ComponentType.Name} Icon");
                        if(icon == null)
                        {
                            icon = settingsComponent.SelectedTheme.GetIcon("RTE_Component_Default Icon");
                        }
                    }

                    IconImage.sprite = icon;
                }

                IconImage.transform.parent.gameObject.SetActive(showIcon && IconImage.sprite != null);
            }

            if (ResetButton != null)
            {
                ResetButton.gameObject.SetActive(componentDescriptor != null ?
                    headerDescriptor.ShowResetButton:
                    componentEditorSettings.ShowResetButton);
            }

            if (RemoveButton != null)
            {
                bool showRemoveButton = componentDescriptor != null ?
                    headerDescriptor.ShowRemoveButton :
                    componentEditorSettings.ShowRemoveButton;
                if (showRemoveButton)
                {
                    bool canRemove = m_project == null || m_project.Utils.ToProjectItem(Components[0].gameObject) == null;
                    if (!canRemove)
                    {
                        showRemoveButton = false;
                    }
                }

                RemoveButton.gameObject.SetActive(showRemoveButton);
            }

            if (EnabledEditor != null && EnabledProperty != null)
            {
                EnabledEditor.gameObject.SetActive(componentDescriptor != null ?
                    headerDescriptor.ShowEnableButton :
                    componentEditorSettings.ShowEnableButton);
            }
            
            if (ExpanderToggle == null)
            {
                BuildEditor(componentDescriptor, descriptors);
            }
            else
            {
       
                if (componentDescriptor != null ? !headerDescriptor.ShowExpander : !componentEditorSettings.ShowExpander)
                {
                    ExpanderToggle.isOn = true;
                    ExpanderToggle.enabled = false;
                }
                              
                if (ExpanderToggle.isOn)
                {
                    if (ExpanderGraphics != null)
                    {
                        ExpanderGraphics.SetActive(componentDescriptor != null ? headerDescriptor.ShowExpander : componentEditorSettings.ShowExpander);
                    }
                    BuildEditor(componentDescriptor, descriptors);
                }
            }

        }

        protected virtual void BuildEditor(IComponentDescriptor componentDescriptor, PropertyDescriptor[] descriptors)
        {
            DestroyEditor();
            TryCreateGizmos(componentDescriptor);

            for (int i = 0; i < descriptors.Length; ++i)
            {
                PropertyDescriptor descriptor = descriptors[i];
                if (descriptor.MemberInfo == EnabledProperty)
                {
                    continue;
                }
                BuildPropertyEditor(descriptor);
            }
        }

        private PropertyEditor InstantiatePropertyEditor(PropertyDescriptor descriptor)
        {
            if (descriptor.MemberInfo == null)
            {
                Debug.LogError("desciptor.MemberInfo is null");
                return null;
            }

            Type memberType;
            if (descriptor.MemberInfo is MethodInfo)
            {
                memberType = typeof(MethodInfo);
            }
            else
            {
                memberType = descriptor.MemberType;
            }

            if (memberType == null)
            {
                Debug.LogError("descriptor.MemberType is null");
                return null;
            }

            GameObject editorGo = m_editorsMap.GetPropertyEditor(memberType);
            if (editorGo == null)
            {
                return null;
            }

            if (!m_editorsMap.IsPropertyEditorEnabled(memberType))
            {
                return null;
            }
            PropertyEditor editor = editorGo.GetComponent<PropertyEditor>();
            if (editor == null)
            {
                Debug.LogErrorFormat("editor {0} is not PropertyEditor", editorGo);
                return null;
            }
            PropertyEditor instance = Instantiate(editor);
            instance.name = editor.name;
            instance.transform.SetParent(EditorsPanel, false);

            if(descriptor.ConvertUnits != null)
            {
                instance.ConvertUnits = descriptor.ConvertUnits.Value;
            }

            return instance;
        }

        protected virtual void BuildPropertyEditor(PropertyDescriptor descriptor)
        {
            PropertyEditor editor = InstantiatePropertyEditor(descriptor);
            if (editor == null)
            {
                return;
            }
            if (descriptor.PropertyMetadata != null)
            {
                if (descriptor.PropertyMetadata is RangeInt)
                {
                    RangeInt range = (RangeInt)descriptor.PropertyMetadata;
                    RangeIntEditor rangeEditor = editor as RangeIntEditor;
                    rangeEditor.Min = (int)range.Min;
                    rangeEditor.Max = (int)range.Max;
                }
                else if (descriptor.PropertyMetadata is RangeFlags)
                {
                    RangeFlags flags = (RangeFlags)descriptor.PropertyMetadata;
                    FlagsIntEditor flagsEditor = editor as FlagsIntEditor;
                    flagsEditor.Options = flags.Options;
                }
                else if (descriptor.PropertyMetadata is RangeOptions)
                {
                    RangeFlags options = (RangeFlags)descriptor.PropertyMetadata;
                    OptionsEditor optionsEditor = editor as OptionsEditor;
                    optionsEditor.Options = options.Options;

                }
                else if(descriptor.PropertyMetadata is Range)
                {
                    Range range = (Range)descriptor.PropertyMetadata;
                    RangeEditor rangeEditor = editor as RangeEditor;
                    rangeEditor.Min = range.Min;
                    rangeEditor.Max = range.Max;
                }   
                else if(descriptor.PropertyMetadata is BoolFloat)
                {
                    BoolFloat boolFloat = (BoolFloat)descriptor.PropertyMetadata;
                    BoolFloatEditor boolFloatEditor = editor as BoolFloatEditor;
                    boolFloatEditor.EnabledValue = boolFloat.EnabledValue;
                    boolFloatEditor.DisabledValue = boolFloat.DisabledValue;
                }
            }

            InitEditor(editor, descriptor);
        }

        //Better name for this is InitPropertyEditor
        protected virtual void InitEditor(PropertyEditor editor, PropertyDescriptor descriptor)
        {
            editor.Init(descriptor.Targets, descriptor.Targets, descriptor.MemberInfo, null, descriptor.Label, null, () => { descriptor.ValueChangedCallback?.Invoke(); OnValueChanged(); }, () => {  descriptor.EndEditCallback?.Invoke(); EndEditCallback?.Invoke(); OnEndEdit(); }, true, descriptor.ChildDesciptors, null, null, null, null, OnValueReloaded);
        }

        protected virtual void DestroyEditor()
        {
            DestroyGizmos();
            if(EditorsPanel != null)
            {
                foreach (Transform t in EditorsPanel)
                {
                    Destroy(t.gameObject);
                }
            }
        }

        private void CreateOrDestroyGizmos(IComponentDescriptor componentDescriptor)
        {
            if (IsComponentEnabled)
            {
                TryCreateGizmos(componentDescriptor);
            }
            else
            {
                DestroyGizmos();
            }
        }

        protected virtual void TryCreateGizmos(IComponentDescriptor componentDescriptor)
        {
            if (componentDescriptor != null && componentDescriptor.GizmoType != null && IsComponentEnabled)
            {
                RuntimeWindow[] windows = m_editor.Windows;
                for(int i = 0; i < windows.Length; ++i)
                {
                    RuntimeWindow window = windows[i];
                    if(window.WindowType == RuntimeWindowType.Scene)
                    {
                        List<Component> gizmos = new List<Component>();
                        TryCreateGizmos(componentDescriptor, gizmos, window);
                        m_gizmos.Add(window, gizmos.ToArray());
                    }   
                }
            }
        }

        protected virtual void TryCreateGizmos(IComponentDescriptor componentDescriptor, List<Component> gizmos, RuntimeWindow window)
        {
            if (componentDescriptor != null && componentDescriptor.GizmoType != null && IsComponentEnabled && (ExpanderToggle == null || ExpanderToggle.isOn))
            {
                for (int j = 0; j < Components.Length; ++j)
                {
                    Component component = Components[j];
                    if (component != null)
                    {
                        Component gizmo = component.gameObject.AddComponent(componentDescriptor.GizmoType);
                        
                        if (gizmo is IRTEComponent)
                        {
                            IRTEComponent rteComponent = (IRTEComponent)gizmo;
                            rteComponent.Window = window;
                        }

                        //TODO: replace with IRTEComponent.Reset method call!!!
                        if(gizmo is BaseGizmo)
                        {
                            BaseGizmo baseGizmo = (BaseGizmo)gizmo;
                            baseGizmo.Reset();
                        }
                        //gizmo.SendMessage("Reset", SendMessageOptions.DontRequireReceiver);
                        gizmos.Add(gizmo);
                    }
                }
            }
        }

        protected virtual void DestroyGizmos()
        {
            foreach (Component[] gizmos in m_gizmos.Values)
            {
                for (int i = 0; i < gizmos.Length; ++i)
                {
                    Component gizmo = gizmos[i];
                    if (gizmo != null)
                    {
                        DestroyImmediate(gizmo);
                    }
                }
            }
            m_gizmos.Clear();
        }

        private PropertyEditor GetPropertyEditor(MemberInfo memberInfo)
        {
            foreach (Transform t in EditorsPanel)
            {
                PropertyEditor propertyEditor = t.GetComponent<PropertyEditor>();
                if (propertyEditor != null && propertyEditor.MemberInfo == memberInfo)
                {
                    return propertyEditor;
                }
            }
            return null;
        }

        protected IComponentDescriptor GetComponentDescriptor()
        {
            IComponentDescriptor componentDescriptor;
            if (m_editorsMap.ComponentDescriptors.TryGetValue(ComponentType, out componentDescriptor))
            {
                return componentDescriptor;
            }
            return null;
        }


        private void OnBeforePlayModeStateChange()
        {
            DestroyGizmos();
        }

        protected virtual void OnValueChanged()
        {
        }

        protected virtual void OnValueReloaded()
        {
        }

        protected virtual void OnEndEdit()
        {
        }

        protected virtual void OnWindowRegistered(RuntimeWindow window)
        {
            if (window.WindowType != RuntimeWindowType.Scene)
            {
                return;
            }

            List<Component> gizmos = new List<Component>();
            TryCreateGizmos(GetComponentDescriptor(), gizmos, window);
            if (gizmos.Count > 0)
            {
                m_gizmos.Add(window, gizmos.ToArray());
            }
        }

        protected virtual void OnWindowUnregistered(RuntimeWindow window)
        {
            if (window.WindowType != RuntimeWindowType.Scene)
            {
                return;
            }

            Component[] gizmos;
            if(!m_gizmos.TryGetValue(window, out gizmos))
            {
                return;
            }

            for (int i = gizmos.Length - 1; i >= 0; i--)
            {
                DestroyImmediate(gizmos[i]);
            }
            m_gizmos.Remove(window);
        }

        protected virtual void OnExpanded(bool expanded)
        {
            IsComponentExpanded = expanded;
            if (expanded)
            {
                IComponentDescriptor componentDescriptor = GetComponentDescriptor();
                PropertyDescriptor[] descriptors = GetPropertyDescriptors(ComponentType, this, m_converter);
                if(ExpanderGraphics != null)
                {
                    ExpanderGraphics.SetActive(true);
                }
                
                BuildEditor(componentDescriptor, descriptors);
            }
            else
            {
                DestroyEditor();
            }
        }

        private void OnRedoCompleted()
        {
            ReloadEditors(false);
        }

        private void OnUndoCompleted()
        {
            ReloadEditors(false);
        }

        protected void OnReloadComponentEditor(ExposeToEditor obj, Component component, bool force)
        {
            if(component == Component)
            {
                ReloadEditors(force);
            }
        }

        private void ReloadEditors(bool force)
        {
            foreach (Transform t in EditorsPanel)
            {
                PropertyEditor propertyEditor = t.GetComponent<PropertyEditor>();
                if (propertyEditor != null)
                {
                    propertyEditor.Reload(force);
                }
            }
        }

        protected virtual void OnResetClick()
        {
            IInspectorModel inspectorModel = IOC.Resolve<IInspectorModel>();
            if (inspectorModel != null)
            {
                inspectorModel.NotifyBeginEdit(Components);
            }

            GameObject go = new GameObject();
            go.SetActive(false);

            Component defaultComponent = go.GetComponent(ComponentType);
            if (defaultComponent == null)
            {
                defaultComponent = go.AddComponent(ComponentType);
            }
            bool isMonoBehavior = defaultComponent is MonoBehaviour;

            PropertyDescriptor[] descriptors = GetPropertyDescriptors(ComponentType, this, m_converter);
            for (int i = 0; i < descriptors.Length; ++i)
            {
                PropertyDescriptor descriptor = descriptors[i];
                MemberInfo memberInfo = descriptor.ComponentMemberInfo;
                if(memberInfo is PropertyInfo)
                {
                    PropertyInfo p = (PropertyInfo)memberInfo;
                    foreach(Component component in Components)
                    {
                        if(component == null)
                        {
                            continue;
                        }

                        object defaultValue = p.GetValue(defaultComponent, null);
                        m_editor.Undo.BeginRecordValue(component, memberInfo);
                        p.SetValue(component, defaultValue, null);
                    }
                }
                else
                {
                    if (isMonoBehavior)
                    {
                        if(memberInfo is FieldInfo)
                        {
                            foreach (Component component in Components)
                            {
                                if (component == null)
                                {
                                    continue;
                                }

                                FieldInfo f = (FieldInfo)memberInfo;
                                object defaultValue = f.GetValue(defaultComponent);
                                m_editor.Undo.BeginRecordValue(component, memberInfo);
                                f.SetValue(component, defaultValue);
                            }
                        }
                    }
                }
            }

            for (int i = 0; i < descriptors.Length; ++i)
            {
                PropertyDescriptor descriptor = descriptors[i];
                MemberInfo memberInfo = descriptor.MemberInfo;
                PropertyEditor propertyEditor = GetPropertyEditor(memberInfo);
                if (propertyEditor != null)
                {
                    propertyEditor.Reload(true);
                }
            }

            Destroy(go);

            m_editor.Undo.BeginRecord();
            for (int i = 0; i < descriptors.Length; ++i)
            {
                PropertyDescriptor descriptor = descriptors[i];
                MemberInfo memberInfo = descriptor.ComponentMemberInfo;
                if (memberInfo is PropertyInfo)
                {
                    foreach (Component component in Components)
                    {
                        if (component == null)
                        {
                            continue;
                        }

                        m_editor.Undo.EndRecordValue(component, memberInfo);
                    }
                }
                else
                {
                    if(isMonoBehavior)
                    {
                        foreach (Component component in Components)
                        {
                            if (component == null)
                            {
                                continue;
                            }

                            m_editor.Undo.EndRecordValue(component, memberInfo);
                        }
                    }
                }
            }

            if (inspectorModel != null)
            {
                inspectorModel.NotifyEndEdit(Components);
            }

            m_editor.Undo.EndRecord();
       
        }

        protected virtual void OnRemove()
        {
            PropertyDescriptor[] descriptors = GetPropertyDescriptors(ComponentType, this, m_converter);

            Editor.Undo.BeginRecord();
            Component[] components = Components;
            for (int i = components.Length - 1; i >= 0; i--)
            {
                Component component = components[i];
                if (component == null)
                {
                    continue;
                }

                Editor.Undo.DestroyComponent(component, descriptors.Where(d => d.Targets != null && d.Targets[0] == (object)component).Select(d => d.ComponentMemberInfo).ToArray());
            }
           
            Editor.Undo.EndRecord();
        }

        protected virtual PropertyDescriptor[] GetPropertyDescriptors(Type componentType, ComponentEditor editor, object converter)
        {
            return m_editorsMap.GetPropertyDescriptors(ComponentType, editor, converter);
        }

        [Obsolete]
        protected internal Toggle Expander
        {
            get { return ExpanderToggle; }
            set { ExpanderToggle = value; }
        }

        [Obsolete]
        protected internal Image Icon
        {
            get { return IconImage; }
            set { IconImage = value; }
        }

        [Obsolete]
        protected virtual void AwakeOverride()
        {
        }

        [Obsolete]
        protected virtual void StartOverride()
        {
        }

        [Obsolete]
        protected virtual void OnDestroyOverride()
        {
        }

        [Obsolete]
        protected virtual void UpdateOverride()
        {

        }
    }

}
