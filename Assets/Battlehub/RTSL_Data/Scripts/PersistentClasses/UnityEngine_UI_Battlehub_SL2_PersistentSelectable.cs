using System.Collections.Generic;
using ProtoBuf;
using Battlehub.RTSL;
using UnityEngine.UI;
using UnityEngine.UI.Battlehub.SL2;
using UnityEngine.EventSystems.Battlehub.SL2;
using System;

using UnityObject = UnityEngine.Object;
namespace UnityEngine.UI.Battlehub.SL2
{
    [ProtoContract]
    public partial class PersistentSelectable<TID> : PersistentUIBehaviour<TID>
    {
        [ProtoMember(271)]
        public PersistentNavigation<TID> navigation;

        [ProtoMember(272)]
        public Selectable.Transition transition;

        [ProtoMember(273)]
        public PersistentColorBlock<TID> colors;

        [ProtoMember(274)]
        public PersistentSpriteState<TID> spriteState;

        [ProtoMember(275)]
        public PersistentAnimationTriggers<TID> animationTriggers;

        [ProtoMember(276)]
        public TID targetGraphic;

        [ProtoMember(277)]
        public bool interactable;

        [ProtoMember(278)]
        public TID image;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            Selectable uo = (Selectable)obj;
            navigation = uo.navigation;
            transition = uo.transition;
            colors = uo.colors;
            spriteState = uo.spriteState;
            animationTriggers = uo.animationTriggers;
            targetGraphic = ToID(uo.targetGraphic);
            interactable = uo.interactable;
            image = ToID(uo.image);
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            Selectable uo = (Selectable)obj;
            uo.navigation = navigation;
            uo.transition = transition;
            uo.colors = colors;
            uo.spriteState = spriteState;
            uo.animationTriggers = animationTriggers;
            uo.targetGraphic = FromID(targetGraphic, uo.targetGraphic);
            uo.interactable = interactable;
            uo.image = FromID(image, uo.image);
            return uo;
        }

        protected override void GetDepsImpl(GetDepsContext<TID> context)
        {
            base.GetDepsImpl(context);
            AddSurrogateDeps(navigation, context);
            AddSurrogateDeps(spriteState, context);
            AddDep(targetGraphic, context);
            AddDep(image, context);
        }

        protected override void GetDepsFromImpl(object obj, GetDepsFromContext context)
        {
            base.GetDepsFromImpl(obj, context);
            Selectable uo = (Selectable)obj;
            AddSurrogateDeps(uo.navigation, v_ => (PersistentNavigation<TID>)v_, context);
            AddSurrogateDeps(uo.spriteState, v_ => (PersistentSpriteState<TID>)v_, context);
            AddDep(uo.targetGraphic, context);
            AddDep(uo.image, context);
        }
    }
}

