using System.Collections.Generic;
using ProtoBuf;
using Battlehub.RTSL;
using Battlehub.Cubeman;
using Battlehub.Cubeman.Battlehub.SL2;
using UnityEngine.Battlehub.SL2;
using UnityEngine.UI;
using UnityEngine;
using System;

using UnityObject = UnityEngine.Object;
namespace Battlehub.Cubeman.Battlehub.SL2
{
    [ProtoContract]
    public partial class PersistentCubemenGame<TID> : PersistentMonoBehaviour<TID>
    {
        [ProtoMember(256)]
        public TID TxtScore;

        [ProtoMember(257)]
        public TID TxtCompleted;

        [ProtoMember(258)]
        public TID TxtTip;

        [ProtoMember(259)]
        public TID BtnReplay;

        [ProtoMember(260)]
        public TID GameUI;

        [ProtoMember(265)]
        public int CurrentIndex;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            CubemenGame uo = (CubemenGame)obj;
            TxtScore = ToID(uo.TxtScore);
            TxtCompleted = ToID(uo.TxtCompleted);
            TxtTip = ToID(uo.TxtTip);
            BtnReplay = ToID(uo.BtnReplay);
            GameUI = ToID(uo.GameUI);
            CurrentIndex = uo.CurrentIndex;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            CubemenGame uo = (CubemenGame)obj;
            uo.TxtScore = FromID(TxtScore, uo.TxtScore);
            uo.TxtCompleted = FromID(TxtCompleted, uo.TxtCompleted);
            uo.TxtTip = FromID(TxtTip, uo.TxtTip);
            uo.BtnReplay = FromID(BtnReplay, uo.BtnReplay);
            uo.GameUI = FromID(GameUI, uo.GameUI);
            uo.CurrentIndex = CurrentIndex;
            return uo;
        }

        protected override void GetDepsImpl(GetDepsContext<TID> context)
        {
            base.GetDepsImpl(context);
            AddDep(TxtScore, context);
            AddDep(TxtCompleted, context);
            AddDep(TxtTip, context);
            AddDep(BtnReplay, context);
            AddDep(GameUI, context);
        }

        protected override void GetDepsFromImpl(object obj, GetDepsFromContext context)
        {
            base.GetDepsFromImpl(obj, context);
            CubemenGame uo = (CubemenGame)obj;
            AddDep(uo.TxtScore, context);
            AddDep(uo.TxtCompleted, context);
            AddDep(uo.TxtTip, context);
            AddDep(uo.BtnReplay, context);
            AddDep(uo.GameUI, context);
        }
    }
}

