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
    
    [PersistentTemplate("UnityEngine.ParticleSystem+ExternalForcesModule")]
    public partial class PersistentParticleSystemNestedExternalForcesModule_RTSL_Template : PersistentSurrogateTemplate
    {
        public class PersistentParticleSystemNestedBurst<T> : PersistentSurrogateTemplate { }
#if RTSL_COMPILE_TEMPLATES
        //<TEMPLATE_BODY_START>

        [ProtoMember(1)]
        public TID[] m_influences;

        public override object WriteTo(object obj)
        {
            obj = base.WriteTo(obj);
            if (obj == null)
            {
                return null;
            }

            ParticleSystem.ExternalForcesModule o = (ParticleSystem.ExternalForcesModule)obj;
            if (m_influences != null)
            {
                for (int i = 0; i < m_influences.Length; ++i)
                {
                    ParticleSystemForceField forceField = FromID<ParticleSystemForceField>(m_influences[i]);
                    if(forceField != null)
                    {
                        o.AddInfluence(forceField);
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

            ParticleSystem.ExternalForcesModule o = (ParticleSystem.ExternalForcesModule)obj;
            m_influences = new TID[o.influenceCount];
            for(int i = 0; i < m_influences.Length; ++i)
            {
                ParticleSystemForceField forceField = o.GetInfluence(i);
                m_influences[i] = ToID(forceField);
            }
        }

        public override void GetDeps(GetDepsContext<TID> context)
        {
            base.GetDeps(context);
            AddDep(m_influences, context);
        }

        public override void GetDepsFrom(object obj, GetDepsFromContext context)
        {
            base.GetDepsFrom(obj, context);
            if (obj == null)
            {
                return;
            }

            ParticleSystem.ExternalForcesModule o = (ParticleSystem.ExternalForcesModule)obj;
            for (int i = 0; i < o.influenceCount; ++i)
            {
                AddDep(o.GetInfluence(i), context);
            }
        }

        //<TEMPLATE_BODY_END>
#endif
    }
}
