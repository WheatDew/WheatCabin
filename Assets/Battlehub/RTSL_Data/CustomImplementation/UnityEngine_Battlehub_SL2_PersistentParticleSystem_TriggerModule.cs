#if !RTSL_MAINTENANCE
using Battlehub.RTSL;
using ProtoBuf;
using UnityEngine;
using Battlehub;

namespace UnityEngine.Battlehub.SL2
{
    [CustomImplementation]
    public partial class PersistentParticleSystemNestedTriggerModule<TID> 
    {        

        [ProtoMember(1)]
        public TID[] m_colliders;

        public override object WriteTo(object obj)
        {
            obj = base.WriteTo(obj);
            if (obj == null)
            {
                return null;
            }

            ParticleSystem.TriggerModule o = (ParticleSystem.TriggerModule)obj;
            if (m_colliders == null)
            {
                int colliderCount = o.GetColliderCount();
                for (int i = 0; i < colliderCount; ++i)
                {
                    o.SetCollider(i, null);
                }
            }
            else
            {
                int colliderCount = o.GetColliderCount();
                for (int i = 0; i < Mathf.Min(colliderCount, m_colliders.Length); ++i)
                {
                    o.SetCollider(i, FromID<Component>(m_colliders[i]));
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

            ParticleSystem.TriggerModule o = (ParticleSystem.TriggerModule)obj;
            int colliderCount = o.GetColliderCount();
            if (colliderCount > 20)
            {
                Debug.LogWarning("maxPlaneCount is expected to be 6 or at least <= 20");
            }

            m_colliders = new TID[colliderCount];
            for (int i = 0; i < colliderCount; ++i)
            {
                Component collider = o.GetCollider(i);
                m_colliders[i] = ToID(collider);
            }
        }

        public override void GetDeps(GetDepsContext<TID> context)
        {
            base.GetDeps(context);
            AddDep(m_colliders, context);
        }

        public override void GetDepsFrom(object obj, GetDepsFromContext context)
        {
            base.GetDepsFrom(obj, context);
            if (obj == null)
            {
                return;
            }

            ParticleSystem.TriggerModule o = (ParticleSystem.TriggerModule)obj;
            int colliderCount = o.GetColliderCount();
            for (int i = 0; i < colliderCount; ++i)
            {
                AddDep(o.GetCollider(i), context);
            }
        }
    }
}
#endif

