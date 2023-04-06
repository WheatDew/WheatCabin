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
    public partial class PersistentHumanBone<TID> : PersistentSurrogate<TID>
    {
        [ProtoMember(256)]
        public PersistentHumanLimit<TID> limit;

        [ProtoMember(259)]
        public string boneName;

        [ProtoMember(260)]
        public string humanName;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            HumanBone uo = (HumanBone)obj;
            limit = uo.limit;
            boneName = uo.boneName;
            humanName = uo.humanName;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            HumanBone uo = (HumanBone)obj;
            uo.limit = limit;
            uo.boneName = boneName;
            uo.humanName = humanName;
            return uo;
        }

        public static implicit operator HumanBone(PersistentHumanBone<TID> surrogate)
        {
            if(surrogate == null) return default(HumanBone);
            return (HumanBone)surrogate.WriteTo(new HumanBone());
        }
        
        public static implicit operator PersistentHumanBone<TID>(HumanBone obj)
        {
            PersistentHumanBone<TID> surrogate = new PersistentHumanBone<TID>();
            surrogate.ReadFrom(obj);
            return surrogate;
        }
    }
}

