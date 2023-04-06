using System.Collections.Generic;
using ProtoBuf;
using Battlehub.RTSL;
using UnityEngine;
using UnityEngine.Battlehub.SL2;
using System;

using UnityObject = UnityEngine.Object;
namespace UnityEngine.Battlehub.SL2
{
    [ProtoContract]
    public partial class PersistentCanvas<TID> : PersistentBehaviour<TID>
    {
        [ProtoMember(256)]
        public RenderMode renderMode;

        [ProtoMember(257)]
        public float scaleFactor;

        [ProtoMember(258)]
        public float referencePixelsPerUnit;

        [ProtoMember(259)]
        public bool overridePixelPerfect;

        [ProtoMember(260)]
        public bool pixelPerfect;

        [ProtoMember(261)]
        public float planeDistance;

        [ProtoMember(262)]
        public bool overrideSorting;

        [ProtoMember(263)]
        public int sortingOrder;

        [ProtoMember(264)]
        public int targetDisplay;

        [ProtoMember(265)]
        public int sortingLayerID;

        [ProtoMember(266)]
        public AdditionalCanvasShaderChannels additionalShaderChannels;

        [ProtoMember(267)]
        public string sortingLayerName;

        [ProtoMember(268)]
        public TID worldCamera;

        [ProtoMember(269)]
        public float normalizedSortingGridSize;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            Canvas uo = (Canvas)obj;
            renderMode = uo.renderMode;
            scaleFactor = uo.scaleFactor;
            referencePixelsPerUnit = uo.referencePixelsPerUnit;
            overridePixelPerfect = uo.overridePixelPerfect;
            pixelPerfect = uo.pixelPerfect;
            planeDistance = uo.planeDistance;
            overrideSorting = uo.overrideSorting;
            sortingOrder = uo.sortingOrder;
            targetDisplay = uo.targetDisplay;
            sortingLayerID = uo.sortingLayerID;
            additionalShaderChannels = uo.additionalShaderChannels;
            sortingLayerName = uo.sortingLayerName;
            worldCamera = ToID(uo.worldCamera);
            normalizedSortingGridSize = uo.normalizedSortingGridSize;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            Canvas uo = (Canvas)obj;
            uo.renderMode = renderMode;
            uo.scaleFactor = scaleFactor;
            uo.referencePixelsPerUnit = referencePixelsPerUnit;
            uo.overridePixelPerfect = overridePixelPerfect;
            uo.pixelPerfect = pixelPerfect;
            uo.planeDistance = planeDistance;
            uo.overrideSorting = overrideSorting;
            uo.sortingOrder = sortingOrder;
            uo.targetDisplay = targetDisplay;
            uo.sortingLayerID = sortingLayerID;
            uo.additionalShaderChannels = additionalShaderChannels;
            uo.sortingLayerName = sortingLayerName;
            uo.worldCamera = FromID(worldCamera, uo.worldCamera);
            uo.normalizedSortingGridSize = normalizedSortingGridSize;
            return uo;
        }

        protected override void GetDepsImpl(GetDepsContext<TID> context)
        {
            base.GetDepsImpl(context);
            AddDep(worldCamera, context);
        }

        protected override void GetDepsFromImpl(object obj, GetDepsFromContext context)
        {
            base.GetDepsFromImpl(obj, context);
            Canvas uo = (Canvas)obj;
            AddDep(uo.worldCamera, context);
        }
    }
}

