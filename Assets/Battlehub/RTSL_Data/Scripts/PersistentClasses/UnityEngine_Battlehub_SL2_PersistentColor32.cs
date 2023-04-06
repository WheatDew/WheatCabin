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
    public partial class PersistentColor32<TID> : PersistentSurrogate<TID>
    {
        [ProtoMember(256)]
        public byte r;

        [ProtoMember(257)]
        public byte g;

        [ProtoMember(258)]
        public byte b;

        [ProtoMember(259)]
        public byte a;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            Color32 uo = (Color32)obj;
            r = uo.r;
            g = uo.g;
            b = uo.b;
            a = uo.a;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            Color32 uo = (Color32)obj;
            uo.r = r;
            uo.g = g;
            uo.b = b;
            uo.a = a;
            return uo;
        }

        public static implicit operator Color32(PersistentColor32<TID> surrogate)
        {
            if(surrogate == null) return default(Color32);
            return (Color32)surrogate.WriteTo(new Color32());
        }
        
        public static implicit operator PersistentColor32<TID>(Color32 obj)
        {
            PersistentColor32<TID> surrogate = new PersistentColor32<TID>();
            surrogate.ReadFrom(obj);
            return surrogate;
        }
    }
}

