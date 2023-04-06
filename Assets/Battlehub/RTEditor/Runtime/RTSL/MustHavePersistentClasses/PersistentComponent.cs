using Battlehub.RTSL;
using ProtoBuf;
using System;

namespace UnityEngine.Battlehub.SL2
{
    [ProtoContract]
    public partial class PersistentComponent<TID> : PersistentObject<TID>
    {
        public static implicit operator PersistentComponent<TID>(Component obj)
        {
            PersistentComponent<TID> surrogate = new PersistentComponent<TID>();
            surrogate.ReadFrom(obj);
            return surrogate;
        }
    }

    [Obsolete("Use generic version")]
    [ProtoContract]
    public partial class PersistentComponent : PersistentComponent<long>
    {
        //For compatibility
        public virtual void GetDeps(GetDepsContext context)
        {
            GetDepsContext<long> ctx = context;
            GetDeps(ctx);
        }

        protected virtual void GetDepsImpl(GetDepsContext context)
        {
            GetDepsContext<long> ctx = context;
            GetDepsImpl(ctx);
        }
    }
}

