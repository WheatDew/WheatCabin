using ProtoBuf;
using System;
using Battlehub.RTSL;
using Battlehub.Utils;

using UnityObject = UnityEngine.Object;

namespace UnityEngine.Events.Battlehub.SL2
{
    [ProtoContract]
    public class PersistentArgumentCache<TID> : PersistentSurrogate<TID>
    {
        [ProtoMember(1)]
        public bool m_BoolArgument;
        [ProtoMember(2)]
        public float m_FloatArgument;
        [ProtoMember(3)]
        public int m_IntArgument;
        [ProtoMember(4)]
        public string m_StringArgument;
        [ProtoMember(5)]
        public TID m_ObjectArgument; //instanceId
        [ProtoMember(6)]
        public string m_ObjectArgumentAssemblyTypeName;  
   
        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            if (obj == null)
            {
                m_BoolArgument = false;
                m_FloatArgument = 0;
                m_IntArgument = 0;
                m_StringArgument = null;
                m_ObjectArgument = default(TID);
                m_ObjectArgumentAssemblyTypeName = null;
                return;
            }

            ArgumentsCache cache = (ArgumentsCache)obj;
            m_BoolArgument = cache.BoolArgument;
            m_FloatArgument = cache.FloatArgument;
            m_IntArgument = cache.IntArgument;
            m_StringArgument = cache.StringArgument;
            m_ObjectArgument = ToID(cache.ObjectArgument);
            m_ObjectArgumentAssemblyTypeName = cache.ObjectArgumentAssemblyTypeName;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            if (obj == null)
            {
                return obj;
            }

            ArgumentsCache cache = (ArgumentsCache)obj;
            cache.BoolArgument = m_BoolArgument;
            cache.FloatArgument = m_FloatArgument;
            cache.IntArgument = m_IntArgument;
            cache.StringArgument = m_StringArgument;
            cache.ObjectArgument = FromID<UnityObject>(m_ObjectArgument);
            cache.ObjectArgumentAssemblyTypeName = cache.ObjectArgumentAssemblyTypeName;

            return obj;
        }

        protected override void GetDepsFromImpl(object obj, GetDepsFromContext context)
        {
            base.GetDepsFromImpl(obj, context);
            if (obj == null)
            {
                return;
            }

            ArgumentsCache cache = (ArgumentsCache)obj;
            AddDep(cache.ObjectArgument, context);
        }

        protected override void GetDepsImpl(GetDepsContext<TID> context)
        {
            base.GetDepsImpl(context);
            AddDep(m_ObjectArgument, context);
        }
    }

    [ProtoContract]
    public class PersistentPersistentCall<TID> : PersistentSurrogate<TID>
    {
        [ProtoMember(1)]
        public PersistentArgumentCache<TID> m_Arguments;
        [ProtoMember(2)]
        public UnityEventCallState m_CallState;
        [ProtoMember(3)]
        public string m_MethodName;
        [ProtoMember(4)]
        public PersistentListenerMode m_Mode;
        [ProtoMember(5)]
        public TID m_Target; //instanceId
        [ProtoMember(6)]
        public string TypeName;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            if (obj == null)
            {
                m_Arguments = default;
                m_CallState = default;
                m_MethodName = null;
                m_Mode = default;
                m_Target = default;
                return;
            }

            PersistentCall call = (PersistentCall)obj;

            m_Arguments = new PersistentArgumentCache<TID>();
            m_Arguments.ReadFrom(call.ArgumentsCache);
            m_CallState = call.CallState;
            m_MethodName = call.MethodName;
            m_Mode = call.Mode;
            m_Target = ToID(call.Target);
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            if (obj == null)
            {
                return obj;
            }

            PersistentCall call = (PersistentCall)obj;
            call.ArgumentsCache = new ArgumentsCache();
            if (m_Arguments != null)
            {
                m_Arguments.WriteTo(call.ArgumentsCache);
            }

            call.Target = FromID<UnityObject>(m_Target);
            call.CallState = m_CallState;
            call.MethodName = m_MethodName;
            call.Mode = m_Mode;
            
            return obj;
        }

        protected override void GetDepsFromImpl(object obj, GetDepsFromContext context)
        {
            base.GetDepsFromImpl(obj, context);
            if (obj == null)
            {
                return;
            }

            PersistentCall call = (PersistentCall)obj;

            PersistentArgumentCache<TID> args = new PersistentArgumentCache<TID>();
            args.GetDepsFrom(call.ArgumentsCache, context);
            AddDep(call.Target, context);
        }

        public override void GetDeps(GetDepsContext<TID> context)
        {
            base.GetDeps(context);
            if (m_Arguments != null)
            {
                m_Arguments.GetDeps(context);
            }

            AddDep(m_Target, context);
        }
    }

    [ProtoContract]
    public class PersistentUnityEventBase<TID> : PersistentSurrogate<TID>
    {
        [ProtoMember(1)]
        public PersistentPersistentCall<TID>[] m_calls;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            if (obj == null)
            {
                return;
            }

            UnityEventBase eventBase = (UnityEventBase)obj;
            PersistentCall[] calls = eventBase.GetPersistentCalls();

            m_calls = new PersistentPersistentCall<TID>[calls.Length];
            for (int i = 0; i < calls.Length; ++i)
            {
                PersistentCall call = calls[i];
                if(call != null)
                {
                    PersistentPersistentCall<TID> persistentCall = new PersistentPersistentCall<TID>();
                    persistentCall.ReadFrom(call);
                    m_calls[i] = persistentCall;
                }
            }
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            if (obj == null)
            {
                return null;
            }

            UnityEventBase eventBase = (UnityEventBase)obj;
            if (m_calls == null)
            {
                eventBase.SetPersistentCalls(new PersistentCall[0]);
                return obj;
            }

            PersistentCall[] calls = new PersistentCall[m_calls.Length];
            for (int i = 0; i < m_calls.Length; ++i)
            {
                PersistentPersistentCall<TID> persistentCall = m_calls[i];
                if (persistentCall != null)
                {
                    PersistentCall call = new PersistentCall();
                    persistentCall.WriteTo(call);
                    calls[i] = call;
                }
            }

            eventBase.SetPersistentCalls(calls);
            return eventBase;
        }

        protected override void GetDepsFromImpl(object obj, GetDepsFromContext context)
        {
            base.GetDepsFromImpl(obj, context);
            if (obj == null)
            {
                return;
            }

            UnityEventBase eventBase = (UnityEventBase)obj;
            PersistentCall[] calls = eventBase.GetPersistentCalls();
            for (int i = 0; i < calls.Length; ++i)
            {
                PersistentCall call = calls[i];
                PersistentPersistentCall<TID> persistentCall = new PersistentPersistentCall<TID>();
                persistentCall.GetDepsFrom(call, context);
            }
        }

        protected override void GetDepsImpl(GetDepsContext<TID> context)
        {
            base.GetDepsImpl(context);
            if (m_calls == null)
            {
                return;
            }

            for (int i = 0; i < m_calls.Length; ++i)
            {
                PersistentPersistentCall<TID> persistentCall = m_calls[i];
                if (persistentCall != null)
                {
                    persistentCall.GetDeps(context);
                }
            }
        }
    }

    [ProtoContract]
    public class PersistentUnityEvent<TID> : PersistentUnityEventBase<TID>
    {
        public static implicit operator UnityEvent(PersistentUnityEvent<TID> surrogate)
        {
            if (surrogate == null) return default(UnityEvent);
            return (UnityEvent)surrogate.WriteTo(new UnityEvent());
        }

        public static implicit operator PersistentUnityEvent<TID>(UnityEvent obj)
        {
            PersistentUnityEvent<TID> surrogate = new PersistentUnityEvent<TID>();
            surrogate.ReadFrom(obj);
            return surrogate;
        }
    }

    [Obsolete("Use generic version")]
    [ProtoContract]
    public class PersistentUnityEventBase : PersistentUnityEventBase<long>
    {
    }

    [Obsolete("Use generic version")]
    [ProtoContract]
    public class PersistentUnityEvent : PersistentUnityEvent<long>
    {
    }
}