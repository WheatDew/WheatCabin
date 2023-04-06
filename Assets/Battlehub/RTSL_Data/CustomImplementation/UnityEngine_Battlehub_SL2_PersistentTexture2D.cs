#if !RTSL_MAINTENANCE
using Battlehub.RTSL;
using ProtoBuf;
using System;
using Battlehub.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace UnityEngine.Battlehub.SL2
{
    [CustomImplementation]
    public partial class PersistentTexture2D<TID> 
    {        

        [ProtoMember(1)]
        public byte[] m_bytes;

        [ProtoMember(2)]
        public bool m_isReadable;

        [ProtoMember(3)]
        public bool m_mipChain;

        public override void ReadFrom(object obj)
        {
            base.ReadFrom(obj);

            Texture2D texture = (Texture2D)obj;
            if (texture == null)
            {
                return;
            }

            if (m_assetDB.IsStaticResourceID(m_assetDB.ToID(texture)))
            {
                return;
            }

            try
            {
                bool supportedFormat = texture.format == TextureFormat.ARGB32 ||
                                        texture.format == TextureFormat.RGBA32 ||
                                        texture.format == TextureFormat.RGB24 ||
                                        texture.format == TextureFormat.Alpha8;

                if (texture.isReadable && supportedFormat)
                {
                    if(texture.format == TextureFormat.RGB24)
                    {
                        m_bytes = texture.EncodeToJPG();
                    }
                    else
                    {
                        m_bytes = texture.EncodeToPNG();
                    }
                }
                else
                {
                    Texture2D decompressed = texture.DeCompress();
                    if (texture.format == TextureFormat.RGB24)
                    {
                        m_bytes = decompressed.EncodeToJPG();
                    }
                    else
                    {
                        m_bytes = decompressed.EncodeToPNG();
                    }
                    UnityEngine.Object.Destroy(decompressed);
                }

                m_isReadable = true;
                m_mipChain = texture.mipmapCount > 1;
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
                m_bytes = new byte[0];
                m_isReadable = false;
            }
        }

        public override object WriteTo(object obj)
        {
            obj = base.WriteTo(obj);
            Texture2D texture = (Texture2D)obj;
            if (texture == null)
            {
                return null;
            }

            if (m_assetDB.IsStaticResourceID(m_assetDB.ToID(texture)))
            {
                return texture;
            }

            if (m_isReadable)
            {
                texture.LoadImage(m_bytes, false);
            }
            return texture;
        }

        public override void GetDeps(GetDepsContext<TID> context)
        {
            base.GetDeps(context);
        }

        public override void GetDepsFrom(object obj, GetDepsFromContext context)
        {
            base.GetDepsFrom(obj, context);
        }

        public override bool CanInstantiate(Type type)
        {
            return true;
        }

        public override object Instantiate(Type type)
        {
            Texture2D texture = new Texture2D(1, 1, TextureFormat.ARGB32, m_mipChain);
            return texture;
        }
    }
}
#endif

