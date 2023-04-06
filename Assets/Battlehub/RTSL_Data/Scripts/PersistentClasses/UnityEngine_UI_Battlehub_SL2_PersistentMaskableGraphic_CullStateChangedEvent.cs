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
    public partial class PersistentMaskableGraphicNestedCullStateChangedEvent<TID> : PersistentUnityEventBase<TID>
    {
        
        public static implicit operator MaskableGraphic.CullStateChangedEvent(PersistentMaskableGraphicNestedCullStateChangedEvent<TID> surrogate)
        {
            if(surrogate == null) return default(MaskableGraphic.CullStateChangedEvent);
            return (MaskableGraphic.CullStateChangedEvent)surrogate.WriteTo(new MaskableGraphic.CullStateChangedEvent());
        }
        
        public static implicit operator PersistentMaskableGraphicNestedCullStateChangedEvent<TID>(MaskableGraphic.CullStateChangedEvent obj)
        {
            PersistentMaskableGraphicNestedCullStateChangedEvent<TID> surrogate = new PersistentMaskableGraphicNestedCullStateChangedEvent<TID>();
            surrogate.ReadFrom(obj);
            return surrogate;
        }
    }
}

