using System.Collections.Generic;
using ProtoBuf;
using Battlehub.RTSL;
using UnityEngine.UI;
using UnityEngine.UI.Battlehub.SL2;
using UnityEngine.EventSystems.Battlehub.SL2;
using UnityEngine;
using UnityEngine.Battlehub.SL2;
using System;

using UnityObject = UnityEngine.Object;
namespace UnityEngine.UI.Battlehub.SL2
{
    [ProtoContract]
    public partial class PersistentGraphic<TID> : PersistentUIBehaviour<TID>
    {
        [ProtoMember(262)]
        public PersistentColor<TID> color;

        [ProtoMember(263)]
        public bool raycastTarget;

        [ProtoMember(264)]
        public TID material;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            Graphic uo = (Graphic)obj;
            color = uo.color;
            raycastTarget = uo.raycastTarget;
            material = ToID(uo.material);
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            Graphic uo = (Graphic)obj;
            uo.color = color;
            uo.raycastTarget = raycastTarget;
            uo.material = FromID(material, uo.material);
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
            Graphic uo = (Graphic)obj;
            AddDep(uo.material, context);
        }
    }
}

