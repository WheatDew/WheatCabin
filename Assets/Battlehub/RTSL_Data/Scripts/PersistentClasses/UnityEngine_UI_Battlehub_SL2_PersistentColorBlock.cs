using System.Collections.Generic;
using ProtoBuf;
using Battlehub.RTSL;
using UnityEngine.UI;
using UnityEngine.UI.Battlehub.SL2;
using UnityEngine;
using UnityEngine.Battlehub.SL2;
using System;

using UnityObject = UnityEngine.Object;
namespace UnityEngine.UI.Battlehub.SL2
{
    [ProtoContract]
    public partial class PersistentColorBlock<TID> : PersistentSurrogate<TID>
    {
        [ProtoMember(256)]
        public PersistentColor<TID> normalColor;

        [ProtoMember(257)]
        public PersistentColor<TID> highlightedColor;

        [ProtoMember(258)]
        public PersistentColor<TID> pressedColor;

        [ProtoMember(259)]
        public PersistentColor<TID> disabledColor;

        [ProtoMember(260)]
        public float colorMultiplier;

        [ProtoMember(261)]
        public float fadeDuration;

        [ProtoMember(269)]
        public PersistentColor<TID> selectedColor;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            ColorBlock uo = (ColorBlock)obj;
            normalColor = uo.normalColor;
            highlightedColor = uo.highlightedColor;
            pressedColor = uo.pressedColor;
            disabledColor = uo.disabledColor;
            colorMultiplier = uo.colorMultiplier;
            fadeDuration = uo.fadeDuration;
            selectedColor = uo.selectedColor;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            ColorBlock uo = (ColorBlock)obj;
            uo.normalColor = normalColor;
            uo.highlightedColor = highlightedColor;
            uo.pressedColor = pressedColor;
            uo.disabledColor = disabledColor;
            uo.colorMultiplier = colorMultiplier;
            uo.fadeDuration = fadeDuration;
            uo.selectedColor = selectedColor;
            return uo;
        }

        public static implicit operator ColorBlock(PersistentColorBlock<TID> surrogate)
        {
            if(surrogate == null) return default(ColorBlock);
            return (ColorBlock)surrogate.WriteTo(new ColorBlock());
        }
        
        public static implicit operator PersistentColorBlock<TID>(ColorBlock obj)
        {
            PersistentColorBlock<TID> surrogate = new PersistentColorBlock<TID>();
            surrogate.ReadFrom(obj);
            return surrogate;
        }
    }
}

