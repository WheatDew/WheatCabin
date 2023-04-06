using UnityEngine;

namespace Battlehub.RTTerrain
{
    public class Brush 
    {
        public enum BlendFunction
        {
            Set,
            Add,
            Clamp,
            Smooth,
        }

        private BlendFunction m_blend;
        public BlendFunction Blend
        {
            get { return m_blend; }
            set
            {
                m_blend = value;
                switch (Blend)
                {
                    case BlendFunction.Set: m_blender = BlenderSet; break;
                    case BlendFunction.Add: m_blender = BlenderAdd; break;
                    case BlendFunction.Clamp: m_blender = BlenderClamp; break;
                    case BlendFunction.Smooth: m_blender = BlenderSmooth; break;
                }
            }
        }

        public bool IsPainting
        {
            get;
            private set;
        }

        public float Radius = 10.0f;
        public float Min = 0.0f;
        public float Max = 100.0f;
        public Terrain Terrain;
        public Texture2D Texture;

        protected delegate float Blender(float height, float value);
        protected Blender m_blender;

        protected float m_clampMin;
        protected float m_clampMax;

        protected virtual Vector2 Scale
        {
            get
            {
                if(Terrain == null || Terrain.terrainData == null)
                {
                    return Vector2.one;
                }

                TerrainData terrainData = Terrain.terrainData;
                return new Vector2(terrainData.heightmapScale.x, terrainData.heightmapScale.z);
            }
        }

        protected Vector2 Clamp(Vector2 v, Vector2 min, Vector2 max)
        {
            return new Vector2(Mathf.Clamp(v.x, min.x, max.x), Mathf.Clamp(v.y, min.y, max.y));
        }

        protected Vector2Int Floor(Vector2 v)
        {
            return new Vector2Int(Mathf.FloorToInt(v.x), Mathf.FloorToInt(v.y));
        }

        protected Vector2Int Ceil(Vector2 v)
        {
            return new Vector2Int(Mathf.CeilToInt(v.x), Mathf.CeilToInt(v.y));
        }

        protected virtual float Eval(float u, float v)
        {
            if(Texture == null)
            {
                return 0;
            }
            return Texture.GetPixelBilinear(u, v).a;
        }

        protected float BlenderSet(float height, float value)
        {
            return value;
        }

        protected float BlenderAdd(float height, float value)
        {
            return height + value;
        }

        protected float BlenderClamp(float height, float value)
        {
            if(height + value > m_clampMax)
            {
                float newHeight = height - value;
                if(newHeight < m_clampMax)
                {
                    return m_clampMax;
                }
                return newHeight;
            }
                
            return Mathf.Clamp(height + value, m_clampMin, m_clampMax);
        }

        protected float BlenderSmooth(float height, float value)
        {
            return 0.0f;
        }

        public virtual void BeginPaint()
        {
            IsPainting = true;
        }

        public virtual void EndPaint()
        {
            IsPainting = false;
        }

        public virtual void Paint(Vector3 pos, float value, float opacity)
        {
            var data = Terrain.terrainData;

            m_clampMin = Min / data.heightmapScale.y;
            m_clampMax = Max / data.heightmapScale.y;

            // Calculate bounds
            Vector2 center = new Vector2(pos.x / Scale.x, pos.z / Scale.y);
            Vector2 radius_ext = new Vector2(Radius / Scale.x, Radius / Scale.y);

            Vector2Int minPos = Floor(center - radius_ext);
            Vector2Int maxPos = Ceil(center + radius_ext);

            if (Blend != BlendFunction.Smooth)
            {
                Modify(minPos, maxPos, value, opacity);
            }
            else
            {
                Smooth(minPos, maxPos, value, opacity);
            }
        }

        public virtual void Modify(Vector2Int minPos, Vector2Int maxPos, float value, float opacity)
        {
           
        }

        public virtual void Smooth(Vector2Int minPos, Vector2Int maxPos, float value, float opacity)
        {
          
        }
    }
}
