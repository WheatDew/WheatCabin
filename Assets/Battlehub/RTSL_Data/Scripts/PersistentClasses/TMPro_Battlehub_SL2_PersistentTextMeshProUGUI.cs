using System.Collections.Generic;
using ProtoBuf;
using Battlehub.RTSL;
using TMPro;
using TMPro.Battlehub.SL2;
using System;
using UnityEngine;
using UnityEngine.Battlehub.SL2;

using UnityObject = UnityEngine.Object;
namespace TMPro.Battlehub.SL2
{
    [ProtoContract]
    public partial class PersistentTextMeshProUGUI<TID> : PersistentTMP_Text<TID>
    {
        [ProtoMember(259)]
        public bool autoSizeTextContainer;

        [ProtoMember(260)]
        public PersistentVector4<TID> maskOffset;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            TextMeshProUGUI uo = (TextMeshProUGUI)obj;
            autoSizeTextContainer = uo.autoSizeTextContainer;
            maskOffset = uo.maskOffset;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            TextMeshProUGUI uo = (TextMeshProUGUI)obj;
            uo.autoSizeTextContainer = autoSizeTextContainer;
            uo.maskOffset = maskOffset;
            return uo;
        }
    }
}

