using ProtoBuf;
using Battlehub.RTSL;
using System;

namespace UnityEngine.Battlehub.SL2
{
    [ProtoContract]
    public partial class PersistentVector3<TID> : PersistentSurrogateBase
    {
        [ProtoMember(256)]
        public float x;

        [ProtoMember(257)]
        public float y;

        [ProtoMember(258)]
        public float z;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            Vector3 uo = (Vector3)obj;
            x = uo.x;
            y = uo.y;
            z = uo.z;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            Vector3 uo = (Vector3)obj;
            uo.x = x;
            uo.y = y;
            uo.z = z;
            return uo;
        }

   
        public static implicit operator Vector3(PersistentVector3<TID> surrogate)
        {
            if (surrogate == null) return default;
            return (Vector3)surrogate.WriteTo(new Vector3());
        }
        
        public static implicit operator PersistentVector3<TID>(Vector3 obj)
        {
            PersistentVector3<TID> surrogate = new PersistentVector3<TID>();
            surrogate.ReadFrom(obj);
            return surrogate;
        }
    }

    [Obsolete("Use generic version")]
    [ProtoContract]
    public class PersistentVector3 : PersistentVector3<long>
    {
    }
}

