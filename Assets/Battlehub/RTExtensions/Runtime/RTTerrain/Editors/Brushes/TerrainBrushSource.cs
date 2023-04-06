using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Battlehub.RTTerrain
{
    [DefaultExecutionOrder(-100)]
    public class TerrainBrushSource : MonoBehaviour
    {
        private Sprite[] m_builtInBrushes;

        private List<Sprite> m_userBrushes = new List<Sprite>();
        public List<Sprite> UserBrushes
        {
            get { return m_userBrushes; }
            set { m_userBrushes = value; }
        }

        public Texture2D[] UserTextures
        {
            get { return m_userBrushes.Select(b => b.texture).ToArray(); }
            set
            {
                if(value != null)
                {
                    m_userBrushes.Clear();
                    for(int i = 0; i < value.Length; ++i)
                    {
                        Texture2D texture = value[i];
                        if(texture != null)
                        {
                            texture.wrapMode = TextureWrapMode.Clamp;
                            Sprite brush = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                            m_userBrushes.Add(brush);
                        }
                    }
                }
            }
        }

        public IEnumerable<Sprite> Brushes
        {
            get { return m_builtInBrushes.Union(m_userBrushes); }
        }

        private void Awake()
        {
            Sprite[] brushes = Resources.LoadAll<Sprite>("Textures/Brushes");
            m_builtInBrushes = brushes;
        }

        public bool IsBuiltInBrush(Sprite brush)
        {
            return System.Array.IndexOf(m_builtInBrushes, brush) >= 0;
        }
    }
}

