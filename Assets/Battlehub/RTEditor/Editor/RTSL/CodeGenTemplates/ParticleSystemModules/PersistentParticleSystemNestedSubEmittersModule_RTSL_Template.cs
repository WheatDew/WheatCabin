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
    [PersistentTemplate("UnityEngine.ParticleSystem+SubEmittersModule")]
    public partial class PersistentParticleSystemNestedSubEmittersModule_RTSL_Template : PersistentSurrogateTemplate
    {
#if RTSL_COMPILE_TEMPLATES
        //<TEMPLATE_BODY_START>

        [ProtoMember(1)]
        public float[] m_subEmitProbability;

        [ProtoMember(2)]
        public ParticleSystemSubEmitterProperties[] m_subProperties;

        [ProtoMember(3)]
        public ParticleSystemSubEmitterType[] m_subTypes;

        [ProtoMember(4)]
        public TID[] m_subSystems;
        
        public override object WriteTo(object obj)
        {
            obj = base.WriteTo(obj);
            if (obj == null)
            {
                return null;
            }

            ParticleSystem.SubEmittersModule o = (ParticleSystem.SubEmittersModule)obj;
            if(m_subSystems != null)
            {
                for(int i = 0; i < m_subSystems.Length; ++i)
                {
                    ParticleSystem ps = FromID<ParticleSystem>(m_subSystems[i]);
                    if(ps != null)
                    {
                        o.AddSubEmitter(ps, m_subTypes[i], m_subProperties[i], m_subEmitProbability[i]);
                    }
                }
            }

            return obj;
        }

        public override void ReadFrom(object obj)
        {
            base.ReadFrom(obj);
            if (obj == null)
            {
                return;
            }

            ParticleSystem.SubEmittersModule o = (ParticleSystem.SubEmittersModule)obj;
            m_subSystems = new TID[o.subEmittersCount];
            m_subEmitProbability = new float[o.subEmittersCount];
            m_subProperties = new ParticleSystemSubEmitterProperties[o.subEmittersCount];
            m_subTypes = new ParticleSystemSubEmitterType[o.subEmittersCount];
            for(int i = 0; i < o.subEmittersCount; ++i)
            {
                m_subSystems[i] = ToID(o.GetSubEmitterSystem(i));
                m_subEmitProbability[i] = o.GetSubEmitterEmitProbability(i);
                m_subProperties[i] = o.GetSubEmitterProperties(i);
                m_subTypes[i] = o.GetSubEmitterType(i);
            }
        }

        public override void GetDeps(GetDepsContext<TID> context)
        {
            base.GetDeps(context);
            AddDep(m_subSystems, context);
        }

        public override void GetDepsFrom(object obj, GetDepsFromContext context)
        {
            base.GetDepsFrom(obj, context);
            if (obj == null)
            {
                return;
            }

            ParticleSystem.SubEmittersModule o = (ParticleSystem.SubEmittersModule)obj;
            for (int i = 0; i < o.subEmittersCount; ++i)
            {
                AddDep(o.GetSubEmitterSystem(i), context);
            }
        }

        //<TEMPLATE_BODY_END>
#endif
    }
}
