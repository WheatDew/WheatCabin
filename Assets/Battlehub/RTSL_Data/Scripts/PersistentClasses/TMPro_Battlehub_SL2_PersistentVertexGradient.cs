using System.Collections.Generic;
using ProtoBuf;
using Battlehub.RTSL;
using TMPro;
using TMPro.Battlehub.SL2;
using UnityEngine;
using UnityEngine.Battlehub.SL2;

using UnityObject = UnityEngine.Object;
namespace TMPro.Battlehub.SL2
{
    [ProtoContract]
    public partial class PersistentVertexGradient<TID> : PersistentSurrogate<TID>
    {
        [ProtoMember(256)]
        public PersistentColor<TID> topLeft;

        [ProtoMember(257)]
        public PersistentColor<TID> topRight;

        [ProtoMember(258)]
        public PersistentColor<TID> bottomLeft;

        [ProtoMember(259)]
        public PersistentColor<TID> bottomRight;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            VertexGradient uo = (VertexGradient)obj;
            topLeft = uo.topLeft;
            topRight = uo.topRight;
            bottomLeft = uo.bottomLeft;
            bottomRight = uo.bottomRight;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            VertexGradient uo = (VertexGradient)obj;
            uo.topLeft = topLeft;
            uo.topRight = topRight;
            uo.bottomLeft = bottomLeft;
            uo.bottomRight = bottomRight;
            return uo;
        }

        public static implicit operator VertexGradient(PersistentVertexGradient<TID> surrogate)
        {
            if(surrogate == null) return default(VertexGradient);
            return (VertexGradient)surrogate.WriteTo(new VertexGradient());
        }
        
        public static implicit operator PersistentVertexGradient<TID>(VertexGradient obj)
        {
            PersistentVertexGradient<TID> surrogate = new PersistentVertexGradient<TID>();
            surrogate.ReadFrom(obj);
            return surrogate;
        }
    }
}

