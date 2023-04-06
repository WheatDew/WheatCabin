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
    public partial class PersistentMatrix4x4<TID> : PersistentSurrogate<TID>
    {
        [ProtoMember(256)]
        public float m00;

        [ProtoMember(257)]
        public float m10;

        [ProtoMember(258)]
        public float m20;

        [ProtoMember(259)]
        public float m30;

        [ProtoMember(260)]
        public float m01;

        [ProtoMember(261)]
        public float m11;

        [ProtoMember(262)]
        public float m21;

        [ProtoMember(263)]
        public float m31;

        [ProtoMember(264)]
        public float m02;

        [ProtoMember(265)]
        public float m12;

        [ProtoMember(266)]
        public float m22;

        [ProtoMember(267)]
        public float m32;

        [ProtoMember(268)]
        public float m03;

        [ProtoMember(269)]
        public float m13;

        [ProtoMember(270)]
        public float m23;

        [ProtoMember(271)]
        public float m33;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            Matrix4x4 uo = (Matrix4x4)obj;
            m00 = uo.m00;
            m10 = uo.m10;
            m20 = uo.m20;
            m30 = uo.m30;
            m01 = uo.m01;
            m11 = uo.m11;
            m21 = uo.m21;
            m31 = uo.m31;
            m02 = uo.m02;
            m12 = uo.m12;
            m22 = uo.m22;
            m32 = uo.m32;
            m03 = uo.m03;
            m13 = uo.m13;
            m23 = uo.m23;
            m33 = uo.m33;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            Matrix4x4 uo = (Matrix4x4)obj;
            uo.m00 = m00;
            uo.m10 = m10;
            uo.m20 = m20;
            uo.m30 = m30;
            uo.m01 = m01;
            uo.m11 = m11;
            uo.m21 = m21;
            uo.m31 = m31;
            uo.m02 = m02;
            uo.m12 = m12;
            uo.m22 = m22;
            uo.m32 = m32;
            uo.m03 = m03;
            uo.m13 = m13;
            uo.m23 = m23;
            uo.m33 = m33;
            return uo;
        }

        public static implicit operator Matrix4x4(PersistentMatrix4x4<TID> surrogate)
        {
            if(surrogate == null) return default(Matrix4x4);
            return (Matrix4x4)surrogate.WriteTo(new Matrix4x4());
        }
        
        public static implicit operator PersistentMatrix4x4<TID>(Matrix4x4 obj)
        {
            PersistentMatrix4x4<TID> surrogate = new PersistentMatrix4x4<TID>();
            surrogate.ReadFrom(obj);
            return surrogate;
        }
    }
}

