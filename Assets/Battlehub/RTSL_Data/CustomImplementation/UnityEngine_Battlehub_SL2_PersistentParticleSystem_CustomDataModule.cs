#if !RTSL_MAINTENANCE
using Battlehub.RTSL;
using ProtoBuf;
using UnityEngine;

namespace UnityEngine.Battlehub.SL2
{
    [CustomImplementation]
    public partial class PersistentParticleSystemNestedCustomDataModule<TID> 
    {        

        [ProtoMember(1)]
        public ParticleSystemCustomDataMode m_mode1;

        [ProtoMember(2)]
        public ParticleSystemCustomDataMode m_mode2;

        [ProtoMember(3)]
        public PersistentParticleSystemNestedMinMaxGradient<TID> m_color1;

        [ProtoMember(4)]
        public PersistentParticleSystemNestedMinMaxGradient<TID> m_color2;

        [ProtoMember(5)]
        public int m_vectorComponentCount1;

        [ProtoMember(6)]
        public int m_vectorComponentCount2;

        [ProtoMember(7)]
        public PersistentParticleSystemNestedMinMaxCurve<TID>[] m_vectors1;

        [ProtoMember(8)]
        public PersistentParticleSystemNestedMinMaxCurve<TID>[] m_vectors2;

        public override object WriteTo(object obj)
        {
            obj = base.WriteTo(obj);
            if (obj == null)
            {
                return null;
            }

            ParticleSystem.CustomDataModule o = (ParticleSystem.CustomDataModule)obj;
            o.SetMode(ParticleSystemCustomData.Custom1, m_mode1);
            o.SetMode(ParticleSystemCustomData.Custom1, m_mode2);

            if (m_mode1 == ParticleSystemCustomDataMode.Color)
            {
                if (m_color1 != null)
                {
                    ParticleSystem.MinMaxGradient grad = new ParticleSystem.MinMaxGradient();
                    m_color1.WriteTo(grad);
                    o.SetColor(ParticleSystemCustomData.Custom1, grad);
                }
            
            }
            else if(m_mode1 == ParticleSystemCustomDataMode.Vector)
            {
                o.SetVectorComponentCount(ParticleSystemCustomData.Custom1, m_vectorComponentCount1);
                if (m_vectors1 != null)
                {
                    for (int i = 0; i < m_vectorComponentCount1; ++i)
                    {
                        if(m_vectors1[i] != null)
                        {
                            ParticleSystem.MinMaxCurve v = new ParticleSystem.MinMaxCurve();
                            m_vectors1[i].WriteTo(v);
                            o.SetVector(ParticleSystemCustomData.Custom1, i, v);
                        }
                    }
                }
                
            }
            
            if(m_mode2 == ParticleSystemCustomDataMode.Color)
            {
                if(m_color2 != null)
                {
                    ParticleSystem.MinMaxGradient grad = new ParticleSystem.MinMaxGradient();
                    m_color2.WriteTo(grad);
                    o.SetColor(ParticleSystemCustomData.Custom2, grad);
                }
                
            }
            else if(m_mode2 == ParticleSystemCustomDataMode.Vector)
            {
                o.SetVectorComponentCount(ParticleSystemCustomData.Custom2, m_vectorComponentCount2);
                if(m_vectors2 != null)
                {
                    for (int i = 0; i < m_vectorComponentCount2; ++i)
                    {
                        if(m_vectors2[i] != null)
                        {
                            ParticleSystem.MinMaxCurve v = new ParticleSystem.MinMaxCurve();
                            m_vectors2[i].WriteTo(v);
                            o.SetVector(ParticleSystemCustomData.Custom2, i, v);
                        }
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

            ParticleSystem.CustomDataModule o = (ParticleSystem.CustomDataModule)obj;
            m_mode1 = o.GetMode(ParticleSystemCustomData.Custom1);
            m_mode2 = o.GetMode(ParticleSystemCustomData.Custom2);


            m_color1 = new PersistentParticleSystemNestedMinMaxGradient<TID>();
            m_color1.ReadFrom(o.GetColor(ParticleSystemCustomData.Custom1));

            m_color2 = new PersistentParticleSystemNestedMinMaxGradient<TID>();
            m_color2.ReadFrom(o.GetColor(ParticleSystemCustomData.Custom2));

            m_vectorComponentCount1 = o.GetVectorComponentCount(ParticleSystemCustomData.Custom1);
            m_vectorComponentCount2 = o.GetVectorComponentCount(ParticleSystemCustomData.Custom2);

            if(m_vectorComponentCount1 > 0)
            {
                m_vectors1 = new PersistentParticleSystemNestedMinMaxCurve<TID>[m_vectorComponentCount1];
                for (int i = 0; i < m_vectors1.Length; ++i)
                {
                    PersistentParticleSystemNestedMinMaxCurve<TID> v = new PersistentParticleSystemNestedMinMaxCurve<TID>();
                    v.ReadFrom(o.GetVector(ParticleSystemCustomData.Custom1, i));
                    m_vectors1[i] = v;
                }

            }

            if(m_vectorComponentCount2 > 0)
            {
                m_vectors2 = new PersistentParticleSystemNestedMinMaxCurve<TID>[m_vectorComponentCount2];
                for (int i = 0; i < m_vectors2.Length; ++i)
                {
                    PersistentParticleSystemNestedMinMaxCurve<TID> v = new PersistentParticleSystemNestedMinMaxCurve<TID>();
                    v.ReadFrom(o.GetVector(ParticleSystemCustomData.Custom2, i));
                    m_vectors2[i] = v;
                }
            }
        }

        public override void GetDeps(GetDepsContext<TID> context)
        {
            base.GetDeps(context);
            AddSurrogateDeps(m_vectors1, v_ => (PersistentParticleSystemNestedMinMaxCurve<TID>)v_, context);
            AddSurrogateDeps(m_vectors2, v_ => (PersistentParticleSystemNestedMinMaxCurve<TID>)v_, context);
        }

        public override void GetDepsFrom(object obj, GetDepsFromContext context)
        {
            base.GetDepsFrom(obj, context);
            ParticleSystem.CustomDataModule o = (ParticleSystem.CustomDataModule)obj;
           
            int count = o.GetVectorComponentCount(ParticleSystemCustomData.Custom1);
            for (int i = 0; i < count; ++i)
            {
                AddSurrogateDeps(o.GetVector(ParticleSystemCustomData.Custom1, i), v_ => (PersistentParticleSystemNestedMinMaxCurve<TID>)v_, context);
            }

            count = o.GetVectorComponentCount(ParticleSystemCustomData.Custom2);
            for (int i = 0; i < count; ++i)
            {
                AddSurrogateDeps(o.GetVector(ParticleSystemCustomData.Custom2, i), v_ => (PersistentParticleSystemNestedMinMaxCurve<TID>)v_, context);
            }
        }
    }
}
#endif

