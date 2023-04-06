using System.Collections.Generic;
using ProtoBuf;
using Battlehub.RTSL;
using Battlehub.RTCommon;
using Battlehub.RTCommon.Battlehub.SL2;
using UnityEngine.Events.Battlehub.SL2;

using UnityObject = UnityEngine.Object;
namespace Battlehub.RTCommon.Battlehub.SL2
{
    [ProtoContract]
    public partial class PersistentExposeToEditorUnityEvent<TID> : PersistentUnityEventBase<TID>
    {
        
        public static implicit operator ExposeToEditorUnityEvent(PersistentExposeToEditorUnityEvent<TID> surrogate)
        {
            if(surrogate == null) return default(ExposeToEditorUnityEvent);
            return (ExposeToEditorUnityEvent)surrogate.WriteTo(new ExposeToEditorUnityEvent());
        }
        
        public static implicit operator PersistentExposeToEditorUnityEvent<TID>(ExposeToEditorUnityEvent obj)
        {
            PersistentExposeToEditorUnityEvent<TID> surrogate = new PersistentExposeToEditorUnityEvent<TID>();
            surrogate.ReadFrom(obj);
            return surrogate;
        }
    }
}

