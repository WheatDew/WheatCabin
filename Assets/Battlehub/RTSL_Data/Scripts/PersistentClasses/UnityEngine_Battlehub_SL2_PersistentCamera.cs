using System.Collections.Generic;
using ProtoBuf;
using Battlehub.RTSL;
using UnityEngine;
using UnityEngine.Battlehub.SL2;
using System;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Battlehub.SL2;
using UnityEngine.SceneManagement;
using UnityEngine.SceneManagement.Battlehub.SL2;

using UnityObject = UnityEngine.Object;
namespace UnityEngine.Battlehub.SL2
{
    [ProtoContract]
    public partial class PersistentCamera<TID> : PersistentBehaviour<TID>
    {
        [ProtoMember(256)]
        public float nearClipPlane;

        [ProtoMember(257)]
        public float farClipPlane;

        [ProtoMember(258)]
        public float fieldOfView;

        [ProtoMember(259)]
        public RenderingPath renderingPath;

        [ProtoMember(260)]
        public bool allowHDR;

        [ProtoMember(261)]
        public bool allowMSAA;

        [ProtoMember(262)]
        public bool allowDynamicResolution;

        [ProtoMember(263)]
        public bool forceIntoRenderTexture;

        [ProtoMember(264)]
        public float orthographicSize;

        [ProtoMember(265)]
        public bool orthographic;

        [ProtoMember(266)]
        public OpaqueSortMode opaqueSortMode;

        [ProtoMember(267)]
        public TransparencySortMode transparencySortMode;

        [ProtoMember(268)]
        public PersistentVector3<TID> transparencySortAxis;

        [ProtoMember(269)]
        public float depth;

        [ProtoMember(271)]
        public int cullingMask;

        [ProtoMember(272)]
        public int eventMask;

        [ProtoMember(273)]
        public bool layerCullSpherical;

        [ProtoMember(274)]
        public CameraType cameraType;

        [ProtoMember(275)]
        public float[] layerCullDistances;

        [ProtoMember(276)]
        public bool useOcclusionCulling;

        [ProtoMember(278)]
        public PersistentColor<TID> backgroundColor;

        [ProtoMember(279)]
        public CameraClearFlags clearFlags;

        [ProtoMember(280)]
        public DepthTextureMode depthTextureMode;

        [ProtoMember(281)]
        public bool clearStencilAfterLightingPass;

        [ProtoMember(282)]
        public bool usePhysicalProperties;

        [ProtoMember(283)]
        public PersistentVector2<TID> sensorSize;

        [ProtoMember(284)]
        public PersistentVector2<TID> lensShift;

        [ProtoMember(285)]
        public float focalLength;

        [ProtoMember(289)]
        public int targetDisplay;

        [ProtoMember(294)]
        public PersistentScene<TID> scene;

        [ProtoMember(295)]
        public float stereoSeparation;

        [ProtoMember(296)]
        public float stereoConvergence;

        [ProtoMember(297)]
        public StereoTargetEyeMask stereoTargetEye;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            Camera uo = (Camera)obj;
            nearClipPlane = uo.nearClipPlane;
            farClipPlane = uo.farClipPlane;
            fieldOfView = uo.fieldOfView;
            renderingPath = uo.renderingPath;
            allowHDR = uo.allowHDR;
            allowMSAA = uo.allowMSAA;
            allowDynamicResolution = uo.allowDynamicResolution;
            forceIntoRenderTexture = uo.forceIntoRenderTexture;
            orthographicSize = uo.orthographicSize;
            orthographic = uo.orthographic;
            opaqueSortMode = uo.opaqueSortMode;
            transparencySortMode = uo.transparencySortMode;
            transparencySortAxis = uo.transparencySortAxis;
            depth = uo.depth;
            cullingMask = uo.cullingMask;
            eventMask = uo.eventMask;
            layerCullSpherical = uo.layerCullSpherical;
            cameraType = uo.cameraType;
            layerCullDistances = uo.layerCullDistances;
            useOcclusionCulling = uo.useOcclusionCulling;
            backgroundColor = uo.backgroundColor;
            clearFlags = uo.clearFlags;
            depthTextureMode = uo.depthTextureMode;
            clearStencilAfterLightingPass = uo.clearStencilAfterLightingPass;
            usePhysicalProperties = uo.usePhysicalProperties;
            sensorSize = uo.sensorSize;
            lensShift = uo.lensShift;
            focalLength = uo.focalLength;
            targetDisplay = uo.targetDisplay;
            scene = uo.scene;
            stereoSeparation = uo.stereoSeparation;
            stereoConvergence = uo.stereoConvergence;
            stereoTargetEye = uo.stereoTargetEye;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            Camera uo = (Camera)obj;
            uo.nearClipPlane = nearClipPlane;
            uo.farClipPlane = farClipPlane;
            uo.fieldOfView = fieldOfView;
            uo.renderingPath = renderingPath;
            uo.allowHDR = allowHDR;
            uo.allowMSAA = allowMSAA;
            uo.allowDynamicResolution = allowDynamicResolution;
            uo.forceIntoRenderTexture = forceIntoRenderTexture;
            uo.orthographicSize = orthographicSize;
            uo.orthographic = orthographic;
            uo.opaqueSortMode = opaqueSortMode;
            uo.transparencySortMode = transparencySortMode;
            uo.transparencySortAxis = transparencySortAxis;
            uo.depth = depth;
            uo.cullingMask = cullingMask;
            uo.eventMask = eventMask;
            uo.layerCullSpherical = layerCullSpherical;
            uo.cameraType = cameraType;
            uo.layerCullDistances = layerCullDistances;
            uo.useOcclusionCulling = useOcclusionCulling;
            uo.backgroundColor = backgroundColor;
            uo.clearFlags = clearFlags;
            uo.depthTextureMode = depthTextureMode;
            uo.clearStencilAfterLightingPass = clearStencilAfterLightingPass;
            uo.usePhysicalProperties = usePhysicalProperties;
            uo.sensorSize = sensorSize;
            uo.lensShift = lensShift;
            uo.focalLength = focalLength;
            uo.targetDisplay = targetDisplay;
            uo.scene = scene;
            uo.stereoSeparation = stereoSeparation;
            uo.stereoConvergence = stereoConvergence;
            uo.stereoTargetEye = stereoTargetEye;
            return uo;
        }
    }
}

