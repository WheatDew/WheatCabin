using System.Collections.Generic;
using ProtoBuf;
using Battlehub.RTSL;
using Battlehub.Cubeman;
using Battlehub.Cubeman.Battlehub.SL2;
using UnityEngine.Battlehub.SL2;
using UnityEngine;
using System;

using UnityObject = UnityEngine.Object;
namespace Battlehub.Cubeman.Battlehub.SL2
{
    [ProtoContract]
    public partial class PersistentCubemanUserControl<TID> : PersistentMonoBehaviour<TID>
    {
        [ProtoMember(256)]
        public TID Cam;

        [ProtoMember(257)]
        public bool HandleInput;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            CubemanUserControl uo = (CubemanUserControl)obj;
            Cam = ToID(uo.Cam);
            HandleInput = uo.HandleInput;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            CubemanUserControl uo = (CubemanUserControl)obj;
            uo.Cam = FromID(Cam, uo.Cam);
            uo.HandleInput = HandleInput;
            return uo;
        }

        protected override void GetDepsImpl(GetDepsContext<TID> context)
        {
            base.GetDepsImpl(context);
            AddDep(Cam, context);
        }

        protected override void GetDepsFromImpl(object obj, GetDepsFromContext context)
        {
            base.GetDepsFromImpl(obj, context);
            CubemanUserControl uo = (CubemanUserControl)obj;
            AddDep(uo.Cam, context);
        }
    }
}

