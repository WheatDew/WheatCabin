//#define RTSL_COMPILE_TEMPLATES
#if RTSL_COMPILE_TEMPLATES
//<TEMPLATE_USINGS_START>
using ProtoBuf;
using UnityEngine;
using TMPro;
//<TEMPLATE_USINGS_END>
#else
using UnityEngine;
#endif

namespace Battlehub.RTSL.Internal
{
    [PersistentTemplate("TMPro.TMP_Text",
        new[] {"color"},
        new[] { "UnityEngine.Color" })]
    public class PersistentTMP_Text_RTSL_Template : PersistentSurrogateTemplate
    {
#if RTSL_COMPILE_TEMPLATES
        //<TEMPLATE_BODY_START>

        [ProtoMember(1)]
        public Color m_color;

        public override void ReadFrom(object obj)
        {
            base.ReadFrom(obj);

            TMP_Text text = (TMP_Text)obj;
            if (text == null)
            {
                return;
            }

            m_color = text.color;
        }

        public override object WriteTo(object obj)
        {
            obj = base.WriteTo(obj);
            TMP_Text text = (TMP_Text)obj;
            if (text == null)
            {
                return null;
            }

            text.color = m_color; 
            return text;
        }

        //<TEMPLATE_BODY_END>
#endif
    }
}


