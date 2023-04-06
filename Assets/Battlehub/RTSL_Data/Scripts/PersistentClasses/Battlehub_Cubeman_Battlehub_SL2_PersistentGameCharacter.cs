using System.Collections.Generic;
using ProtoBuf;
using Battlehub.RTSL;
using Battlehub.Cubeman;
using Battlehub.Cubeman.Battlehub.SL2;
using UnityEngine.Battlehub.SL2;

using UnityObject = UnityEngine.Object;
namespace Battlehub.Cubeman.Battlehub.SL2
{
    [ProtoContract]
    public partial class PersistentGameCharacter<TID> : PersistentMonoBehaviour<TID>
    {
        [ProtoMember(256)]
        public TID Game;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            GameCharacter uo = (GameCharacter)obj;
            Game = ToID(uo.Game);
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            GameCharacter uo = (GameCharacter)obj;
            uo.Game = FromID(Game, uo.Game);
            return uo;
        }

        protected override void GetDepsImpl(GetDepsContext<TID> context)
        {
            base.GetDepsImpl(context);
            AddDep(Game, context);
        }

        protected override void GetDepsFromImpl(object obj, GetDepsFromContext context)
        {
            base.GetDepsFromImpl(obj, context);
            GameCharacter uo = (GameCharacter)obj;
            AddDep(uo.Game, context);
        }
    }
}

