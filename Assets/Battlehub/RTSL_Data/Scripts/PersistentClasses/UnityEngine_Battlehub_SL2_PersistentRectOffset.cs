using System.Collections.Generic;
using ProtoBuf;
using Battlehub.RTSL;
using UnityEngine;
using UnityEngine.Battlehub.SL2;

using UnityObject = UnityEngine.Object;
namespace UnityEngine.Battlehub.SL2
{
    [ProtoContract]
    public partial class PersistentRectOffset<TID> : PersistentSurrogate<TID>
    {
        
        public static implicit operator RectOffset(PersistentRectOffset<TID> surrogate)
        {
            if(surrogate == null) return default(RectOffset);
            return (RectOffset)surrogate.WriteTo(new RectOffset());
        }
        
        public static implicit operator PersistentRectOffset<TID>(RectOffset obj)
        {
            PersistentRectOffset<TID> surrogate = new PersistentRectOffset<TID>();
            surrogate.ReadFrom(obj);
            return surrogate;
        }
    }
}

