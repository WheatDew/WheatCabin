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
    public partial class PersistentSkinnedMeshRenderer<TID> : PersistentRenderer<TID>
    {
        [ProtoMember(256)]
        public SkinQuality quality;

        [ProtoMember(257)]
        public bool updateWhenOffscreen;

        [ProtoMember(258)]
        public TID rootBone;

        [ProtoMember(259)]
        public TID[] bones;

        [ProtoMember(260)]
        public TID sharedMesh;

        [ProtoMember(261)]
        public bool skinnedMotionVectors;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            SkinnedMeshRenderer uo = (SkinnedMeshRenderer)obj;
            quality = uo.quality;
            updateWhenOffscreen = uo.updateWhenOffscreen;
            rootBone = ToID(uo.rootBone);
            bones = ToID(uo.bones);
            sharedMesh = ToID(uo.sharedMesh);
            skinnedMotionVectors = uo.skinnedMotionVectors;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            SkinnedMeshRenderer uo = (SkinnedMeshRenderer)obj;
            uo.quality = quality;
            uo.updateWhenOffscreen = updateWhenOffscreen;
            uo.rootBone = FromID(rootBone, uo.rootBone);
            uo.bones = FromID(bones, uo.bones);
            uo.sharedMesh = FromID(sharedMesh, uo.sharedMesh);
            uo.skinnedMotionVectors = skinnedMotionVectors;
            return uo;
        }

        protected override void GetDepsImpl(GetDepsContext<TID> context)
        {
            base.GetDepsImpl(context);
            AddDep(rootBone, context);
            AddDep(bones, context);
            AddDep(sharedMesh, context);
        }

        protected override void GetDepsFromImpl(object obj, GetDepsFromContext context)
        {
            base.GetDepsFromImpl(obj, context);
            SkinnedMeshRenderer uo = (SkinnedMeshRenderer)obj;
            AddDep(uo.rootBone, context);
            AddDep(uo.bones, context);
            AddDep(uo.sharedMesh, context);
        }
    }
}

