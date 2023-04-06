//#define RTSL_COMPILE_TEMPLATES
#if RTSL_COMPILE_TEMPLATES
//<TEMPLATE_USINGS_START>
using System;
using ProtoBuf;
using UnityEngine;
//<TEMPLATE_USINGS_END>
#endif

namespace Battlehub.RTSL.Internal
{
    [PersistentTemplate("UnityEngine.AudioClip")]
    public class PersistentAudioClip_RTSL_Template : PersistentSurrogateTemplate
    {
#if RTSL_COMPILE_TEMPLATES
        //<TEMPLATE_BODY_START>
        [ProtoMember(1)]
        public float[] m_data;

        [ProtoMember(2)]
        public string m_name;

        [ProtoMember(3)]
        public int m_lengthSamples;

        [ProtoMember(4)]
        public int m_channels;

        [ProtoMember(5)]
        public int m_frequency;

        public override object WriteTo(object obj)
        {
            if (obj == null)
            {
                return null;
            }

            AudioClip o = (AudioClip)obj;
            if (!m_assetDB.IsStaticResourceID(m_assetDB.ToID(o)))
            {   
                o.SetData(m_data, 0);
                if (!o.preloadAudioData)
                {
                    o.LoadAudioData();
                }
            }

            return base.WriteTo(obj);
        }

        public override void ReadFrom(object obj)
        {
            base.ReadFrom(obj);
            if (obj == null)
            {
                return;
            }

            AudioClip o = (AudioClip)obj;
            if (!m_assetDB.IsStaticResourceID(m_assetDB.ToID(o)))
            {
                m_name = o.name;
                m_lengthSamples = o.samples;
                m_channels = o.channels;
                m_frequency = o.frequency;

                m_data = new float[o.samples * o.channels];
                o.GetData(m_data, 0);
            }
        }

        public override bool CanInstantiate(Type type)
        {
            return type == typeof(AudioClip);
        }

        public override object Instantiate(Type type)
        {
            return AudioClip.Create(m_name, m_lengthSamples, m_channels, m_frequency, false);
        }

        //<TEMPLATE_BODY_END>
#endif
    }
}


