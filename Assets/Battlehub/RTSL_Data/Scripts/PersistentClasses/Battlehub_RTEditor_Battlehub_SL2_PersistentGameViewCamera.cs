using System.Collections.Generic;
using ProtoBuf;
using Battlehub.RTSL;
using Battlehub.RTEditor;
using Battlehub.RTEditor.Battlehub.SL2;
using UnityEngine.Battlehub.SL2;
using UnityEngine;
using System;

using UnityObject = UnityEngine.Object;
namespace Battlehub.RTEditor.Battlehub.SL2
{
    [ProtoContract]
    public partial class PersistentGameViewCamera<TID> : PersistentMonoBehaviour<TID>
    {
        [ProtoMember(258)]
        public PersistentRect<TID> Rect;

        [ProtoMember(259)]
        public int Depth;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            GameViewCamera uo = (GameViewCamera)obj;
            Rect = uo.Rect;
            Depth = uo.Depth;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            GameViewCamera uo = (GameViewCamera)obj;
            uo.Rect = Rect;
            uo.Depth = Depth;
            return uo;
        }
    }
}

