using System.Collections.Generic;
using ProtoBuf;
using Battlehub.RTSL;
using TMPro;
using TMPro.Battlehub.SL2;
using UnityEngine.Battlehub.SL2;
using System;
using UnityEngine;

using UnityObject = UnityEngine.Object;
namespace TMPro.Battlehub.SL2
{
    [ProtoContract]
    public partial class PersistentTMP_Asset<TID> : PersistentObject<TID>
    {
        [ProtoMember(256)]
        public int hashCode;

        [ProtoMember(257)]
        public TID material;

        [ProtoMember(258)]
        public int materialHashCode;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            TMP_Asset uo = (TMP_Asset)obj;
            hashCode = uo.hashCode;
            material = ToID(uo.material);
            materialHashCode = uo.materialHashCode;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            TMP_Asset uo = (TMP_Asset)obj;
            uo.hashCode = hashCode;
            uo.material = FromID(material, uo.material);
            uo.materialHashCode = materialHashCode;
            return uo;
        }

        protected override void GetDepsImpl(GetDepsContext<TID> context)
        {
            base.GetDepsImpl(context);
            AddDep(material, context);
        }

        protected override void GetDepsFromImpl(object obj, GetDepsFromContext context)
        {
            base.GetDepsFromImpl(obj, context);
            TMP_Asset uo = (TMP_Asset)obj;
            AddDep(uo.material, context);
        }
    }
}

