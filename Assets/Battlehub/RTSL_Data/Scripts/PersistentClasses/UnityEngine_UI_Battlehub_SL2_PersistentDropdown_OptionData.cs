using System.Collections.Generic;
using ProtoBuf;
using Battlehub.RTSL;
using UnityEngine.UI;
using UnityEngine.UI.Battlehub.SL2;
using System;
using UnityEngine;

using UnityObject = UnityEngine.Object;
namespace UnityEngine.UI.Battlehub.SL2
{
    [ProtoContract]
    public partial class PersistentDropdownNestedOptionData<TID> : PersistentSurrogate<TID>
    {
        [ProtoMember(258)]
        public string text;

        [ProtoMember(259)]
        public TID image;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            Dropdown.OptionData uo = (Dropdown.OptionData)obj;
            text = uo.text;
            image = ToID(uo.image);
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            Dropdown.OptionData uo = (Dropdown.OptionData)obj;
            uo.text = text;
            uo.image = FromID(image, uo.image);
            return uo;
        }

        protected override void GetDepsImpl(GetDepsContext<TID> context)
        {
            base.GetDepsImpl(context);
            AddDep(image, context);
        }

        protected override void GetDepsFromImpl(object obj, GetDepsFromContext context)
        {
            base.GetDepsFromImpl(obj, context);
            Dropdown.OptionData uo = (Dropdown.OptionData)obj;
            AddDep(uo.image, context);
        }

        public static implicit operator Dropdown.OptionData(PersistentDropdownNestedOptionData<TID> surrogate)
        {
            if(surrogate == null) return default(Dropdown.OptionData);
            return (Dropdown.OptionData)surrogate.WriteTo(new Dropdown.OptionData());
        }
        
        public static implicit operator PersistentDropdownNestedOptionData<TID>(Dropdown.OptionData obj)
        {
            PersistentDropdownNestedOptionData<TID> surrogate = new PersistentDropdownNestedOptionData<TID>();
            surrogate.ReadFrom(obj);
            return surrogate;
        }
    }
}

