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
    public partial class PersistentSpriteRenderer<TID> : PersistentRenderer<TID>
    {
        [ProtoMember(256)]
        public TID sprite;

        [ProtoMember(257)]
        public SpriteDrawMode drawMode;

        [ProtoMember(258)]
        public PersistentVector2<TID> size;

        [ProtoMember(259)]
        public float adaptiveModeThreshold;

        [ProtoMember(260)]
        public SpriteTileMode tileMode;

        [ProtoMember(261)]
        public PersistentColor<TID> color;

        [ProtoMember(262)]
        public SpriteMaskInteraction maskInteraction;

        [ProtoMember(263)]
        public bool flipX;

        [ProtoMember(264)]
        public bool flipY;

        [ProtoMember(265)]
        public SpriteSortPoint spriteSortPoint;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            SpriteRenderer uo = (SpriteRenderer)obj;
            sprite = ToID(uo.sprite);
            drawMode = uo.drawMode;
            size = uo.size;
            adaptiveModeThreshold = uo.adaptiveModeThreshold;
            tileMode = uo.tileMode;
            color = uo.color;
            maskInteraction = uo.maskInteraction;
            flipX = uo.flipX;
            flipY = uo.flipY;
            spriteSortPoint = uo.spriteSortPoint;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            SpriteRenderer uo = (SpriteRenderer)obj;
            uo.sprite = FromID(sprite, uo.sprite);
            uo.drawMode = drawMode;
            uo.size = size;
            uo.adaptiveModeThreshold = adaptiveModeThreshold;
            uo.tileMode = tileMode;
            uo.color = color;
            uo.maskInteraction = maskInteraction;
            uo.flipX = flipX;
            uo.flipY = flipY;
            uo.spriteSortPoint = spriteSortPoint;
            return uo;
        }

        protected override void GetDepsImpl(GetDepsContext<TID> context)
        {
            base.GetDepsImpl(context);
            AddDep(sprite, context);
        }

        protected override void GetDepsFromImpl(object obj, GetDepsFromContext context)
        {
            base.GetDepsFromImpl(obj, context);
            SpriteRenderer uo = (SpriteRenderer)obj;
            AddDep(uo.sprite, context);
        }
    }
}

