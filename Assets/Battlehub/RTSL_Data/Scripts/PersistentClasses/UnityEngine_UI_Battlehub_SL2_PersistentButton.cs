using System.Collections.Generic;
using ProtoBuf;
using Battlehub.RTSL;
using UnityEngine.UI;
using UnityEngine.UI.Battlehub.SL2;

using UnityObject = UnityEngine.Object;
namespace UnityEngine.UI.Battlehub.SL2
{
    [ProtoContract]
    public partial class PersistentButton<TID> : PersistentSelectable<TID>
    {
        [ProtoMember(258)]
        public PersistentButtonNestedButtonClickedEvent<TID> onClick;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            Button uo = (Button)obj;
            onClick = uo.onClick;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            Button uo = (Button)obj;
            uo.onClick = onClick;
            return uo;
        }

        protected override void GetDepsImpl(GetDepsContext<TID> context)
        {
            base.GetDepsImpl(context);
            AddSurrogateDeps(onClick, context);
        }

        protected override void GetDepsFromImpl(object obj, GetDepsFromContext context)
        {
            base.GetDepsFromImpl(obj, context);
            Button uo = (Button)obj;
            AddSurrogateDeps(uo.onClick, v_ => (PersistentButtonNestedButtonClickedEvent<TID>)v_, context);
        }
    }
}

