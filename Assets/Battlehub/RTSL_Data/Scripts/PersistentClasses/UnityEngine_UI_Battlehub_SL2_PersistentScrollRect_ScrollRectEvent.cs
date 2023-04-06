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
    public partial class PersistentScrollRectNestedScrollRectEvent<TID> : PersistentUnityEventBase<TID>
    {
        
        public static implicit operator ScrollRect.ScrollRectEvent(PersistentScrollRectNestedScrollRectEvent<TID> surrogate)
        {
            if(surrogate == null) return default(ScrollRect.ScrollRectEvent);
            return (ScrollRect.ScrollRectEvent)surrogate.WriteTo(new ScrollRect.ScrollRectEvent());
        }
        
        public static implicit operator PersistentScrollRectNestedScrollRectEvent<TID>(ScrollRect.ScrollRectEvent obj)
        {
            PersistentScrollRectNestedScrollRectEvent<TID> surrogate = new PersistentScrollRectNestedScrollRectEvent<TID>();
            surrogate.ReadFrom(obj);
            return surrogate;
        }
    }
}

