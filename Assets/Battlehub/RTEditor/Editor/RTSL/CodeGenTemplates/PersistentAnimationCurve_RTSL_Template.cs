//#define RTSL_COMPILE_TEMPLATES
#if RTSL_COMPILE_TEMPLATES
//<TEMPLATE_USINGS_START>
using ProtoBuf;
using UnityEngine;
//<TEMPLATE_USINGS_END>
#else
using UnityEngine;
#endif

namespace Battlehub.RTSL.Internal
{
    [PersistentTemplate("UnityEngine.AnimationCurve",
        new[] { "keys" }, new[] { "UnityEngine.Keyframe" })]
    public class PersistentAnimationCurve_RTSL_Template : PersistentSurrogateTemplate
    {
#if RTSL_COMPILE_TEMPLATES
        //<TEMPLATE_BODY_START>

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

        //<TEMPLATE_BODY_END>
#endif
    }
}


