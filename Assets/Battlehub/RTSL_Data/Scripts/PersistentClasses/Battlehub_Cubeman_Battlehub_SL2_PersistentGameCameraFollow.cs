using System.Collections.Generic;
using ProtoBuf;
using Battlehub.RTSL;
using Battlehub.Cubeman;
using Battlehub.Cubeman.Battlehub.SL2;
using UnityEngine.Battlehub.SL2;
using System;
using UnityEngine;

using UnityObject = UnityEngine.Object;
namespace Battlehub.Cubeman.Battlehub.SL2
{
    [ProtoContract]
    public partial class PersistentGameCameraFollow<TID> : PersistentMonoBehaviour<TID>
    {
        [ProtoMember(256)]
        public float distance;

        [ProtoMember(257)]
        public float height;

        [ProtoMember(258)]
        public float rotationDamping;

        [ProtoMember(259)]
        public float heightDamping;

        [ProtoMember(260)]
        public TID target;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            GameCameraFollow uo = (GameCameraFollow)obj;
            distance = uo.distance;
            height = uo.height;
            rotationDamping = uo.rotationDamping;
            heightDamping = uo.heightDamping;
            target = ToID(uo.target);
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            GameCameraFollow uo = (GameCameraFollow)obj;
            uo.distance = distance;
            uo.height = height;
            uo.rotationDamping = rotationDamping;
            uo.heightDamping = heightDamping;
            uo.target = FromID(target, uo.target);
            return uo;
        }

        protected override void GetDepsImpl(GetDepsContext<TID> context)
        {
            base.GetDepsImpl(context);
            AddDep(target, context);
        }

        protected override void GetDepsFromImpl(object obj, GetDepsFromContext context)
        {
            base.GetDepsFromImpl(obj, context);
            GameCameraFollow uo = (GameCameraFollow)obj;
            AddDep(uo.target, context);
        }
    }
}

