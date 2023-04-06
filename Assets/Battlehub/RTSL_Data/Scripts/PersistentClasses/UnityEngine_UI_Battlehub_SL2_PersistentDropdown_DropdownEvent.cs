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
    public partial class PersistentDropdownNestedDropdownEvent<TID> : PersistentUnityEventBase<TID>
    {
        
        public static implicit operator Dropdown.DropdownEvent(PersistentDropdownNestedDropdownEvent<TID> surrogate)
        {
            if(surrogate == null) return default(Dropdown.DropdownEvent);
            return (Dropdown.DropdownEvent)surrogate.WriteTo(new Dropdown.DropdownEvent());
        }
        
        public static implicit operator PersistentDropdownNestedDropdownEvent<TID>(Dropdown.DropdownEvent obj)
        {
            PersistentDropdownNestedDropdownEvent<TID> surrogate = new PersistentDropdownNestedDropdownEvent<TID>();
            surrogate.ReadFrom(obj);
            return surrogate;
        }
    }
}

