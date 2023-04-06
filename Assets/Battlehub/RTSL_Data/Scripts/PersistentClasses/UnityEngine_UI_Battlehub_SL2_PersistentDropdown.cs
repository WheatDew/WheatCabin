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
    public partial class PersistentDropdown<TID> : PersistentSelectable<TID>
    {
        [ProtoMember(265)]
        public TID template;

        [ProtoMember(266)]
        public TID captionText;

        [ProtoMember(267)]
        public TID captionImage;

        [ProtoMember(268)]
        public TID itemText;

        [ProtoMember(269)]
        public TID itemImage;

        [ProtoMember(270)]
        public List<PersistentDropdownNestedOptionData<TID>> options;

        [ProtoMember(271)]
        public PersistentDropdownNestedDropdownEvent<TID> onValueChanged;

        [ProtoMember(272)]
        public float alphaFadeSpeed;

        [ProtoMember(273)]
        public int value;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            Dropdown uo = (Dropdown)obj;
            template = ToID(uo.template);
            captionText = ToID(uo.captionText);
            captionImage = ToID(uo.captionImage);
            itemText = ToID(uo.itemText);
            itemImage = ToID(uo.itemImage);
            options = Assign(uo.options, v_ => (PersistentDropdownNestedOptionData<TID>)v_);
            onValueChanged = uo.onValueChanged;
            alphaFadeSpeed = uo.alphaFadeSpeed;
            value = uo.value;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            Dropdown uo = (Dropdown)obj;
            uo.template = FromID(template, uo.template);
            uo.captionText = FromID(captionText, uo.captionText);
            uo.captionImage = FromID(captionImage, uo.captionImage);
            uo.itemText = FromID(itemText, uo.itemText);
            uo.itemImage = FromID(itemImage, uo.itemImage);
            uo.options = Assign(options, v_ => (Dropdown.OptionData)v_);
            uo.onValueChanged = onValueChanged;
            uo.alphaFadeSpeed = alphaFadeSpeed;
            uo.value = value;
            return uo;
        }

        protected override void GetDepsImpl(GetDepsContext<TID> context)
        {
            base.GetDepsImpl(context);
            AddDep(template, context);
            AddDep(captionText, context);
            AddDep(captionImage, context);
            AddDep(itemText, context);
            AddDep(itemImage, context);
            AddSurrogateDeps(options, context);
            AddSurrogateDeps(onValueChanged, context);
        }

        protected override void GetDepsFromImpl(object obj, GetDepsFromContext context)
        {
            base.GetDepsFromImpl(obj, context);
            Dropdown uo = (Dropdown)obj;
            AddDep(uo.template, context);
            AddDep(uo.captionText, context);
            AddDep(uo.captionImage, context);
            AddDep(uo.itemText, context);
            AddDep(uo.itemImage, context);
            AddSurrogateDeps(uo.options, v_ => (PersistentDropdownNestedOptionData<TID>)v_, context);
            AddSurrogateDeps(uo.onValueChanged, v_ => (PersistentDropdownNestedDropdownEvent<TID>)v_, context);
        }
    }
}

