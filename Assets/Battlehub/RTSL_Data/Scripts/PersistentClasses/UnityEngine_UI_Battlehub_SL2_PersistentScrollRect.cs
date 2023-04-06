using System.Collections.Generic;
using ProtoBuf;
using Battlehub.RTSL;
using UnityEngine.UI;
using UnityEngine.UI.Battlehub.SL2;
using UnityEngine.EventSystems.Battlehub.SL2;
using UnityEngine;
using System;
using UnityEngine.Battlehub.SL2;

using UnityObject = UnityEngine.Object;
namespace UnityEngine.UI.Battlehub.SL2
{
    [ProtoContract]
    public partial class PersistentScrollRect<TID> : PersistentUIBehaviour<TID>
    {
        [ProtoMember(272)]
        public TID content;

        [ProtoMember(273)]
        public bool horizontal;

        [ProtoMember(274)]
        public bool vertical;

        [ProtoMember(275)]
        public ScrollRect.MovementType movementType;

        [ProtoMember(276)]
        public float elasticity;

        [ProtoMember(277)]
        public bool inertia;

        [ProtoMember(278)]
        public float decelerationRate;

        [ProtoMember(279)]
        public float scrollSensitivity;

        [ProtoMember(280)]
        public TID viewport;

        [ProtoMember(281)]
        public TID horizontalScrollbar;

        [ProtoMember(282)]
        public TID verticalScrollbar;

        [ProtoMember(283)]
        public ScrollRect.ScrollbarVisibility horizontalScrollbarVisibility;

        [ProtoMember(284)]
        public ScrollRect.ScrollbarVisibility verticalScrollbarVisibility;

        [ProtoMember(285)]
        public float horizontalScrollbarSpacing;

        [ProtoMember(286)]
        public float verticalScrollbarSpacing;

        [ProtoMember(287)]
        public PersistentScrollRectNestedScrollRectEvent<TID> onValueChanged;

        [ProtoMember(288)]
        public PersistentVector2<TID> velocity;

        [ProtoMember(289)]
        public PersistentVector2<TID> normalizedPosition;

        [ProtoMember(290)]
        public float horizontalNormalizedPosition;

        [ProtoMember(291)]
        public float verticalNormalizedPosition;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            ScrollRect uo = (ScrollRect)obj;
            content = ToID(uo.content);
            horizontal = uo.horizontal;
            vertical = uo.vertical;
            movementType = uo.movementType;
            elasticity = uo.elasticity;
            inertia = uo.inertia;
            decelerationRate = uo.decelerationRate;
            scrollSensitivity = uo.scrollSensitivity;
            viewport = ToID(uo.viewport);
            horizontalScrollbar = ToID(uo.horizontalScrollbar);
            verticalScrollbar = ToID(uo.verticalScrollbar);
            horizontalScrollbarVisibility = uo.horizontalScrollbarVisibility;
            verticalScrollbarVisibility = uo.verticalScrollbarVisibility;
            horizontalScrollbarSpacing = uo.horizontalScrollbarSpacing;
            verticalScrollbarSpacing = uo.verticalScrollbarSpacing;
            onValueChanged = uo.onValueChanged;
            velocity = uo.velocity;
            normalizedPosition = uo.normalizedPosition;
            horizontalNormalizedPosition = uo.horizontalNormalizedPosition;
            verticalNormalizedPosition = uo.verticalNormalizedPosition;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            ScrollRect uo = (ScrollRect)obj;
            uo.content = FromID(content, uo.content);
            uo.horizontal = horizontal;
            uo.vertical = vertical;
            uo.movementType = movementType;
            uo.elasticity = elasticity;
            uo.inertia = inertia;
            uo.decelerationRate = decelerationRate;
            uo.scrollSensitivity = scrollSensitivity;
            uo.viewport = FromID(viewport, uo.viewport);
            uo.horizontalScrollbar = FromID(horizontalScrollbar, uo.horizontalScrollbar);
            uo.verticalScrollbar = FromID(verticalScrollbar, uo.verticalScrollbar);
            uo.horizontalScrollbarVisibility = horizontalScrollbarVisibility;
            uo.verticalScrollbarVisibility = verticalScrollbarVisibility;
            uo.horizontalScrollbarSpacing = horizontalScrollbarSpacing;
            uo.verticalScrollbarSpacing = verticalScrollbarSpacing;
            uo.onValueChanged = onValueChanged;
            uo.velocity = velocity;
            uo.normalizedPosition = normalizedPosition;
            uo.horizontalNormalizedPosition = horizontalNormalizedPosition;
            uo.verticalNormalizedPosition = verticalNormalizedPosition;
            return uo;
        }

        protected override void GetDepsImpl(GetDepsContext<TID> context)
        {
            base.GetDepsImpl(context);
            AddDep(content, context);
            AddDep(viewport, context);
            AddDep(horizontalScrollbar, context);
            AddDep(verticalScrollbar, context);
            AddSurrogateDeps(onValueChanged, context);
        }

        protected override void GetDepsFromImpl(object obj, GetDepsFromContext context)
        {
            base.GetDepsFromImpl(obj, context);
            ScrollRect uo = (ScrollRect)obj;
            AddDep(uo.content, context);
            AddDep(uo.viewport, context);
            AddDep(uo.horizontalScrollbar, context);
            AddDep(uo.verticalScrollbar, context);
            AddSurrogateDeps(uo.onValueChanged, v_ => (PersistentScrollRectNestedScrollRectEvent<TID>)v_, context);
        }
    }
}

