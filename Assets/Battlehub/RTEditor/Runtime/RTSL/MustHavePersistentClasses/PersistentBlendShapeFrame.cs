using ProtoBuf;
using System;
using UnityEngine;

namespace Battlehub.RTSL.Battlehub.SL2
{
    [ProtoContract]
    public class PersistentBlendShapeFrame<TID>
    {
        [ProtoMember(1)]
        public float Weight;

        [ProtoMember(2)]
        public Vector3[] DeltaVertices;

        [ProtoMember(3)]
        public Vector3[] DeltaNormals;

        [ProtoMember(4)]
        public Vector3[] DeltaTangents;

        public PersistentBlendShapeFrame(float weight, Vector3[] deltaVertices, Vector3[] deltaNormals, Vector3[] deltaTangents)
        {
            Weight = weight;
            DeltaVertices = deltaVertices;
            DeltaNormals = deltaNormals;
            DeltaTangents = deltaTangents;
        }

        public PersistentBlendShapeFrame()
        {
        }
    }

    [Obsolete("Use generic version")]
    [ProtoContract]
    public class PersistentBlendShapeFrame : PersistentBlendShapeFrame<long>
    {
    }
}