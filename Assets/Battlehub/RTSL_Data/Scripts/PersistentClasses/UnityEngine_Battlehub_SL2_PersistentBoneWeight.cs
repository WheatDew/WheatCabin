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
    public partial class PersistentBoneWeight<TID> : PersistentSurrogate<TID>
    {
        [ProtoMember(256)]
        public float weight0;

        [ProtoMember(257)]
        public float weight1;

        [ProtoMember(258)]
        public float weight2;

        [ProtoMember(259)]
        public float weight3;

        [ProtoMember(260)]
        public int boneIndex0;

        [ProtoMember(261)]
        public int boneIndex1;

        [ProtoMember(262)]
        public int boneIndex2;

        [ProtoMember(263)]
        public int boneIndex3;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            BoneWeight uo = (BoneWeight)obj;
            weight0 = uo.weight0;
            weight1 = uo.weight1;
            weight2 = uo.weight2;
            weight3 = uo.weight3;
            boneIndex0 = uo.boneIndex0;
            boneIndex1 = uo.boneIndex1;
            boneIndex2 = uo.boneIndex2;
            boneIndex3 = uo.boneIndex3;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            BoneWeight uo = (BoneWeight)obj;
            uo.weight0 = weight0;
            uo.weight1 = weight1;
            uo.weight2 = weight2;
            uo.weight3 = weight3;
            uo.boneIndex0 = boneIndex0;
            uo.boneIndex1 = boneIndex1;
            uo.boneIndex2 = boneIndex2;
            uo.boneIndex3 = boneIndex3;
            return uo;
        }

        public static implicit operator BoneWeight(PersistentBoneWeight<TID> surrogate)
        {
            if(surrogate == null) return default(BoneWeight);
            return (BoneWeight)surrogate.WriteTo(new BoneWeight());
        }
        
        public static implicit operator PersistentBoneWeight<TID>(BoneWeight obj)
        {
            PersistentBoneWeight<TID> surrogate = new PersistentBoneWeight<TID>();
            surrogate.ReadFrom(obj);
            return surrogate;
        }
    }
}

