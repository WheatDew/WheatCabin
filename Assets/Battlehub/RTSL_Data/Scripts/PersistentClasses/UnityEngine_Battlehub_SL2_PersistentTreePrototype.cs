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
    public partial class PersistentTreePrototype<TID> : PersistentSurrogate<TID>
    {
        [ProtoMember(256)]
        public TID prefab;

        [ProtoMember(257)]
        public float bendFactor;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            TreePrototype uo = (TreePrototype)obj;
            prefab = ToID(uo.prefab);
            bendFactor = uo.bendFactor;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            TreePrototype uo = (TreePrototype)obj;
            uo.prefab = FromID(prefab, uo.prefab);
            uo.bendFactor = bendFactor;
            return uo;
        }

        protected override void GetDepsImpl(GetDepsContext<TID> context)
        {
            base.GetDepsImpl(context);
            AddDep(prefab, context);
        }

        protected override void GetDepsFromImpl(object obj, GetDepsFromContext context)
        {
            base.GetDepsFromImpl(obj, context);
            TreePrototype uo = (TreePrototype)obj;
            AddDep(uo.prefab, context);
        }

        public static implicit operator TreePrototype(PersistentTreePrototype<TID> surrogate)
        {
            if(surrogate == null) return default(TreePrototype);
            return (TreePrototype)surrogate.WriteTo(new TreePrototype());
        }
        
        public static implicit operator PersistentTreePrototype<TID>(TreePrototype obj)
        {
            PersistentTreePrototype<TID> surrogate = new PersistentTreePrototype<TID>();
            surrogate.ReadFrom(obj);
            return surrogate;
        }
    }
}

