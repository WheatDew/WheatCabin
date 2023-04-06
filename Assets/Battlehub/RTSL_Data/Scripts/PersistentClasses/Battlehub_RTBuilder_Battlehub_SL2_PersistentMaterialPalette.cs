using System.Collections.Generic;
using ProtoBuf;
using Battlehub.RTSL;
using Battlehub.RTBuilder;
using Battlehub.RTBuilder.Battlehub.SL2;
using UnityEngine.Battlehub.SL2;
using UnityEngine;

using UnityObject = UnityEngine.Object;
namespace Battlehub.RTBuilder.Battlehub.SL2
{
    [ProtoContract]
    public partial class PersistentMaterialPalette<TID> : PersistentMonoBehaviour<TID>
    {
        [ProtoMember(256)]
        public TID[] Materials;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            MaterialPalette uo = (MaterialPalette)obj;
            Materials = ToID(uo.Materials);
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            MaterialPalette uo = (MaterialPalette)obj;
            uo.Materials = FromID(Materials, uo.Materials);
            return uo;
        }

        protected override void GetDepsImpl(GetDepsContext<TID> context)
        {
            base.GetDepsImpl(context);
            AddDep(Materials, context);
        }

        protected override void GetDepsFromImpl(object obj, GetDepsFromContext context)
        {
            base.GetDepsFromImpl(obj, context);
            MaterialPalette uo = (MaterialPalette)obj;
            AddDep(uo.Materials, context);
        }
    }
}

