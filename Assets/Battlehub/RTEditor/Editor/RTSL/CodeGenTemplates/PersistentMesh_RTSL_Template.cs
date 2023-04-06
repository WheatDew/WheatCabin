//#define RTSL_COMPILE_TEMPLATES
#if RTSL_COMPILE_TEMPLATES
//<TEMPLATE_USINGS_START>
using Battlehub.RTSL.Battlehub.SL2;
using ProtoBuf;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
//<TEMPLATE_USINGS_END>
#endif

namespace Battlehub.RTSL.Internal
{
    [PersistentTemplate("UnityEngine.Mesh",
        new[] { "vertices", "subMeshCount", "indexFormat", "triangles", "boneWeights", "bindposes", "bounds", "normals", "tangents", "uv", "uv2", "uv3", "uv4", "uv5", "uv6", "uv7", "uv8", "colors" },
        new[] { "UnityEngine.Vector2", "UnityEngine.Vector3", "UnityEngine.Vector4", "UnityEngine.BoneWeight", "UnityEngine.Matrix4x4", "UnityEngine.Bounds", "UnityEngine.Color" })]
    public class PersistentMesh_RTSL_Template : PersistentSurrogateTemplate
#if RTSL_COMPILE_TEMPLATES
        ,
        //<TEMPLATE_INTERFACES_START>
        ICustomSerialization
        //<TEMPLATE_INTERFACES_END>
#endif
    {
#if RTSL_COMPILE_TEMPLATES
        //<TEMPLATE_BODY_START>    
        [ProtoMember(1)]
        public Vector3[] vertices;

        [ProtoMember(2)]
        public int subMeshCount;

        [ProtoMember(3)]
        public IntArray[] m_tris;

        [ProtoMember(4)]
        public UnityEngine.Rendering.IndexFormat indexFormat;

        [ProtoMember(5)]
        public MeshTopology[] m_topology;

        [ProtoMember(6)]
        public int blendShapeCount;

        [ProtoMember(7)]
        public string[] blendShapeNames;

        [ProtoMember(8)]
        public int[] blendShapeFrameCount;

        [ProtoMember(9)]
        public List<PersistentBlendShapeFrame<TID>> blendShapeFrames;

        [ProtoMember(258)]
        public BoneWeight[] boneWeights;

        [ProtoMember(259)]
        public Matrix4x4[] bindposes;

        [ProtoMember(261)]
        public Bounds bounds;

        [ProtoMember(263)]
        public Vector3[] normals;

        [ProtoMember(264)]
        public Vector4[] tangents;

        [ProtoMember(265)]
        public Vector2[] uv;

        [ProtoMember(266)]
        public Vector2[] uv2;

        [ProtoMember(267)]
        public Vector2[] uv3;

        [ProtoMember(268)]
        public Vector2[] uv4;

        [ProtoMember(269)]
        public Vector2[] uv5;

        [ProtoMember(270)]
        public Vector2[] uv6;

        [ProtoMember(271)]
        public Vector2[] uv7;

        [ProtoMember(272)]
        public Vector2[] uv8;

        [ProtoMember(273)]
        public Color[] colors;


        public override object WriteTo(object obj)
        {
            if (obj == null)
            {
                return null;
            }

            Mesh o = (Mesh)obj;
            if (!m_assetDB.IsStaticResourceID(m_assetDB.ToID(o)))
            {
                o.indexFormat = indexFormat;
                if (vertices != null)
                {
                    o.vertices = vertices;
                }

                o.subMeshCount = subMeshCount;
                if (m_tris != null)
                {
                    if (m_topology != null && m_topology.Length == subMeshCount)
                    {
                        for (int i = 0; i < subMeshCount; ++i)
                        {
                            MeshTopology topology = m_topology[i];
                            switch (topology)
                            {
                                case MeshTopology.Points:
                                case MeshTopology.Lines:
                                    o.SetIndices(m_tris[i].Array, topology, i);
                                    break;
                                case MeshTopology.Triangles:
                                    o.SetTriangles(m_tris[i].Array, i);
                                    break;
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < subMeshCount; ++i)
                        {
                            o.SetTriangles(m_tris[i].Array, i);
                        }
                    }
                }

                o.boneWeights = boneWeights;
                o.bindposes = bindposes;
                o.bounds = bounds;
                o.normals = normals;
                o.tangents = tangents;
                o.uv = uv;
                o.uv2 = uv2;
                o.uv3 = uv3;
                o.uv4 = uv4;
                o.uv5 = uv5;
                o.uv6 = uv6;
                o.uv7 = uv7;
                o.uv8 = uv8;
                o.colors = colors;

                if(blendShapeCount > 0)
                {
                    o.ClearBlendShapes();

                    int index = 0;
                    for (int shapeIndex = 0; shapeIndex < blendShapeCount; ++shapeIndex)
                    {
                        int frameCount = blendShapeFrameCount[shapeIndex];
                        string blendShapeName = blendShapeNames[shapeIndex];
                        for (int frameIndex = 0; frameIndex < frameCount; ++frameIndex)
                        {
                            PersistentBlendShapeFrame<TID> frame = blendShapeFrames[index];
                            o.AddBlendShapeFrame(blendShapeName, frame.Weight, frame.DeltaVertices, frame.DeltaNormals, frame.DeltaTangents);
                            index++;
                        }
                    }
                }
            }

            return base.WriteTo(obj);
        }

        public override void ReadFrom(object obj)
        {
            base.ReadFrom(obj);
            if (obj == null)
            {
                return;
            }
            Mesh o = (Mesh)obj;
            if (!m_assetDB.IsStaticResourceID(m_assetDB.ToID(o)))
            {
                boneWeights = o.boneWeights;
                bindposes = o.bindposes;
                bounds = o.bounds;
                normals = o.normals;
                tangents = o.tangents;
                uv = o.uv;
                uv2 = o.uv2;
                uv3 = o.uv3;
                uv4 = o.uv4;
                uv5 = o.uv5;
                uv6 = o.uv6;
                uv7 = o.uv7;
                uv8 = o.uv8;
                colors = o.colors;

                indexFormat = o.indexFormat;
                subMeshCount = o.subMeshCount;
                if (o.vertices != null)
                {
                    vertices = o.vertices;
                }

                m_tris = new IntArray[subMeshCount];
                m_topology = new MeshTopology[subMeshCount];
                for (int i = 0; i < subMeshCount; ++i)
                {
                    MeshTopology topology = o.GetTopology(i);
                    m_topology[i] = topology;
                    switch (topology)
                    {
                        case MeshTopology.Points:
                            m_tris[i] = new IntArray();
                            m_tris[i].Array = o.GetIndices(i);
                            break;
                        case MeshTopology.Lines:
                            m_tris[i] = new IntArray();
                            m_tris[i].Array = o.GetIndices(i);
                            break;
                        case MeshTopology.Triangles:
                            m_tris[i] = new IntArray();
                            m_tris[i].Array = o.GetTriangles(i);
                            break;
                    }
                }

                blendShapeCount = o.blendShapeCount;
                if (blendShapeCount > 0)
                {
                    blendShapeNames = new string[blendShapeCount];
                    blendShapeFrameCount = new int[blendShapeCount];
                    blendShapeFrames = new List<PersistentBlendShapeFrame<TID>>();
                    int vertexCount = o.vertexCount;
                    for (int shapeIndex = 0; shapeIndex < blendShapeCount; ++shapeIndex)
                    {
                        int frameCount = o.GetBlendShapeFrameCount(shapeIndex);
                        blendShapeFrameCount[shapeIndex] = frameCount;

                        string blendShapeName = o.GetBlendShapeName(shapeIndex);
                        blendShapeNames[shapeIndex] = blendShapeName;

                        for (int frameIndex = 0; frameIndex < frameCount; ++frameIndex)
                        {
                            float weight = o.GetBlendShapeFrameWeight(shapeIndex, frameIndex);

                            Vector3[] deltaVertices = new Vector3[vertexCount];
                            Vector3[] deltaNormals = new Vector3[vertexCount];
                            Vector3[] deltaTangents = new Vector3[vertexCount];
                            o.GetBlendShapeFrameVertices(shapeIndex, frameIndex, deltaVertices, deltaNormals, deltaTangents);

                            PersistentBlendShapeFrame<TID> frame = new PersistentBlendShapeFrame<TID>(weight, deltaVertices, deltaNormals, deltaTangents);
                            blendShapeFrames.Add(frame);
                        }
                    }
                }
            }
        }

        public bool AllowStandardSerialization
        {
            get { return false; }
        }

        public void Serialize(Stream stream, BinaryWriter writer)
        {
            writer.Write(name);
            writer.Write(hideFlags);
            writer.Write(vertices);
            writer.Write(subMeshCount);
            writer.Write(m_tris.Length);
            for (int i = 0; i < m_tris.Length; ++i)
            {
                writer.Write(m_tris[i].Array);
            }

            writer.Write((int)indexFormat);
            writer.Write(m_topology.Length);
            for (int i = 0; i < m_topology.Length; ++i)
            {
                writer.Write((int)m_topology[i]);
            }

            writer.Write(boneWeights);
            writer.Write(bindposes);
            writer.Write(bounds);
            writer.Write(normals);
            writer.Write(tangents);
            writer.Write(colors);
            writer.Write(uv);
            writer.Write(uv2);
            writer.Write(uv3);
            writer.Write(uv4);
            writer.Write(uv5);
            writer.Write(uv6);
            writer.Write(uv7);
            writer.Write(uv8);
        }

        public void Deserialize(Stream stream, BinaryReader reader)
        {
            name = reader.ReadString();
            hideFlags = reader.ReadInt32();
            vertices = reader.ReadVector3Array();
            subMeshCount = reader.ReadInt32();
            m_tris = new IntArray[reader.ReadInt32()];
            for (int i = 0; i < m_tris.Length; ++i)
            {
                m_tris[i] = new IntArray
                {
                    Array = reader.ReadInt32Array()
                };
            }
            indexFormat = (UnityEngine.Rendering.IndexFormat)reader.ReadInt32();
            m_topology = new MeshTopology[reader.ReadInt32()];
            for (int i = 0; i < m_topology.Length; ++i)
            {
                m_topology[i] = (MeshTopology)reader.ReadInt32();
            }
            boneWeights = reader.ReadBoneWeightsArray();
            bindposes = reader.ReadMatrixArray();
            bounds = reader.ReadBounds();
            normals = reader.ReadVector3Array();
            tangents = reader.ReadVector4Array();
            colors = reader.ReadColorArray();
            uv = reader.ReadVector2Array();
            uv2 = reader.ReadVector2Array();
            uv3 = reader.ReadVector2Array();
            uv4 = reader.ReadVector2Array();
            uv5 = reader.ReadVector2Array();
            uv6 = reader.ReadVector2Array();
            uv7 = reader.ReadVector2Array();
            uv8 = reader.ReadVector2Array();
        }
        //<TEMPLATE_BODY_END>
#endif
    }
}


