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
    public partial class PersistentLayerMask<TID> : PersistentSurrogate<TID>
    {
        [ProtoMember(256)]
        public int value;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            LayerMask uo = (LayerMask)obj;
            value = uo.value;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            LayerMask uo = (LayerMask)obj;
            uo.value = value;
            return uo;
        }

        public static implicit operator LayerMask(PersistentLayerMask<TID> surrogate)
        {
            if(surrogate == null) return default(LayerMask);
            return (LayerMask)surrogate.WriteTo(new LayerMask());
        }
        
        public static implicit operator PersistentLayerMask<TID>(LayerMask obj)
        {
            PersistentLayerMask<TID> surrogate = new PersistentLayerMask<TID>();
            surrogate.ReadFrom(obj);
            return surrogate;
        }
    }
}

