using ProtoBuf;
using Battlehub.RTSL;
using System;

namespace UnityEngine.Battlehub.SL2
{
    [ProtoContract]
    public class PersistentQuaternion<TID> : PersistentSurrogateBase
    {
        [ProtoMember(256)]
        public float x;

        [ProtoMember(257)]
        public float y;

        [ProtoMember(258)]
        public float z;

        [ProtoMember(259)]
        public float w;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            Quaternion uo = (Quaternion)obj;
            x = uo.x;
            y = uo.y;
            z = uo.z;
            w = uo.w;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            Quaternion uo = (Quaternion)obj;
            uo.x = x;
            uo.y = y;
            uo.z = z;
            uo.w = w;
            return uo;
        }

        public static implicit operator Quaternion(PersistentQuaternion<TID> surrogate)
        {
            if(surrogate == null) { return default; }
            return (Quaternion)surrogate.WriteTo(new Quaternion());
        }
        
        public static implicit operator PersistentQuaternion<TID>(Quaternion obj)
        {
            PersistentQuaternion<TID> surrogate = new PersistentQuaternion<TID>();
            surrogate.ReadFrom(obj);
            return surrogate;
        }
    }

    [Obsolete("Use generic version")]
    [ProtoContract]
    public class PersistentQuaternion : PersistentQuaternion<long>
    {
    }
}

