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
    public partial class PersistentDetailPrototype<TID> : PersistentSurrogate<TID>
    {
        [ProtoMember(256)]
        public TID prototype;

        [ProtoMember(257)]
        public TID prototypeTexture;

        [ProtoMember(258)]
        public float minWidth;

        [ProtoMember(259)]
        public float maxWidth;

        [ProtoMember(260)]
        public float minHeight;

        [ProtoMember(261)]
        public float maxHeight;

        [ProtoMember(262)]
        public float noiseSpread;

        [ProtoMember(264)]
        public PersistentColor<TID> healthyColor;

        [ProtoMember(265)]
        public PersistentColor<TID> dryColor;

        [ProtoMember(266)]
        public DetailRenderMode renderMode;

        [ProtoMember(267)]
        public bool usePrototypeMesh;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            DetailPrototype uo = (DetailPrototype)obj;
            prototype = ToID(uo.prototype);
            prototypeTexture = ToID(uo.prototypeTexture);
            minWidth = uo.minWidth;
            maxWidth = uo.maxWidth;
            minHeight = uo.minHeight;
            maxHeight = uo.maxHeight;
            noiseSpread = uo.noiseSpread;
            healthyColor = uo.healthyColor;
            dryColor = uo.dryColor;
            renderMode = uo.renderMode;
            usePrototypeMesh = uo.usePrototypeMesh;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            DetailPrototype uo = (DetailPrototype)obj;
            uo.prototype = FromID(prototype, uo.prototype);
            uo.prototypeTexture = FromID(prototypeTexture, uo.prototypeTexture);
            uo.minWidth = minWidth;
            uo.maxWidth = maxWidth;
            uo.minHeight = minHeight;
            uo.maxHeight = maxHeight;
            uo.noiseSpread = noiseSpread;
            uo.healthyColor = healthyColor;
            uo.dryColor = dryColor;
            uo.renderMode = renderMode;
            uo.usePrototypeMesh = usePrototypeMesh;
            return uo;
        }

        protected override void GetDepsImpl(GetDepsContext<TID> context)
        {
            base.GetDepsImpl(context);
            AddDep(prototype, context);
            AddDep(prototypeTexture, context);
        }

        protected override void GetDepsFromImpl(object obj, GetDepsFromContext context)
        {
            base.GetDepsFromImpl(obj, context);
            DetailPrototype uo = (DetailPrototype)obj;
            AddDep(uo.prototype, context);
            AddDep(uo.prototypeTexture, context);
        }

        public static implicit operator DetailPrototype(PersistentDetailPrototype<TID> surrogate)
        {
            if(surrogate == null) return default(DetailPrototype);
            return (DetailPrototype)surrogate.WriteTo(new DetailPrototype());
        }
        
        public static implicit operator PersistentDetailPrototype<TID>(DetailPrototype obj)
        {
            PersistentDetailPrototype<TID> surrogate = new PersistentDetailPrototype<TID>();
            surrogate.ReadFrom(obj);
            return surrogate;
        }
    }
}

