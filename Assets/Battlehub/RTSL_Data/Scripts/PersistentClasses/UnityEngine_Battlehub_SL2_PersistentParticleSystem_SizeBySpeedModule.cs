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
    public partial class PersistentParticleSystemNestedSizeBySpeedModule<TID> : PersistentSurrogate<TID>
    {
        [ProtoMember(256)]
        public bool enabled;

        [ProtoMember(257)]
        public PersistentParticleSystemNestedMinMaxCurve<TID> size;

        [ProtoMember(258)]
        public float sizeMultiplier;

        [ProtoMember(259)]
        public PersistentParticleSystemNestedMinMaxCurve<TID> x;

        [ProtoMember(260)]
        public float xMultiplier;

        [ProtoMember(261)]
        public PersistentParticleSystemNestedMinMaxCurve<TID> y;

        [ProtoMember(262)]
        public float yMultiplier;

        [ProtoMember(263)]
        public PersistentParticleSystemNestedMinMaxCurve<TID> z;

        [ProtoMember(264)]
        public float zMultiplier;

        [ProtoMember(265)]
        public bool separateAxes;

        [ProtoMember(266)]
        public PersistentVector2<TID> range;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            ParticleSystem.SizeBySpeedModule uo = (ParticleSystem.SizeBySpeedModule)obj;
            enabled = uo.enabled;
            size = uo.size;
            sizeMultiplier = uo.sizeMultiplier;
            x = uo.x;
            xMultiplier = uo.xMultiplier;
            y = uo.y;
            yMultiplier = uo.yMultiplier;
            z = uo.z;
            zMultiplier = uo.zMultiplier;
            separateAxes = uo.separateAxes;
            range = uo.range;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            ParticleSystem.SizeBySpeedModule uo = (ParticleSystem.SizeBySpeedModule)obj;
            uo.enabled = enabled;
            uo.size = size;
            uo.sizeMultiplier = sizeMultiplier;
            uo.x = x;
            uo.xMultiplier = xMultiplier;
            uo.y = y;
            uo.yMultiplier = yMultiplier;
            uo.z = z;
            uo.zMultiplier = zMultiplier;
            uo.separateAxes = separateAxes;
            uo.range = range;
            return uo;
        }

        public static implicit operator ParticleSystem.SizeBySpeedModule(PersistentParticleSystemNestedSizeBySpeedModule<TID> surrogate)
        {
            if(surrogate == null) return default(ParticleSystem.SizeBySpeedModule);
            return (ParticleSystem.SizeBySpeedModule)surrogate.WriteTo(new ParticleSystem.SizeBySpeedModule());
        }
        
        public static implicit operator PersistentParticleSystemNestedSizeBySpeedModule<TID>(ParticleSystem.SizeBySpeedModule obj)
        {
            PersistentParticleSystemNestedSizeBySpeedModule<TID> surrogate = new PersistentParticleSystemNestedSizeBySpeedModule<TID>();
            surrogate.ReadFrom(obj);
            return surrogate;
        }
    }
}

