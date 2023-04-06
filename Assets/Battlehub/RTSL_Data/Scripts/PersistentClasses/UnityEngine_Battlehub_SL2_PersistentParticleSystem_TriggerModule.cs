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
    public partial class PersistentParticleSystemNestedTriggerModule<TID> : PersistentSurrogate<TID>
    {
        [ProtoMember(256)]
        public bool enabled;

        [ProtoMember(257)]
        public ParticleSystemOverlapAction inside;

        [ProtoMember(258)]
        public ParticleSystemOverlapAction outside;

        [ProtoMember(259)]
        public ParticleSystemOverlapAction enter;

        [ProtoMember(260)]
        public ParticleSystemOverlapAction exit;

        [ProtoMember(261)]
        public float radiusScale;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            ParticleSystem.TriggerModule uo = (ParticleSystem.TriggerModule)obj;
            enabled = uo.enabled;
            inside = uo.inside;
            outside = uo.outside;
            enter = uo.enter;
            exit = uo.exit;
            radiusScale = uo.radiusScale;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            ParticleSystem.TriggerModule uo = (ParticleSystem.TriggerModule)obj;
            uo.enabled = enabled;
            uo.inside = inside;
            uo.outside = outside;
            uo.enter = enter;
            uo.exit = exit;
            uo.radiusScale = radiusScale;
            return uo;
        }

        public static implicit operator ParticleSystem.TriggerModule(PersistentParticleSystemNestedTriggerModule<TID> surrogate)
        {
            if(surrogate == null) return default(ParticleSystem.TriggerModule);
            return (ParticleSystem.TriggerModule)surrogate.WriteTo(new ParticleSystem.TriggerModule());
        }
        
        public static implicit operator PersistentParticleSystemNestedTriggerModule<TID>(ParticleSystem.TriggerModule obj)
        {
            PersistentParticleSystemNestedTriggerModule<TID> surrogate = new PersistentParticleSystemNestedTriggerModule<TID>();
            surrogate.ReadFrom(obj);
            return surrogate;
        }
    }
}

