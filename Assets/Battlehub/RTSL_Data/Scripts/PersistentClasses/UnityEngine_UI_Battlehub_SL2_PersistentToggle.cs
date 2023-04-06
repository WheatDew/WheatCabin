using System.Collections.Generic;
using ProtoBuf;
using Battlehub.RTSL;
using UnityEngine.UI;
using UnityEngine.UI.Battlehub.SL2;
using System;

using UnityObject = UnityEngine.Object;
namespace UnityEngine.UI.Battlehub.SL2
{
    [ProtoContract]
    public partial class PersistentToggle<TID> : PersistentSelectable<TID>
    {
        [ProtoMember(256)]
        public Toggle.ToggleTransition toggleTransition;

        [ProtoMember(257)]
        public TID graphic;

        [ProtoMember(261)]
        public TID group;

        [ProtoMember(262)]
        public bool isOn;

        [ProtoMember(263)]
        public PersistentToggleNestedToggleEvent<TID> onValueChanged;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            Toggle uo = (Toggle)obj;
            toggleTransition = uo.toggleTransition;
            graphic = ToID(uo.graphic);
            group = ToID(uo.group);
            isOn = uo.isOn;
            onValueChanged = uo.onValueChanged;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            Toggle uo = (Toggle)obj;
            uo.toggleTransition = toggleTransition;
            uo.graphic = FromID(graphic, uo.graphic);
            uo.group = FromID(group, uo.group);
            uo.isOn = isOn;
            uo.onValueChanged = onValueChanged;
            return uo;
        }

        protected override void GetDepsImpl(GetDepsContext<TID> context)
        {
            base.GetDepsImpl(context);
            AddDep(graphic, context);
            AddDep(group, context);
            AddSurrogateDeps(onValueChanged, context);
        }

        protected override void GetDepsFromImpl(object obj, GetDepsFromContext context)
        {
            base.GetDepsFromImpl(obj, context);
            Toggle uo = (Toggle)obj;
            AddDep(uo.graphic, context);
            AddDep(uo.group, context);
            AddSurrogateDeps(uo.onValueChanged, v_ => (PersistentToggleNestedToggleEvent<TID>)v_, context);
        }
    }
}

