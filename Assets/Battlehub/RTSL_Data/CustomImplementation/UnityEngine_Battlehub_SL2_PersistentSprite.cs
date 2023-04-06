#if !RTSL_MAINTENANCE
using Battlehub.RTSL;
using ProtoBuf;
using System;
using Battlehub.Utils;
using UnityEngine;
using UnityEngine.Battlehub.SL2;

namespace UnityEngine.Battlehub.SL2
{
    [CustomImplementation]
    public partial class PersistentSprite<TID> 
    {        

          //[ProtoMember(1)]
        //public PersistentTexture2D<TID> m_texture;

        [ProtoMember(2)]
        public Vector2 m_position;

        [ProtoMember(3)]
        public Vector2 m_size;

        [ProtoMember(4)]
        public Vector2 m_pivot;

        [ProtoMember(5)]
        public TID m_texture;

        [ProtoMember(6)]
        public PersistentTexture2D<TID> m_persistentTexture;

        public override void ReadFrom(object obj)
        {
            base.ReadFrom(obj);

            Sprite o = (Sprite)obj;
            if (o == null)
            {
                return;
            }
            m_position = o.rect.position;
            m_size = o.rect.size;
            m_pivot = o.pivot;

            if (m_assetDB.IsMapped(o.texture))
            {
                m_texture = ToID(o.texture);
            }
            else
            {
                m_texture = m_assetDB.NullID;
                m_persistentTexture = new PersistentTexture2D<TID>();
                m_persistentTexture.ReadFrom(o.texture);
            }
        }

        public override void GetDeps(GetDepsContext<TID> context)
        {
            base.GetDeps(context);

            if (m_persistentTexture != null)
            {
                m_persistentTexture.GetDeps(context);
            }
            else
            {
                AddDep(m_texture, context);
            }
        }

        public override void GetDepsFrom(object obj, GetDepsFromContext context)
        {
            base.GetDepsFrom(obj, context);

            Sprite o = (Sprite)obj;
            if (o != null && o.texture != null)
            {
                if (m_assetDB.IsMapped(o.texture))
                {
                    AddDep(o.texture, context);
                }
                else
                {
                    PersistentTexture2D<TID> persistentTexture2D = new PersistentTexture2D<TID>();
                    persistentTexture2D.GetDepsFrom(o.texture, context);
                }
            }
        }

        public override bool CanInstantiate(Type type)
        {
            return true;
        }

        public override object Instantiate(Type type)
        {
            Texture2D texture;
            if (m_persistentTexture != null)
            {
                texture = (Texture2D)m_persistentTexture.Instantiate(typeof(Texture2D));
                m_persistentTexture.WriteTo(texture);
            }
            else
            {
                texture = FromID<Texture2D>(m_texture);
            }

            if (texture != null)
            {
                return Sprite.Create(texture, new Rect(m_position, m_size), m_pivot);
            }

            return null;
        }
    }
}
#endif

