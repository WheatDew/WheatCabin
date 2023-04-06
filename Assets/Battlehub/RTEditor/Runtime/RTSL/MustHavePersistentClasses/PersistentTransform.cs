using ProtoBuf;
using System;

namespace UnityEngine.Battlehub.SL2
{
    [ProtoContract]
    public class PersistentTransform<TID> : PersistentComponent<TID>
    {
        [ProtoMember(256)]
        public Vector3 position;

        [ProtoMember(263)]
        public Quaternion rotation;

        [ProtoMember(265)]
        public Vector3 localScale;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            Transform uo = (Transform)obj;
            position = uo.localPosition;
            rotation = uo.localRotation;
            localScale = uo.localScale;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            Transform uo = (Transform)obj;
            uo.localPosition = position;
            uo.localRotation = rotation;
            uo.localScale = localScale;
            return obj;
        }

    
        public static implicit operator PersistentTransform<TID>(Transform obj)
        {
            PersistentTransform<TID> surrogate = new PersistentTransform<TID>();
            surrogate.ReadFrom(obj);
            return surrogate;
        }
    }

    [Obsolete("Use generic version")]
    [ProtoContract]
    public class PersistentTransform : PersistentTransform<long>
    {
    }
}

