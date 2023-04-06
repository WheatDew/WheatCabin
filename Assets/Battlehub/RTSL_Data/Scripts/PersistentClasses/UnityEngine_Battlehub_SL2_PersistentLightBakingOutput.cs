using System.Collections.Generic;
using ProtoBuf;
using Battlehub.RTSL;
using UnityEngine;
using UnityEngine.Battlehub.SL2;

using UnityObject = UnityEngine.Object;
namespace UnityEngine.Battlehub.SL2
{
    [ProtoContract]
    public partial class PersistentLightBakingOutput<TID> : PersistentSurrogate<TID>
    {
        
        public static implicit operator LightBakingOutput(PersistentLightBakingOutput<TID> surrogate)
        {
            if(surrogate == null) return default(LightBakingOutput);
            return (LightBakingOutput)surrogate.WriteTo(new LightBakingOutput());
        }
        
        public static implicit operator PersistentLightBakingOutput<TID>(LightBakingOutput obj)
        {
            PersistentLightBakingOutput<TID> surrogate = new PersistentLightBakingOutput<TID>();
            surrogate.ReadFrom(obj);
            return surrogate;
        }
    }
}

