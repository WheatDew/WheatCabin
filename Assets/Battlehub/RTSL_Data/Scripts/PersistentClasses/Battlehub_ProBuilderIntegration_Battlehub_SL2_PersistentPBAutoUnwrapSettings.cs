using System.Collections.Generic;
using ProtoBuf;
using Battlehub.RTSL;
using Battlehub.ProBuilderIntegration;
using Battlehub.ProBuilderIntegration.Battlehub.SL2;
using System;
using UnityEngine;
using UnityEngine.Battlehub.SL2;

using UnityObject = UnityEngine.Object;
namespace Battlehub.ProBuilderIntegration.Battlehub.SL2
{
    [ProtoContract]
    public partial class PersistentPBAutoUnwrapSettings<TID> : PersistentSurrogate<TID>
    {
        [ProtoMember(259)]
        public float rotation;

        [ProtoMember(260)]
        public PersistentVector2<TID> offset;

        [ProtoMember(261)]
        public PersistentVector2<TID> scale;

        [ProtoMember(263)]
        public bool swapUV;

        [ProtoMember(264)]
        public bool flipV;

        [ProtoMember(265)]
        public bool flipU;

        [ProtoMember(266)]
        public bool useWorldSpace;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            PBAutoUnwrapSettings uo = (PBAutoUnwrapSettings)obj;
            rotation = uo.rotation;
            offset = uo.offset;
            scale = uo.scale;
            swapUV = uo.swapUV;
            flipV = uo.flipV;
            flipU = uo.flipU;
            useWorldSpace = uo.useWorldSpace;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            PBAutoUnwrapSettings uo = (PBAutoUnwrapSettings)obj;
            uo.rotation = rotation;
            uo.offset = offset;
            uo.scale = scale;
            uo.swapUV = swapUV;
            uo.flipV = flipV;
            uo.flipU = flipU;
            uo.useWorldSpace = useWorldSpace;
            return uo;
        }

        public static implicit operator PBAutoUnwrapSettings(PersistentPBAutoUnwrapSettings<TID> surrogate)
        {
            if(surrogate == null) return default(PBAutoUnwrapSettings);
            return (PBAutoUnwrapSettings)surrogate.WriteTo(new PBAutoUnwrapSettings());
        }
        
        public static implicit operator PersistentPBAutoUnwrapSettings<TID>(PBAutoUnwrapSettings obj)
        {
            PersistentPBAutoUnwrapSettings<TID> surrogate = new PersistentPBAutoUnwrapSettings<TID>();
            surrogate.ReadFrom(obj);
            return surrogate;
        }
    }
}

