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
    public partial class PersistentSkeletonBone<TID> : PersistentSurrogate<TID>
    {
        [ProtoMember(256)]
        public string name;

        [ProtoMember(257)]
        public PersistentVector3<TID> position;

        [ProtoMember(258)]
        public PersistentQuaternion<TID> rotation;

        [ProtoMember(259)]
        public PersistentVector3<TID> scale;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            SkeletonBone uo = (SkeletonBone)obj;
            name = uo.name;
            position = uo.position;
            rotation = uo.rotation;
            scale = uo.scale;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            SkeletonBone uo = (SkeletonBone)obj;
            uo.name = name;
            uo.position = position;
            uo.rotation = rotation;
            uo.scale = scale;
            return uo;
        }

        public static implicit operator SkeletonBone(PersistentSkeletonBone<TID> surrogate)
        {
            if(surrogate == null) return default(SkeletonBone);
            return (SkeletonBone)surrogate.WriteTo(new SkeletonBone());
        }
        
        public static implicit operator PersistentSkeletonBone<TID>(SkeletonBone obj)
        {
            PersistentSkeletonBone<TID> surrogate = new PersistentSkeletonBone<TID>();
            surrogate.ReadFrom(obj);
            return surrogate;
        }
    }
}

