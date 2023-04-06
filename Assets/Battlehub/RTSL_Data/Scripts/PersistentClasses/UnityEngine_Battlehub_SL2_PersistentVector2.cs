using System.Collections.Generic;
using ProtoBuf;
using Battlehub.RTSL;
using UnityEngine;
using UnityEngine.Battlehub.SL2;
using System;

using UnityObject = UnityEngine.Object;
namespace UnityEngine.Battlehub.SL2
{
    [ProtoContract]
    public partial class PersistentVector2<TID> : PersistentSurrogate<TID>
    {
        [ProtoMember(256)]
        public float x;

        [ProtoMember(257)]
        public float y;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            Vector2 uo = (Vector2)obj;
            x = uo.x;
            y = uo.y;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            Vector2 uo = (Vector2)obj;
            uo.x = x;
            uo.y = y;
            return uo;
        }

        public static implicit operator Vector2(PersistentVector2<TID> surrogate)
        {
            if(surrogate == null) return default(Vector2);
            return (Vector2)surrogate.WriteTo(new Vector2());
        }
        
        public static implicit operator PersistentVector2<TID>(Vector2 obj)
        {
            PersistentVector2<TID> surrogate = new PersistentVector2<TID>();
            surrogate.ReadFrom(obj);
            return surrogate;
        }
    }
}

