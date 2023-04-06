using System.Collections.Generic;
using ProtoBuf;
using Battlehub.RTSL;
using UnityEngine.SceneManagement;
using UnityEngine.SceneManagement.Battlehub.SL2;

using UnityObject = UnityEngine.Object;
namespace UnityEngine.SceneManagement.Battlehub.SL2
{
    [ProtoContract]
    public partial class PersistentScene<TID> : PersistentSurrogate<TID>
    {
        
        public static implicit operator Scene(PersistentScene<TID> surrogate)
        {
            if(surrogate == null) return default(Scene);
            return (Scene)surrogate.WriteTo(new Scene());
        }
        
        public static implicit operator PersistentScene<TID>(Scene obj)
        {
            PersistentScene<TID> surrogate = new PersistentScene<TID>();
            surrogate.ReadFrom(obj);
            return surrogate;
        }
    }
}

