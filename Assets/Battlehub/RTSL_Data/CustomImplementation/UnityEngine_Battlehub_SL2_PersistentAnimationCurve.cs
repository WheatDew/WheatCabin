#if !RTSL_MAINTENANCE
using Battlehub.RTSL;
using ProtoBuf;
using UnityEngine;

namespace UnityEngine.Battlehub.SL2
{
    [CustomImplementation]
    public partial class PersistentAnimationCurve<TID> 
    {        

        [ProtoMember(1)]
        public Keyframe[] keys;

        public override void ReadFrom(object obj)
        {
            base.ReadFrom(obj);
            
            AnimationCurve property = (AnimationCurve)obj;
            if (property == null)
            {
                return;
            }

            keys = property.keys;
        }

        public override object WriteTo(object obj)
        {
            obj = base.WriteTo(obj);
            AnimationCurve property = (AnimationCurve)obj;
            if (property == null)
            {
                return null;
            }

            if(keys != null)
            {
                property.keys = keys;
            }
            else
            {
                property.keys = new Keyframe[0];
            }
            
            return property;
        }
    }
}
#endif

