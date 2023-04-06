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
    public partial class PersistentScrollbarNestedScrollEvent<TID> : PersistentUnityEventBase<TID>
    {
        
        public static implicit operator Scrollbar.ScrollEvent(PersistentScrollbarNestedScrollEvent<TID> surrogate)
        {
            if(surrogate == null) return default(Scrollbar.ScrollEvent);
            return (Scrollbar.ScrollEvent)surrogate.WriteTo(new Scrollbar.ScrollEvent());
        }
        
        public static implicit operator PersistentScrollbarNestedScrollEvent<TID>(Scrollbar.ScrollEvent obj)
        {
            PersistentScrollbarNestedScrollEvent<TID> surrogate = new PersistentScrollbarNestedScrollEvent<TID>();
            surrogate.ReadFrom(obj);
            return surrogate;
        }
    }
}

