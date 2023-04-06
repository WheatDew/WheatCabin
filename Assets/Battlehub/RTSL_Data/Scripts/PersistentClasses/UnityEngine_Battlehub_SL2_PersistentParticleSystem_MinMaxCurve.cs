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
    public partial class PersistentParticleSystemNestedMinMaxCurve<TID> : PersistentSurrogate<TID>
    {
        [ProtoMember(257)]
        public ParticleSystemCurveMode mode;

        [ProtoMember(258)]
        public float curveMultiplier;

        [ProtoMember(259)]
        public PersistentAnimationCurve<TID> curveMax;

        [ProtoMember(260)]
        public PersistentAnimationCurve<TID> curveMin;

        [ProtoMember(261)]
        public float constantMax;

        [ProtoMember(262)]
        public float constantMin;

        [ProtoMember(263)]
        public float constant;

        [ProtoMember(264)]
        public PersistentAnimationCurve<TID> curve;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            ParticleSystem.MinMaxCurve uo = (ParticleSystem.MinMaxCurve)obj;
            mode = uo.mode;
            curveMultiplier = uo.curveMultiplier;
            curveMax = uo.curveMax;
            curveMin = uo.curveMin;
            constantMax = uo.constantMax;
            constantMin = uo.constantMin;
            constant = uo.constant;
            curve = uo.curve;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            ParticleSystem.MinMaxCurve uo = (ParticleSystem.MinMaxCurve)obj;
            uo.mode = mode;
            uo.curveMultiplier = curveMultiplier;
            uo.curveMax = curveMax;
            uo.curveMin = curveMin;
            uo.constantMax = constantMax;
            uo.constantMin = constantMin;
            uo.constant = constant;
            uo.curve = curve;
            return uo;
        }

        public static implicit operator ParticleSystem.MinMaxCurve(PersistentParticleSystemNestedMinMaxCurve<TID> surrogate)
        {
            if(surrogate == null) return default(ParticleSystem.MinMaxCurve);
            return (ParticleSystem.MinMaxCurve)surrogate.WriteTo(new ParticleSystem.MinMaxCurve());
        }
        
        public static implicit operator PersistentParticleSystemNestedMinMaxCurve<TID>(ParticleSystem.MinMaxCurve obj)
        {
            PersistentParticleSystemNestedMinMaxCurve<TID> surrogate = new PersistentParticleSystemNestedMinMaxCurve<TID>();
            surrogate.ReadFrom(obj);
            return surrogate;
        }
    }
}

