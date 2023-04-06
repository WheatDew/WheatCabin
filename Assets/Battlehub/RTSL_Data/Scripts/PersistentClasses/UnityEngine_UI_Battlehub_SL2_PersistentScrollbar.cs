using System.Collections.Generic;
using ProtoBuf;
using Battlehub.RTSL;
using UnityEngine.UI;
using UnityEngine.UI.Battlehub.SL2;
using UnityEngine;
using System;

using UnityObject = UnityEngine.Object;
namespace UnityEngine.UI.Battlehub.SL2
{
    [ProtoContract]
    public partial class PersistentScrollbar<TID> : PersistentSelectable<TID>
    {
        [ProtoMember(262)]
        public TID handleRect;

        [ProtoMember(263)]
        public Scrollbar.Direction direction;

        [ProtoMember(264)]
        public float value;

        [ProtoMember(265)]
        public float size;

        [ProtoMember(266)]
        public int numberOfSteps;

        [ProtoMember(267)]
        public PersistentScrollbarNestedScrollEvent<TID> onValueChanged;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            Scrollbar uo = (Scrollbar)obj;
            handleRect = ToID(uo.handleRect);
            direction = uo.direction;
            value = uo.value;
            size = uo.size;
            numberOfSteps = uo.numberOfSteps;
            onValueChanged = uo.onValueChanged;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            Scrollbar uo = (Scrollbar)obj;
            uo.handleRect = FromID(handleRect, uo.handleRect);
            uo.direction = direction;
            uo.value = value;
            uo.size = size;
            uo.numberOfSteps = numberOfSteps;
            uo.onValueChanged = onValueChanged;
            return uo;
        }

        protected override void GetDepsImpl(GetDepsContext<TID> context)
        {
            base.GetDepsImpl(context);
            AddDep(handleRect, context);
            AddSurrogateDeps(onValueChanged, context);
        }

        protected override void GetDepsFromImpl(object obj, GetDepsFromContext context)
        {
            base.GetDepsFromImpl(obj, context);
            Scrollbar uo = (Scrollbar)obj;
            AddDep(uo.handleRect, context);
            AddSurrogateDeps(uo.onValueChanged, v_ => (PersistentScrollbarNestedScrollEvent<TID>)v_, context);
        }
    }
}

