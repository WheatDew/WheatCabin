#if !RTSL_MAINTENANCE
using Battlehub.RTSL;
using ProtoBuf;
using UnityEngine;
using TMPro;

namespace TMPro.Battlehub.SL2
{
    [CustomImplementation]
    public partial class PersistentTMP_Text<TID> 
    {        

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
    }
}
#endif

