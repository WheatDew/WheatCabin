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
    public partial class PersistentRect<TID> : PersistentSurrogate<TID>
    {
        [ProtoMember(256)]
        public float x;

        [ProtoMember(257)]
        public float y;

        [ProtoMember(262)]
        public float width;

        [ProtoMember(263)]
        public float height;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            Rect uo = (Rect)obj;
            x = uo.x;
            y = uo.y;
            width = uo.width;
            height = uo.height;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            Rect uo = (Rect)obj;
            uo.x = x;
            uo.y = y;
            uo.width = width;
            uo.height = height;
            return uo;
        }

        public static implicit operator Rect(PersistentRect<TID> surrogate)
        {
            if(surrogate == null) return default(Rect);
            return (Rect)surrogate.WriteTo(new Rect());
        }
        
        public static implicit operator PersistentRect<TID>(Rect obj)
        {
            PersistentRect<TID> surrogate = new PersistentRect<TID>();
            surrogate.ReadFrom(obj);
            return surrogate;
        }
    }
}

