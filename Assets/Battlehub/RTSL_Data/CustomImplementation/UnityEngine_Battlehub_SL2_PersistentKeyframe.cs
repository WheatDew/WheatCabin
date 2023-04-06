#if !RTSL_MAINTENANCE
using Battlehub.RTSL;
using ProtoBuf;
using UnityEngine;

namespace UnityEngine.Battlehub.SL2
{
    [CustomImplementation]
    public partial class PersistentKeyframe<TID> 
    {        

        [ProtoMember(1)]
        public float time;

        [ProtoMember(2)]
        public float value;

        public override void ReadFrom(object obj)
        {
            base.ReadFrom(obj);
            if (obj == null)
            {
                return;
            }

            Keyframe keyframe = (Keyframe)obj;
            time = keyframe.time;
            value = keyframe.value;    
        }

        public override object WriteTo(object obj)
        {
            obj = base.WriteTo(obj);
            if (obj == null)
            {
                return null;
            }
            Keyframe keyframe = (Keyframe)obj;
            keyframe.time = time;
            keyframe.value = value;
            return keyframe;
        }
    }
}
#endif

