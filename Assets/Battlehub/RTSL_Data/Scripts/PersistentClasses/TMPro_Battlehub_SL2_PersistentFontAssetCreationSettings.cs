using System.Collections.Generic;
using ProtoBuf;
using Battlehub.RTSL;
using TMPro;
using TMPro.Battlehub.SL2;

using UnityObject = UnityEngine.Object;
namespace TMPro.Battlehub.SL2
{
    [ProtoContract]
    public partial class PersistentFontAssetCreationSettings<TID> : PersistentSurrogate<TID>
    {
        
        public static implicit operator FontAssetCreationSettings(PersistentFontAssetCreationSettings<TID> surrogate)
        {
            if(surrogate == null) return default(FontAssetCreationSettings);
            return (FontAssetCreationSettings)surrogate.WriteTo(new FontAssetCreationSettings());
        }
        
        public static implicit operator PersistentFontAssetCreationSettings<TID>(FontAssetCreationSettings obj)
        {
            PersistentFontAssetCreationSettings<TID> surrogate = new PersistentFontAssetCreationSettings<TID>();
            surrogate.ReadFrom(obj);
            return surrogate;
        }
    }
}

