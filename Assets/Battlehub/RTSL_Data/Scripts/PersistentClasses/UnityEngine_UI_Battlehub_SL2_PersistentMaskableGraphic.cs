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
    public partial class PersistentMaskableGraphic<TID> : PersistentGraphic<TID>
    {
        [ProtoMember(259)]
        public PersistentMaskableGraphicNestedCullStateChangedEvent<TID> onCullStateChanged;

        [ProtoMember(260)]
        public bool maskable;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            MaskableGraphic uo = (MaskableGraphic)obj;
            onCullStateChanged = uo.onCullStateChanged;
            maskable = uo.maskable;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            MaskableGraphic uo = (MaskableGraphic)obj;
            uo.onCullStateChanged = onCullStateChanged;
            uo.maskable = maskable;
            return uo;
        }

        protected override void GetDepsImpl(GetDepsContext<TID> context)
        {
            base.GetDepsImpl(context);
            AddSurrogateDeps(onCullStateChanged, context);
        }

        protected override void GetDepsFromImpl(object obj, GetDepsFromContext context)
        {
            base.GetDepsFromImpl(obj, context);
            MaskableGraphic uo = (MaskableGraphic)obj;
            AddSurrogateDeps(uo.onCullStateChanged, v_ => (PersistentMaskableGraphicNestedCullStateChangedEvent<TID>)v_, context);
        }
    }
}

