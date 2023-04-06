using System.Collections.Generic;
using ProtoBuf;
using Battlehub.RTSL;
using UnityEngine;
using UnityEngine.Battlehub.SL2;

using UnityObject = UnityEngine.Object;
namespace UnityEngine.Battlehub.SL2
{
    [ProtoContract]
    public partial class PersistentParticleSystemNestedMinMaxGradient<TID> : PersistentSurrogate<TID>
    {
        [ProtoMember(256)]
        public ParticleSystemGradientMode mode;

        [ProtoMember(257)]
        public PersistentGradient<TID> gradientMax;

        [ProtoMember(258)]
        public PersistentGradient<TID> gradientMin;

        [ProtoMember(259)]
        public PersistentColor<TID> colorMax;

        [ProtoMember(260)]
        public PersistentColor<TID> colorMin;

        [ProtoMember(261)]
        public PersistentColor<TID> color;

        [ProtoMember(262)]
        public PersistentGradient<TID> gradient;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            ParticleSystem.MinMaxGradient uo = (ParticleSystem.MinMaxGradient)obj;
            mode = uo.mode;
            gradientMax = uo.gradientMax;
            gradientMin = uo.gradientMin;
            colorMax = uo.colorMax;
            colorMin = uo.colorMin;
            color = uo.color;
            gradient = uo.gradient;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            ParticleSystem.MinMaxGradient uo = (ParticleSystem.MinMaxGradient)obj;
            uo.mode = mode;
            uo.gradientMax = gradientMax;
            uo.gradientMin = gradientMin;
            uo.colorMax = colorMax;
            uo.colorMin = colorMin;
            uo.color = color;
            uo.gradient = gradient;
            return uo;
        }

        public static implicit operator ParticleSystem.MinMaxGradient(PersistentParticleSystemNestedMinMaxGradient<TID> surrogate)
        {
            if(surrogate == null) return default(ParticleSystem.MinMaxGradient);
            return (ParticleSystem.MinMaxGradient)surrogate.WriteTo(new ParticleSystem.MinMaxGradient());
        }
        
        public static implicit operator PersistentParticleSystemNestedMinMaxGradient<TID>(ParticleSystem.MinMaxGradient obj)
        {
            PersistentParticleSystemNestedMinMaxGradient<TID> surrogate = new PersistentParticleSystemNestedMinMaxGradient<TID>();
            surrogate.ReadFrom(obj);
            return surrogate;
        }
    }
}

