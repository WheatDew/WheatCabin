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
    public partial class PersistentTreeInstance<TID> : PersistentSurrogate<TID>
    {
        [ProtoMember(256)]
        public PersistentVector3<TID> position;

        [ProtoMember(257)]
        public float widthScale;

        [ProtoMember(258)]
        public float heightScale;

        [ProtoMember(259)]
        public float rotation;

        [ProtoMember(260)]
        public PersistentColor32<TID> color;

        [ProtoMember(261)]
        public PersistentColor32<TID> lightmapColor;

        [ProtoMember(262)]
        public int prototypeIndex;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            TreeInstance uo = (TreeInstance)obj;
            position = uo.position;
            widthScale = uo.widthScale;
            heightScale = uo.heightScale;
            rotation = uo.rotation;
            color = uo.color;
            lightmapColor = uo.lightmapColor;
            prototypeIndex = uo.prototypeIndex;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            TreeInstance uo = (TreeInstance)obj;
            uo.position = position;
            uo.widthScale = widthScale;
            uo.heightScale = heightScale;
            uo.rotation = rotation;
            uo.color = color;
            uo.lightmapColor = lightmapColor;
            uo.prototypeIndex = prototypeIndex;
            return uo;
        }

        public static implicit operator TreeInstance(PersistentTreeInstance<TID> surrogate)
        {
            if(surrogate == null) return default(TreeInstance);
            return (TreeInstance)surrogate.WriteTo(new TreeInstance());
        }
        
        public static implicit operator PersistentTreeInstance<TID>(TreeInstance obj)
        {
            PersistentTreeInstance<TID> surrogate = new PersistentTreeInstance<TID>();
            surrogate.ReadFrom(obj);
            return surrogate;
        }
    }
}

