using ProtoBuf;
using UnityObject = UnityEngine.Object;
using Battlehub.RTSL;
using System;

namespace UnityEngine.Battlehub.SL2
{
     [ProtoContract]    
    public class PersistentObject<TID> : PersistentSurrogate<TID>
    {
        [ProtoMember(1)]
        public string name;

        [ProtoMember(2)]
        public int hideFlags;

        protected override void ReadFromImpl(object obj)
        {
            UnityObject uo = obj as UnityObject;
            if(uo != null)
            {
                name = uo.name;
                hideFlags = (int)uo.hideFlags;
            }
        }

        protected override object WriteToImpl(object obj)
        {
            UnityObject uo = (UnityObject)obj;
            uo.name = name;
            uo.hideFlags = (HideFlags)hideFlags;
            return obj;
        }  
    }

    [Obsolete("Use generic version")]
    [ProtoContract]
    public class PersistentObject : PersistentObject<long>
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
