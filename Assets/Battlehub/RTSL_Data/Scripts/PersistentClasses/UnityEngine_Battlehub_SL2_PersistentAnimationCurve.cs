using System.Collections.Generic;
using ProtoBuf;
using Battlehub.RTSL;
using UnityEngine;
using UnityEngine.Battlehub.SL2;

using UnityObject = UnityEngine.Object;
namespace UnityEngine.Battlehub.SL2
{
    [ProtoContract]
    public partial class PersistentAnimationCurve<TID> : PersistentSurrogate<TID>
    {
        [ProtoMember(257)]
        public WrapMode preWrapMode;

        [ProtoMember(258)]
        public WrapMode postWrapMode;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            AnimationCurve uo = (AnimationCurve)obj;
            preWrapMode = uo.preWrapMode;
            postWrapMode = uo.postWrapMode;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            AnimationCurve uo = (AnimationCurve)obj;
            uo.preWrapMode = preWrapMode;
            uo.postWrapMode = postWrapMode;
            return uo;
        }

        public static implicit operator AnimationCurve(PersistentAnimationCurve<TID> surrogate)
        {
            if(surrogate == null) return default(AnimationCurve);
            return (AnimationCurve)surrogate.WriteTo(new AnimationCurve());
        }
        
        public static implicit operator PersistentAnimationCurve<TID>(AnimationCurve obj)
        {
            PersistentAnimationCurve<TID> surrogate = new PersistentAnimationCurve<TID>();
            surrogate.ReadFrom(obj);
            return surrogate;
        }
    }
}

