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
    [PersistentTemplate("UnityEngine.ParticleSystem+TextureSheetAnimationModule")]
    public partial class PersistentParticleSystemNestedTextureSheetAnimationModule_RTSL_Template : PersistentSurrogateTemplate
    {
#if RTSL_COMPILE_TEMPLATES
        //<TEMPLATE_BODY_START>

        [ProtoMember(1)]
        public TID[] m_sprites;
        
        public override object WriteTo(object obj)
        {
            obj = base.WriteTo(obj);
            if (obj == null)
            {
                return null;
            }

            ParticleSystem.TextureSheetAnimationModule o = (ParticleSystem.TextureSheetAnimationModule)obj;
            if(m_sprites != null)
            {
                for(int i = 0; i < m_sprites.Length; ++i)
                {
                    Sprite sprite = FromID<Sprite>(m_sprites[i]);
                    if(sprite != null)
                    {
                        o.AddSprite(sprite);
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

            ParticleSystem.TextureSheetAnimationModule o = (ParticleSystem.TextureSheetAnimationModule)obj;
            m_sprites = new TID[o.spriteCount];
            
            for(int i = 0; i < o.spriteCount; ++i)
            {
                m_sprites[i] = ToID(o.GetSprite(i));
            }
        }

        public override void GetDeps(GetDepsContext<TID> context)
        {
            base.GetDeps(context);
            AddDep(m_sprites, context);
        }

        public override void GetDepsFrom(object obj, GetDepsFromContext context)
        {
            base.GetDepsFrom(obj, context);
            if (obj == null)
            {
                return;
            }

            ParticleSystem.TextureSheetAnimationModule o = (ParticleSystem.TextureSheetAnimationModule)obj;
            for (int i = 0; i < o.spriteCount; ++i)
            {
                AddDep(o.GetSprite(i), context);
            }
        }

        //<TEMPLATE_BODY_END>
#endif
    }
}
