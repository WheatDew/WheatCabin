using ProtoBuf;
using Battlehub.RTSL;
using System;

namespace UnityEngine.Battlehub.SL2
{
    [ProtoContract]
    public partial class PersistentVector4<TID> : PersistentSurrogateBase
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
            Vector4 uo = (Vector4)obj;
            x = uo.x;
            y = uo.y;
            z = uo.z;
            w = uo.w;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            Vector4 uo = (Vector4)obj;
            uo.x = x;
            uo.y = y;
            uo.z = z;
            uo.w = w;
            return uo;
        }

        public static implicit operator Vector4(PersistentVector4<TID>surrogate)
        {
            if(surrogate == null) { return default; }
            return (Vector4)surrogate.WriteTo(new Vector4());
        }

        public static implicit operator PersistentVector4<TID>(Vector4 obj)
        {
            PersistentVector4<TID> surrogate = new PersistentVector4<TID>();
            surrogate.ReadFrom(obj);
            return surrogate;
        }
    }

    [Obsolete("Use generic version")]
    [ProtoContract]
    public class PersistentVector4 : PersistentVector4<long>
    {
    }
}

