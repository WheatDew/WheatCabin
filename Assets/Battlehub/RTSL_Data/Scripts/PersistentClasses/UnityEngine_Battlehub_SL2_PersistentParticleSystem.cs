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
    public partial class PersistentParticleSystem<TID> : PersistentComponent<TID>
    {
        [ProtoMember(256)]
        public float time;

        [ProtoMember(257)]
        public uint randomSeed;

        [ProtoMember(258)]
        public bool useAutoRandomSeed;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            ParticleSystem uo = (ParticleSystem)obj;
            time = uo.time;
            randomSeed = uo.randomSeed;
            useAutoRandomSeed = uo.useAutoRandomSeed;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            ParticleSystem uo = (ParticleSystem)obj;
            uo.time = time;
            uo.randomSeed = randomSeed;
            uo.useAutoRandomSeed = useAutoRandomSeed;
            return uo;
        }
    }
}

