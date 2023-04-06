using System.IO;
using UnityEngine;

namespace Battlehub.RTSL
{
    public interface ICustomSerialization
    {
        /// <summary>
        /// Return true if standard serialization using protobuf-net allowed
        /// </summary>
        bool AllowStandardSerialization
        {
            get;
        }
        /// <summary>
        /// Custom serialization
        /// </summary>
        /// <param name="stream">stream</param>
        /// <param name="writer">BinaryWriter for writing into stream</param>
        void Serialize(Stream stream, BinaryWriter writer);

        /// <summary>
        /// Custom deserialization
        /// </summary>
        /// <param name="stream">stream</param>
        /// <param name="reader">BinaryReader for reading from stream</param>
        void Deserialize(Stream stream, BinaryReader reader);
    }

    public struct CustomSerializationHeader
    {
        public int Magic;
        public int Version;
        public int Reserved;

        public static CustomSerializationHeader Default = new CustomSerializationHeader
        {
            Magic = 0x12344321,
            Version = 224
        };

        public bool IsValid
        {
            get { return Magic == Default.Magic; }
        }
    }

    public static class BinaryReaderWriterExtensions
    {
        public static void Write(this BinaryWriter writer, CustomSerializationHeader header)
        {
            writer.Write(header.Magic);
            writer.Write(header.Version);
            writer.Write(header.Reserved);
        }

        public static CustomSerializationHeader ReadHeader(this BinaryReader reader)
        {
            return new CustomSerializationHeader
            {
                Magic = reader.ReadInt32(),
                Version = reader.ReadInt32(),
                Reserved = reader.ReadInt32()
            };
        }

        public static void Write(this BinaryWriter writer, int[] intArray)
        {
            if (intArray == null)
            {
                writer.Write(-1);
            }
            else
            {
                writer.Write(intArray.Length);
                for (int i = 0; i < intArray.Length; ++i)
                {
                    writer.Write(intArray[i]);
                }
            }
        }

        public static int[] ReadInt32Array(this BinaryReader reader)
        {
            int len = reader.ReadInt32();
            if (len == -1)
            {
                return null;
            }

            int[] result = new int[len];
            for (int i = 0; i < len; ++i)
            {
                result[i] = reader.ReadInt32();
            }
            return result;
        }

        public static void Write(this BinaryWriter writer, Vector2 vec2)
        {
            writer.Write(vec2.x);
            writer.Write(vec2.y);
        }
        public static Vector2 ReadVector2(this BinaryReader reader)
        {
            return new Vector2(reader.ReadSingle(), reader.ReadSingle());
        }

        public static void Write(this BinaryWriter writer, Vector2[] vec2Array)
        {
            if(vec2Array == null)
            {
                writer.Write(-1);
            }
            else
            {
                writer.Write(vec2Array.Length);
                for(int i = 0; i < vec2Array.Length; ++i)
                {
                    writer.Write(vec2Array[i]);
                }
            }
        }

        public static Vector2[] ReadVector2Array(this BinaryReader reader)
        {
            int len = reader.ReadInt32();
            if(len == -1)
            {
                return null;
            }

            Vector2[] result = new Vector2[len];
            for(int i = 0; i < len; ++i)
            {
                result[i] = reader.ReadVector2();
            }
            return result;
        }

        public static void Write(this BinaryWriter writer, Vector3 vec3)
        {
            writer.Write(vec3.x);
            writer.Write(vec3.y);
            writer.Write(vec3.z);
        }

        public static Vector3 ReadVector3(this BinaryReader reader)
        {
            return new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }

        public static void Write(this BinaryWriter writer, Vector3[] vec3Array)
        {
            if (vec3Array == null)
            {
                writer.Write(-1);
            }
            else
            {
                writer.Write(vec3Array.Length);
                for (int i = 0; i < vec3Array.Length; ++i)
                {
                    writer.Write(vec3Array[i]);
                }
            }
        }

        public static Vector3[] ReadVector3Array(this BinaryReader reader)
        {
            int len = reader.ReadInt32();
            if (len == -1)
            {
                return null;
            }

            Vector3[] result = new Vector3[len];
            for (int i = 0; i < len; ++i)
            {
                result[i] = reader.ReadVector3();
            }
            return result;
        }


        public static void Write(this BinaryWriter writer, Vector4 vec4)
        {
            writer.Write(vec4.x);
            writer.Write(vec4.y);
            writer.Write(vec4.z);
            writer.Write(vec4.w);
        }

        public static Vector4 ReadVector4(this BinaryReader reader)
        {
            return new Vector4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }

        public static void Write(this BinaryWriter writer, Vector4[] vec4Array)
        {
            if (vec4Array == null)
            {
                writer.Write(-1);
            }
            else
            {
                writer.Write(vec4Array.Length);
                for (int i = 0; i < vec4Array.Length; ++i)
                {
                    writer.Write(vec4Array[i]);
                }
            }
        }

        public static Vector4[] ReadVector4Array(this BinaryReader reader)
        {
            int len = reader.ReadInt32();
            if (len == -1)
            {
                return null;
            }

            Vector4[] result = new Vector4[len];
            for (int i = 0; i < len; ++i)
            {
                result[i] = reader.ReadVector4();
            }
            return result;
        }

        public static void Write(this BinaryWriter writer, Quaternion quat)
        {
            writer.Write(quat.x);
            writer.Write(quat.y);
            writer.Write(quat.z);
            writer.Write(quat.w);
        }

        public static Quaternion ReadQuaternion(this BinaryReader reader)
        {
            return new Quaternion(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }

        public static void Write(this BinaryWriter writer, Quaternion[] quatArray)
        {
            if (quatArray == null)
            {
                writer.Write(-1);
            }
            else
            {
                writer.Write(quatArray.Length);
                for (int i = 0; i < quatArray.Length; ++i)
                {
                    writer.Write(quatArray[i]);
                }
            }
        }

        public static Quaternion[] ReadQuaternionArray(this BinaryReader reader)
        {
            int len = reader.ReadInt32();
            if (len == -1)
            {
                return null;
            }

            Quaternion[] result = new Quaternion[len];
            for (int i = 0; i < len; ++i)
            {
                result[i] = reader.ReadQuaternion();
            }
            return result;
        }

        public static void Write(this BinaryWriter writer, Color color)
        {
            writer.Write(color.r);
            writer.Write(color.g);
            writer.Write(color.b);
            writer.Write(color.a);
        }

        public static Color ReadColor(this BinaryReader reader)
        {
            return new Color(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }

        public static void Write(this BinaryWriter writer, Color[] colorArray)
        {
            if (colorArray == null)
            {
                writer.Write(-1);
            }
            else
            {
                writer.Write(colorArray.Length);
                for (int i = 0; i < colorArray.Length; ++i)
                {
                    writer.Write(colorArray[i]);
                }
            }
        }

        public static Color[] ReadColorArray(this BinaryReader reader)
        {
            int len = reader.ReadInt32();
            if (len == -1)
            {
                return null;
            }

            Color[] result = new Color[len];
            for (int i = 0; i < len; ++i)
            {
                result[i] = reader.ReadColor();
            }
            return result;
        }

        public static void Write(this BinaryWriter writer, Matrix4x4 matrix)
        {
            writer.Write(matrix.GetColumn(0));
            writer.Write(matrix.GetColumn(1));
            writer.Write(matrix.GetColumn(2));
            writer.Write(matrix.GetColumn(3));
        }

        public static Matrix4x4 ReadMatrix(this BinaryReader reader)
        {
            return new Matrix4x4(ReadVector4(reader), ReadVector4(reader), ReadVector4(reader), ReadVector4(reader));
        }

        public static void Write(this BinaryWriter writer, Matrix4x4[] matrixArray)
        {
            if (matrixArray == null)
            {
                writer.Write(-1);
            }
            else
            {
                writer.Write(matrixArray.Length);
                for (int i = 0; i < matrixArray.Length; ++i)
                {
                    writer.Write(matrixArray[i]);
                }
            }
        }

        public static Matrix4x4[] ReadMatrixArray(this BinaryReader reader)
        {
            int len = reader.ReadInt32();
            if (len == -1)
            {
                return null;
            }

            Matrix4x4[] result = new Matrix4x4[len];
            for (int i = 0; i < len; ++i)
            {
                result[i] = reader.ReadMatrix();
            }
            return result;
        }

        public static void Write(this BinaryWriter writer, Bounds bounds)
        {
            writer.Write(bounds.center);
            writer.Write(bounds.size);
        }

        public static Bounds ReadBounds(this BinaryReader reader)
        {
            return new Bounds(reader.ReadVector3(), reader.ReadVector3());
        }

        public static void Write(this BinaryWriter writer, BoneWeight weight)
        {
            writer.Write(weight.boneIndex0);
            writer.Write(weight.boneIndex1);
            writer.Write(weight.boneIndex2);
            writer.Write(weight.boneIndex3);
            writer.Write(weight.weight0);
            writer.Write(weight.weight1);
            writer.Write(weight.weight2);
            writer.Write(weight.weight3);
        }

        public static BoneWeight ReadBoneWeight(this BinaryReader reader)
        {
            return new BoneWeight
            {
                boneIndex0 = reader.ReadInt32(),
                boneIndex1 = reader.ReadInt32(),
                boneIndex2 = reader.ReadInt32(),
                boneIndex3 = reader.ReadInt32(),
                weight0 = reader.ReadSingle(),
                weight1 = reader.ReadSingle(),
                weight2 = reader.ReadSingle(),
                weight3 = reader.ReadSingle()
            };
        }

        public static void Write(this BinaryWriter writer, BoneWeight[] weightArray)
        {
            if (weightArray == null)
            {
                writer.Write(-1);
            }
            else
            {
                writer.Write(weightArray.Length);
                for (int i = 0; i < weightArray.Length; ++i)
                {
                    writer.Write(weightArray[i]);
                }
            }
        }

        public static BoneWeight[] ReadBoneWeightsArray(this BinaryReader reader)
        {
            int len = reader.ReadInt32();
            if (len == -1)
            {
                return null;
            }

            BoneWeight[] result = new BoneWeight[len];
            for (int i = 0; i < len; ++i)
            {
                result[i] = reader.ReadBoneWeight();
            }
            return result;
        }
    }
}

