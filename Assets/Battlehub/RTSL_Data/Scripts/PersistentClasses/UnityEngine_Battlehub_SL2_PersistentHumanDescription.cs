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
    public partial class PersistentHumanDescription<TID> : PersistentSurrogate<TID>
    {
        [ProtoMember(256)]
        public PersistentHumanBone<TID>[] human;

        [ProtoMember(257)]
        public PersistentSkeletonBone<TID>[] skeleton;

        [ProtoMember(270)]
        public float upperArmTwist;

        [ProtoMember(271)]
        public float lowerArmTwist;

        [ProtoMember(272)]
        public float upperLegTwist;

        [ProtoMember(273)]
        public float lowerLegTwist;

        [ProtoMember(274)]
        public float armStretch;

        [ProtoMember(275)]
        public float legStretch;

        [ProtoMember(276)]
        public float feetSpacing;

        [ProtoMember(277)]
        public bool hasTranslationDoF;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            HumanDescription uo = (HumanDescription)obj;
            human = Assign(uo.human, v_ => (PersistentHumanBone<TID>)v_);
            skeleton = Assign(uo.skeleton, v_ => (PersistentSkeletonBone<TID>)v_);
            upperArmTwist = uo.upperArmTwist;
            lowerArmTwist = uo.lowerArmTwist;
            upperLegTwist = uo.upperLegTwist;
            lowerLegTwist = uo.lowerLegTwist;
            armStretch = uo.armStretch;
            legStretch = uo.legStretch;
            feetSpacing = uo.feetSpacing;
            hasTranslationDoF = uo.hasTranslationDoF;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            HumanDescription uo = (HumanDescription)obj;
            uo.human = Assign(human, v_ => (HumanBone)v_);
            uo.skeleton = Assign(skeleton, v_ => (SkeletonBone)v_);
            uo.upperArmTwist = upperArmTwist;
            uo.lowerArmTwist = lowerArmTwist;
            uo.upperLegTwist = upperLegTwist;
            uo.lowerLegTwist = lowerLegTwist;
            uo.armStretch = armStretch;
            uo.legStretch = legStretch;
            uo.feetSpacing = feetSpacing;
            uo.hasTranslationDoF = hasTranslationDoF;
            return uo;
        }

        public static implicit operator HumanDescription(PersistentHumanDescription<TID> surrogate)
        {
            if(surrogate == null) return default(HumanDescription);
            return (HumanDescription)surrogate.WriteTo(new HumanDescription());
        }
        
        public static implicit operator PersistentHumanDescription<TID>(HumanDescription obj)
        {
            PersistentHumanDescription<TID> surrogate = new PersistentHumanDescription<TID>();
            surrogate.ReadFrom(obj);
            return surrogate;
        }
    }
}

