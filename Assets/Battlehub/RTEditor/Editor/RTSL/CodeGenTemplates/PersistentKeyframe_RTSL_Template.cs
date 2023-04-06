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
    [PersistentTemplate("UnityEngine.Keyframe", new[] { "time", "value" })]
    public class PersistentKeyframe_RTSL_Template : PersistentSurrogateTemplate
    {
#if RTSL_COMPILE_TEMPLATES
        //<TEMPLATE_BODY_START>

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

        //<TEMPLATE_BODY_END>
#endif
    }
}


