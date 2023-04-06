using System.Collections.Generic;
using ProtoBuf;
using Battlehub.RTSL;
using UnityEngine.UI;
using UnityEngine.UI.Battlehub.SL2;
using UnityEngine.Events.Battlehub.SL2;

using UnityObject = UnityEngine.Object;
namespace UnityEngine.UI.Battlehub.SL2
{
    [ProtoContract]
    public partial class PersistentToggleNestedToggleEvent<TID> : PersistentUnityEventBase<TID>
    {
        
        public static implicit operator Toggle.ToggleEvent(PersistentToggleNestedToggleEvent<TID> surrogate)
        {
            if(surrogate == null) return default(Toggle.ToggleEvent);
            return (Toggle.ToggleEvent)surrogate.WriteTo(new Toggle.ToggleEvent());
        }
        
        public static implicit operator PersistentToggleNestedToggleEvent<TID>(Toggle.ToggleEvent obj)
        {
            PersistentToggleNestedToggleEvent<TID> surrogate = new PersistentToggleNestedToggleEvent<TID>();
            surrogate.ReadFrom(obj);
            return surrogate;
        }
    }
}

