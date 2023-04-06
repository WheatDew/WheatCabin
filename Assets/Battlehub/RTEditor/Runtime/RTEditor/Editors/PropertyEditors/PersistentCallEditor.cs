using Battlehub.RTCommon;
using Battlehub.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

using UnityObject = UnityEngine.Object;
namespace Battlehub.RTEditor
{
    public class PersistentCallEditor : PropertyEditor<PersistentCall>
    {
        [SerializeField]
        private OptionsEditorString m_methodEditor = null;
        private CanvasGroup m_methodEditorCanvasGroup = null;

        [SerializeField]
        private ObjectEditor m_targetEditor = null;

        private Type m_unityEventType;
        private ILocalization m_localization;

        protected override void AwakeOverride()
        {
            base.AwakeOverride();
            m_localization = IOC.Resolve<ILocalization>();
        }

        protected override void InitOverride(object[] targets, object[] accessors, MemberInfo memberInfo, Action<object, object> eraseTargetCallback = null, string label = null)
        {
            base.InitOverride(targets, accessors, memberInfo, eraseTargetCallback, label);

            UnityEventBaseEditor.PersistentCallAccessor accessor = (UnityEventBaseEditor.PersistentCallAccessor)accessors[0];
            m_unityEventType = accessor.UnityEventType;

            InitTargetEditor();
            m_methodEditorCanvasGroup = m_methodEditor.gameObject.AddComponent<CanvasGroup>();
            UpdateControlsState();
        }

        private void InitTargetEditor()
        {
            PropertyInfo targetInfo = Strong.PropertyInfo((PersistentCall x) => x.Target);
            PropertyInfo valueInfo = Strong.PropertyInfo((CustomTypeFieldAccessor<PersistentCall> x) => x.Value);
            CustomTypeFieldAccessor[] targetAccessor = new CustomTypeFieldAccessor[Targets.Length];
            for (int i = 0; i < targetAccessor.Length; ++i)
            {
                targetAccessor[i] = new CustomTypeFieldAccessor<PersistentCall>(this, i, targetInfo, string.Empty);
            }
            m_targetEditor.Init(targetAccessor, valueInfo, string.Empty, false, OnBeginRecordValue, OnEndRecordValue, OnAfterRedo, OnAfterUndo);
        }

        private void InitMethodEditor()
        {
            PropertyInfo methodInfo = Strong.PropertyInfo((PersistentCall x) => x.MethodName);
            PropertyInfo valueInfo = Strong.PropertyInfo((CustomTypeFieldAccessor<PersistentCall> x) => x.Value);
            CustomTypeFieldAccessor[] methodNameAccessor = new CustomTypeFieldAccessor[Targets.Length];
            for (int i = 0; i < methodNameAccessor.Length; ++i)
            {
                methodNameAccessor[i] = new CustomTypeFieldAccessor<PersistentCall>(this, i, methodInfo, string.Empty);
            }
            m_methodEditor.Init(methodNameAccessor, valueInfo, string.Empty, false, OnBeginRecordValue, OnEndRecordValue, OnAfterRedo, OnAfterUndo);
        }

        private void OnBeginRecordValue()
        {
            BeginEdit();
        }

        private void OnEndRecordValue()
        {
            EndEdit();
            UpdateControlsState();
        }

        private void OnAfterUndo()
        {
            UpdateControlsState();
        }

        private void OnAfterRedo()
        {
            UpdateControlsState();
        }

        private void UpdateControlsState()
        {
            PersistentCall call = GetValue();
            if (call.Target == null && HasMixedValues())
            {
                m_methodEditorCanvasGroup.interactable = false;
                m_methodEditor.Options = new[] { new RangeOptions.Option(m_localization.GetString("ID_RTEditor_PE_PersistentCallEditor_NoFunction", "No Function"), string.Empty) };
                InitMethodEditor();
                m_methodEditor.SetValue(string.Empty);
            }
            else
            {
                m_methodEditorCanvasGroup.interactable = true;
                m_methodEditor.Options = GetMethodOptions();
                InitMethodEditor();
                m_methodEditor.SetValue(call.MethodName);
            }
        }

        private RangeOptions.Option[] GetMethodOptions()
        {
            List<RangeOptions.Option> methodsList = new List<RangeOptions.Option>();
            methodsList.Add(new RangeOptions.Option(m_localization.GetString("ID_RTEditor_PE_PersistentCallEditor_NoFunction", "No Function"), string.Empty));

            Type[] expectedTypes = m_unityEventType.GetGenericArguments();
            if(expectedTypes.Length == 0)
            {
                if(m_unityEventType.BaseType != null && m_unityEventType.BaseType.IsGenericType)
                {
                    expectedTypes = m_unityEventType.BaseType.GetGenericArguments();
                }
            }

            UnityObject target = GetValue().Target;
            if(target != null)
            {
                Type targetType = target.GetType();
                MethodInfo[] methods = targetType.GetMethods(BindingFlags.Public | BindingFlags.Instance);
                for (int i = 0; i < methods.Length; ++i)
                {
                    MethodInfo methodInfo = methods[i];
                    if (methodInfo.ReturnType != typeof(void))
                    {
                        continue;
                    }

                    ParameterInfo[] parameters = methodInfo.GetParameters();
                    if (!MatchParameters(expectedTypes, parameters))
                    {
                        continue;
                    }

                    if (methodInfo.IsSpecialName)
                    {
                        continue;
                    }

                    string text;
                    string value = methodInfo.Name;

                    if (parameters.Length == 0)
                    {
                        text = string.Format("{0} ()", methodInfo.Name);
                    }
                    else
                    {
                        text = string.Format("{0} ({1})", methodInfo.Name, string.Join(", ", parameters.Select(p => p.ParameterType.Name)));
                    }

                    methodsList.Add(new RangeOptions.Option(text, value));
                }

                if (expectedTypes.Length == 1)
                {
                    PropertyInfo[] properties = targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                    for(int i = 0; i < properties.Length; ++i)
                    {
                        PropertyInfo propertyInfo = properties[i];
                        if(propertyInfo.SetMethod == null)
                        {
                            continue;
                        }

                        if(!propertyInfo.PropertyType.IsAssignableFrom(expectedTypes[0]))
                        {
                            continue;
                        }

                        string text = string.Format("{1} {0}", propertyInfo.Name, propertyInfo.PropertyType.Name);
                        string value = propertyInfo.SetMethod.Name;

                        methodsList.Add(new RangeOptions.Option(text, value));
                    }
                }
            }

            return methodsList.ToArray();
        }

        private bool MatchParameters(Type[] expectedTypes, ParameterInfo[] parameters)
        {
            
            if (expectedTypes.Length != parameters.Length)
            {
                return false;
            }

            for (int i = 0; i < parameters.Length; ++i)
            {
                ParameterInfo param = parameters[i];
                Type expectedParamType = expectedTypes[i];
                if (!param.ParameterType.IsAssignableFrom(expectedParamType))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
