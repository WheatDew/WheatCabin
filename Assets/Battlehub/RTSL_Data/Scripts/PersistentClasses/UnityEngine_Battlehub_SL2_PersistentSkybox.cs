using System.Collections.Generic;
using ProtoBuf;
using Battlehub.RTSL;
using UnityEngine;
using UnityEngine.Battlehub.SL2;

using UnityObject = UnityEngine.Object;
namespace UnityEngine.Battlehub.SL2
{
    [ProtoContract]
    public partial class PersistentSkybox<TID> : PersistentBehaviour<TID>
    {
        [ProtoMember(256)]
        public TID material;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            Skybox uo = (Skybox)obj;
            material = ToID(uo.material);
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            Skybox uo = (Skybox)obj;
            uo.material = FromID(material, uo.material);
            return uo;
        }

        protected override void GetDepsImpl(GetDepsContext<TID> context)
        {
            base.GetDepsImpl(context);
            AddDep(material, context);
        }

        protected override void GetDepsFromImpl(object obj, GetDepsFromContext context)
        {
            base.GetDepsFromImpl(obj, context);
            Skybox uo = (Skybox)obj;
            AddDep(uo.material, context);
        }
    }
}

