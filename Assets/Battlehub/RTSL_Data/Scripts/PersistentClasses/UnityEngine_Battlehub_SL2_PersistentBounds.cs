using System.Collections.Generic;
using ProtoBuf;
using Battlehub.RTSL;
using UnityEngine;
using UnityEngine.Battlehub.SL2;

using UnityObject = UnityEngine.Object;
namespace UnityEngine.Battlehub.SL2
{
    [ProtoContract]
    public partial class PersistentBounds<TID> : PersistentSurrogate<TID>
    {
        [ProtoMember(256)]
        public PersistentVector3<TID> center;

        [ProtoMember(258)]
        public PersistentVector3<TID> extents;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            Bounds uo = (Bounds)obj;
            center = uo.center;
            extents = uo.extents;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            Bounds uo = (Bounds)obj;
            uo.center = center;
            uo.extents = extents;
            return uo;
        }

        public static implicit operator Bounds(PersistentBounds<TID> surrogate)
        {
            if(surrogate == null) return default(Bounds);
            return (Bounds)surrogate.WriteTo(new Bounds());
        }
        
        public static implicit operator PersistentBounds<TID>(Bounds obj)
        {
            PersistentBounds<TID> surrogate = new PersistentBounds<TID>();
            surrogate.ReadFrom(obj);
            return surrogate;
        }
    }
}

