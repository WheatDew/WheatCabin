using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.Events;

using UnityObject = UnityEngine.Object;

namespace Battlehub.Utils
{
    public class ArgumentsCache
    {
        private static bool m_isFieldInfoInitialized;
        private static FieldInfo m_boolArgumentFieldInfo;
        private static FieldInfo m_floatArgumentFieldInfo;
        private static FieldInfo m_intArgumentFieldInfo;
        private static FieldInfo m_stringArgumentFieldInfo;
        private static FieldInfo m_objectArgumentFieldInfo;
        private static FieldInfo m_objectArgumentAssemblyTypeNameFieldInfo;

        internal static void Initialize(Type type)
        {
            if (m_isFieldInfoInitialized)
            {
                return;
            }

            m_boolArgumentFieldInfo = type.GetField("m_BoolArgument", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (m_boolArgumentFieldInfo == null)
            {
                throw new NotSupportedException("m_BoolArgument FieldInfo not found.");
            }

            m_floatArgumentFieldInfo = type.GetField("m_FloatArgument", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (m_floatArgumentFieldInfo == null)
            {
                throw new NotSupportedException("m_FloatArgument FieldInfo not found.");
            }

            m_intArgumentFieldInfo = type.GetField("m_IntArgument", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (m_intArgumentFieldInfo == null)
            {
                throw new NotSupportedException("m_IntArgument FieldInfo not found.");
            }

            m_stringArgumentFieldInfo = type.GetField("m_StringArgument", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (m_stringArgumentFieldInfo == null)
            {
                throw new NotSupportedException("m_StringArgument FieldInfo not found.");
            }

            m_objectArgumentFieldInfo = type.GetField("m_ObjectArgument", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (m_objectArgumentFieldInfo == null)
            {
                throw new NotSupportedException("m_ObjectArgument FieldInfo not found.");
            }

            m_objectArgumentAssemblyTypeNameFieldInfo = type.GetField("m_ObjectArgumentAssemblyTypeName", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (m_objectArgumentAssemblyTypeNameFieldInfo == null)
            {
                throw new NotSupportedException("m_ObjectArgumentAssemblyTypeName FieldInfo not found.");
            }

            m_isFieldInfoInitialized = true;
        }

        public bool BoolArgument
        {
            get;
            set;
        }

        public float FloatArgument
        {
            get;
            set;
        }

        public int IntArgument
        {
            get;
            set;
        }

        public string StringArgument
        {
            get;
            set;
        }

        public UnityObject ObjectArgument
        {
            get;
            set;
        }

        public string ObjectArgumentAssemblyTypeName
        {
            get;
            set;
        }

        public void ReadFrom(object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }

            BoolArgument = m_boolArgumentFieldInfo.GetValue<bool>(obj);
            FloatArgument = m_floatArgumentFieldInfo.GetValue<float>(obj);
            IntArgument = m_intArgumentFieldInfo.GetValue<int>(obj);
            StringArgument = m_stringArgumentFieldInfo.GetValue<string>(obj);
            ObjectArgument = m_objectArgumentFieldInfo.GetValue<UnityObject>(obj);
            ObjectArgumentAssemblyTypeName = Reflection.CleanAssemblyName(m_objectArgumentAssemblyTypeNameFieldInfo.GetValue<string>(obj));
        }

        public void WriteTo(object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }

            m_boolArgumentFieldInfo.SetValue(obj, BoolArgument);
            m_floatArgumentFieldInfo.SetValue(obj, FloatArgument);
            m_intArgumentFieldInfo.SetValue(obj, IntArgument);
            m_stringArgumentFieldInfo.SetValue(obj, StringArgument);
            m_objectArgumentFieldInfo.SetValue(obj, ObjectArgument);
            m_objectArgumentAssemblyTypeNameFieldInfo.SetValue(obj, ObjectArgumentAssemblyTypeName);
        }
    }

    public class PersistentCall
    {
        private static bool m_isFieldInfoInitialized;
        private static FieldInfo m_argumentsFieldInfo;
        private static FieldInfo m_callStateFieldInfo;
        private static FieldInfo m_methodNameFieldInfo;
        private static FieldInfo m_modeFieldInfo;
        private static FieldInfo m_targetFieldInfo;

        public static PersistentCall CreateNew()
        {
            return new PersistentCall
            {
                ArgumentsCache = new ArgumentsCache(),
                CallState = UnityEventCallState.RuntimeOnly,
                Mode = PersistentListenerMode.EventDefined,
                MethodName = string.Empty,
            };
        }

        internal static void Initialize(Type type)
        {
            if (m_isFieldInfoInitialized)
            {
                return;
            }

            m_argumentsFieldInfo = type.GetField("m_Arguments", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (m_argumentsFieldInfo == null)
            {
                throw new NotSupportedException("m_Arguments FieldInfo not found.");
            }
            m_callStateFieldInfo = type.GetField("m_CallState", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (m_callStateFieldInfo == null)
            {
                throw new NotSupportedException("m_CallState FieldInfo not found.");
            }
            m_methodNameFieldInfo = type.GetField("m_MethodName", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (m_methodNameFieldInfo == null)
            {
                throw new NotSupportedException("m_MethodName FieldInfo not found.");
            }
            m_modeFieldInfo = type.GetField("m_Mode", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (m_modeFieldInfo == null)
            {
                throw new NotSupportedException("m_Mode FieldInfo not found.");
            }
            m_targetFieldInfo = type.GetField("m_Target", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (m_targetFieldInfo == null)
            {
                throw new NotSupportedException("m_Target FieldInfo not found.");
            }

            m_isFieldInfoInitialized = true;
            ArgumentsCache.Initialize(m_argumentsFieldInfo.FieldType);
        }

        public ArgumentsCache ArgumentsCache
        {
            get;
            set;
        }

        public UnityEventCallState CallState
        {
            get;
            set;
        }


        private string m_methodName;
        public string MethodName
        {
            get { return m_methodName; }
            set
            { 
                if(m_methodName != value)
                {
                    m_methodName = value;

                    //TODO: Add support for other methods.
                    Mode = PersistentListenerMode.EventDefined; 
                }
                
            }
        }

        public PersistentListenerMode Mode
        {
            get;
            set;
        }

        private UnityObject m_target;
        public UnityObject Target
        {
            get { return m_target; }
            set
            {
                if (m_target != value)
                {
                    m_target = value;
                    MethodName = string.Empty;
                }
            }
        }

        public void ReadFrom(object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }

            object argumentsCache = m_argumentsFieldInfo.GetValue(obj);
            if (argumentsCache != null)
            {
                ArgumentsCache = new ArgumentsCache();
                ArgumentsCache.ReadFrom(argumentsCache);
            }
            else
            {
                ArgumentsCache = null;
            }

            m_target = m_targetFieldInfo.GetValue<UnityObject>(obj);
            CallState = m_callStateFieldInfo.GetValue<UnityEventCallState>(obj);
            m_methodName = m_methodNameFieldInfo.GetValue<string>(obj);
            Mode = m_modeFieldInfo.GetValue<PersistentListenerMode>(obj);
        }

        public void WriteTo(object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }

            object arguments = null;
            if (ArgumentsCache != null)
            {
                arguments = Activator.CreateInstance(m_argumentsFieldInfo.FieldType);
                ArgumentsCache.WriteTo(arguments);
            }
            m_argumentsFieldInfo.SetValue(obj, arguments);
            m_callStateFieldInfo.SetValue(obj, CallState);
            m_methodNameFieldInfo.SetValue(obj, MethodName);
            m_modeFieldInfo.SetValue(obj, Mode);
            m_targetFieldInfo.SetValue(obj, Target);
        }
    }

    public static class UnityEventBaseExtensions
    {
        private static readonly FieldInfo m_persistentCallGroupInfo;
        private static readonly FieldInfo m_callsInfo;
        private static readonly Type m_callType;
        
        static UnityEventBaseExtensions()
        {
            m_persistentCallGroupInfo = typeof(UnityEventBase).GetField("m_PersistentCalls", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            if (m_persistentCallGroupInfo == null)
            {
                throw new NotSupportedException("m_PersistentCalls FieldInfo not found.");
            }

            Type persistentCallsType = m_persistentCallGroupInfo.FieldType;
            m_callsInfo = persistentCallsType.GetField("m_Calls", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (m_callsInfo == null)
            {
                throw new NotSupportedException("m_Calls FieldInfo not found. ");
            }

            Type callsType = m_callsInfo.FieldType;
            if (!callsType.IsGenericType() || callsType.GetGenericTypeDefinition() != typeof(List<>))
            {
                throw new NotSupportedException("m_callsInfo.FieldType is not a generic List<>");
            }

            m_callType = callsType.GetGenericArguments()[0];
            PersistentCall.Initialize(m_callType);
        }

        public static void AddPersistentCall(this UnityEventBase obj, PersistentCall call)
        {
            PersistentCall[] calls = obj.GetPersistentCalls();
            Array.Resize(ref calls, calls.Length + 1);
            calls[calls.Length - 1] = call;
            obj.SetPersistentCalls(calls);
        }

        public static void RemovePersistentCall(this UnityEventBase obj, int index)
        {
            PersistentCall[] calls = obj.GetPersistentCalls();
            for(int i = index; i < calls.Length - 1; ++i)
            {
                calls[i] = calls[i + 1];
            }
            Array.Resize(ref calls, calls.Length - 1);
            obj.SetPersistentCalls(calls);
        }

        public static PersistentCall[] GetPersistentCalls(this UnityEventBase obj)
        {
            object persistentCalls = m_persistentCallGroupInfo.GetValue(obj);
            if (persistentCalls == null)
            {
                return new PersistentCall[0];
            }

            object calls = m_callsInfo.GetValue(persistentCalls);
            if (calls == null)
            {
                return new PersistentCall[0];
            }

            IList list = (IList)calls;
            PersistentCall[] result = new PersistentCall[list.Count];
            for (int i = 0; i < result.Length; ++i)
            {
                object call = list[i];
                if (call != null)
                {
                    PersistentCall persistentCall = new PersistentCall();
                    persistentCall.ReadFrom(call);
                    result[i] = persistentCall;
                }
            }
            return result;
        }

        public static void SetPersistentCalls(this UnityEventBase obj, PersistentCall[] persistentCalls)
        {
            object persistentCallGroup = Activator.CreateInstance(m_persistentCallGroupInfo.FieldType);
            object calls = Activator.CreateInstance(m_callsInfo.FieldType);

            IList list = (IList)calls;
            for (int i = 0; i < persistentCalls.Length; ++i)
            {
                object call = null;
                PersistentCall persistentCall = persistentCalls[i];
                if (persistentCall != null)
                {
                    call = Activator.CreateInstance(m_callType);
                    persistentCall.WriteTo(call);
                }
                list.Add(call);
            }

            m_callsInfo.SetValue(persistentCallGroup, list);
            m_persistentCallGroupInfo.SetValue(obj, persistentCallGroup);
        }
    }
}
