using System.Collections.Generic;
using ProtoBuf;
using Battlehub.RTSL;
using UnityEngine.EventSystems;
using UnityEngine.EventSystems.Battlehub.SL2;
using System;

using UnityObject = UnityEngine.Object;
namespace UnityEngine.EventSystems.Battlehub.SL2
{
    [ProtoContract]
    public partial class PersistentStandaloneInputModule<TID> : PersistentPointerInputModule<TID>
    {
        [ProtoMember(273)]
        public float inputActionsPerSecond;

        [ProtoMember(274)]
        public float repeatDelay;

        [ProtoMember(275)]
        public string horizontalAxis;

        [ProtoMember(276)]
        public string verticalAxis;

        [ProtoMember(277)]
        public string submitButton;

        [ProtoMember(278)]
        public string cancelButton;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            StandaloneInputModule uo = (StandaloneInputModule)obj;
            inputActionsPerSecond = uo.inputActionsPerSecond;
            repeatDelay = uo.repeatDelay;
            horizontalAxis = uo.horizontalAxis;
            verticalAxis = uo.verticalAxis;
            submitButton = uo.submitButton;
            cancelButton = uo.cancelButton;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            StandaloneInputModule uo = (StandaloneInputModule)obj;
            uo.inputActionsPerSecond = inputActionsPerSecond;
            uo.repeatDelay = repeatDelay;
            uo.horizontalAxis = horizontalAxis;
            uo.verticalAxis = verticalAxis;
            uo.submitButton = submitButton;
            uo.cancelButton = cancelButton;
            return uo;
        }
    }
}

