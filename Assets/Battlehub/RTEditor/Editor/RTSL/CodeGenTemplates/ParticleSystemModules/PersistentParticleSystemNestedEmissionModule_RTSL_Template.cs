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
    [PersistentTemplate("UnityEngine.ParticleSystem+EmissionModule", new string[0],
        new[] { "UnityEngine.ParticleSystem+Burst" })]
    public partial class PersistentParticleSystemNestedEmissionModule_RTSL_Template : PersistentSurrogateTemplate
    {
        public class PersistentParticleSystemNestedBurst<T> : PersistentSurrogateTemplate {}
#if RTSL_COMPILE_TEMPLATES
        //<TEMPLATE_BODY_START>

        [ProtoMember(1)]
        public PersistentParticleSystemNestedBurst<TID>[] m_bursts;

        public override object WriteTo(object obj)
        {
            obj = base.WriteTo(obj);
            if (obj == null)
            {
                return null;
            }

            ParticleSystem.EmissionModule o = (ParticleSystem.EmissionModule)obj;       
            
            if(m_bursts != null)
            {
                ParticleSystem.Burst[] bursts = new ParticleSystem.Burst[m_bursts.Length];
                for(int i = 0; i < m_bursts.Length; ++i)
                {
                    if(m_bursts[i] != null)
                    {
                        bursts[i] = (ParticleSystem.Burst)m_bursts[i].WriteTo(bursts[i]);
                    }
                }

                o.SetBursts(bursts);
            }
            else
            {
                o.SetBursts(new ParticleSystem.Burst[0]);
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

            ParticleSystem.EmissionModule o = (ParticleSystem.EmissionModule)obj;
            m_bursts = new PersistentParticleSystemNestedBurst<TID>[o.burstCount];

            ParticleSystem.Burst[] bursts = new ParticleSystem.Burst[o.burstCount];
            o.GetBursts(bursts);
            
            for(int i = 0; i < bursts.Length; ++i)
            {
                m_bursts[i] = new PersistentParticleSystemNestedBurst<TID>();
                m_bursts[i].ReadFrom(bursts[i]);
            }
        }

        public override void GetDeps(GetDepsContext<TID> context)
        {
            base.GetDeps(context);

            AddSurrogateDeps(m_bursts, v_ => (PersistentParticleSystemNestedBurst<TID>)v_, context);
        }

        public override void GetDepsFrom(object obj, GetDepsFromContext context)
        {
            base.GetDepsFrom(obj, context);
            ParticleSystem.EmissionModule o = (ParticleSystem.EmissionModule)obj;

            ParticleSystem.Burst[] bursts = new ParticleSystem.Burst[o.burstCount];
            o.GetBursts(bursts);
            AddSurrogateDeps(bursts, v_ => (PersistentParticleSystemNestedBurst<TID>)v_, context);
        }

        //<TEMPLATE_BODY_END>
#endif
    }
}
