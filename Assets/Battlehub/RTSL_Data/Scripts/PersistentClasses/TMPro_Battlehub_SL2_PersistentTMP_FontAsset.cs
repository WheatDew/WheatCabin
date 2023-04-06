using System.Collections.Generic;
using ProtoBuf;
using Battlehub.RTSL;
using TMPro;
using TMPro.Battlehub.SL2;
using UnityEngine;
using System;

using UnityObject = UnityEngine.Object;
namespace TMPro.Battlehub.SL2
{
    [ProtoContract]
    public partial class PersistentTMP_FontAsset<TID> : PersistentTMP_Asset<TID>
    {
        [ProtoMember(256)]
        public TID atlas;

        [ProtoMember(257)]
        public float normalStyle;

        [ProtoMember(258)]
        public float normalSpacingOffset;

        [ProtoMember(259)]
        public float boldStyle;

        [ProtoMember(260)]
        public float boldSpacing;

        [ProtoMember(261)]
        public byte italicStyle;

        [ProtoMember(262)]
        public byte tabSize;

        [ProtoMember(263)]
        public string m_Version;

        [ProtoMember(304)]
        public AtlasPopulationMode atlasPopulationMode;

        [ProtoMember(306)]
        public TID[] atlasTextures;

        [ProtoMember(307)]
        public bool isMultiAtlasTexturesEnabled;

        [ProtoMember(308)]
        public TID[] fallbackFontAssetTable;

        [ProtoMember(309)]
        public PersistentFontAssetCreationSettings<TID> creationSettings;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            TMP_FontAsset uo = (TMP_FontAsset)obj;
            atlas = ToID(uo.atlas);
            normalStyle = uo.normalStyle;
            normalSpacingOffset = uo.normalSpacingOffset;
            boldStyle = uo.boldStyle;
            boldSpacing = uo.boldSpacing;
            italicStyle = uo.italicStyle;
            tabSize = uo.tabSize;
            m_Version = GetPrivate<TMP_FontAsset,string>(uo, "m_Version");
            atlasPopulationMode = uo.atlasPopulationMode;
            atlasTextures = ToID(uo.atlasTextures);
            isMultiAtlasTexturesEnabled = uo.isMultiAtlasTexturesEnabled;
            fallbackFontAssetTable = ToID(uo.fallbackFontAssetTable);
            creationSettings = uo.creationSettings;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            TMP_FontAsset uo = (TMP_FontAsset)obj;
            uo.atlas = FromID(atlas, uo.atlas);
            uo.normalStyle = normalStyle;
            uo.normalSpacingOffset = normalSpacingOffset;
            uo.boldStyle = boldStyle;
            uo.boldSpacing = boldSpacing;
            uo.italicStyle = italicStyle;
            uo.tabSize = tabSize;
            SetPrivate(uo, "m_Version", m_Version);
            uo.atlasPopulationMode = atlasPopulationMode;
            uo.atlasTextures = FromID(atlasTextures, uo.atlasTextures);
            uo.isMultiAtlasTexturesEnabled = isMultiAtlasTexturesEnabled;
            uo.fallbackFontAssetTable = FromID(fallbackFontAssetTable, uo.fallbackFontAssetTable);
            uo.creationSettings = creationSettings;
            return uo;
        }

        protected override void GetDepsImpl(GetDepsContext<TID> context)
        {
            base.GetDepsImpl(context);
            AddDep(atlas, context);
            AddDep(atlasTextures, context);
            AddDep(fallbackFontAssetTable, context);
        }

        protected override void GetDepsFromImpl(object obj, GetDepsFromContext context)
        {
            base.GetDepsFromImpl(obj, context);
            TMP_FontAsset uo = (TMP_FontAsset)obj;
            AddDep(uo.atlas, context);
            AddDep(uo.atlasTextures, context);
            AddDep(uo.fallbackFontAssetTable, context);
        }
    }
}

