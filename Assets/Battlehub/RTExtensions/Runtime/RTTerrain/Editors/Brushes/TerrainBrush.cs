using Battlehub.RTCommon;
using UnityEngine;

namespace Battlehub.RTTerrain
{
    public class TerrainBrush : Brush
    {
        private IRTE m_editor;
        private float[,] m_oldHeightmap;
        public bool AllowNegativeValue
        {
            get;
            set;
        }

        public bool InvertValue
        {
            get;
            set;
        }

        protected IRTE Editor
        {
            get { return m_editor; }
        }

        public TerrainBrush()
        { 
            AllowNegativeValue = true;
            Blend = BlendFunction.Add;
            m_editor = IOC.Resolve<IRTE>();
        }

        public override void BeginPaint()
        {
            base.BeginPaint();
            m_oldHeightmap = GetHeightmap();
        }

        public override void Paint(Vector3 pos, float value, float opacity)
        {
            if(Terrain == null)
            {
                Debug.LogWarning("Terrain is null");
                return;
            }

            if(!AllowNegativeValue && value < 0)
            {
                return;
            }
            if(InvertValue)
            {
                value = -value;
            }

            base.Paint(pos, ClampValue(value), opacity);
        }

        protected virtual float ClampValue(float value)
        {
            value /= Terrain.terrainData.size.y * 10f;
            return value;
        }

        public override void EndPaint()
        {
            base.EndPaint();
            
            Terrain terrain = Terrain;
            terrain.TerrainColliderWithoutHoles();

            float[,] oldHeightmap = m_oldHeightmap;
            float[,] newHeightmap = GetHeightmap();
            m_oldHeightmap = null;

            m_editor.Undo.CreateRecord(record =>
            {
                if(terrain.terrainData != null)
                {
                    terrain.SetHeights(0, 0, newHeightmap);
                    terrain.TerrainColliderWithoutHoles();
                }
                return true;
            },
            record =>
            {
                if(terrain.terrainData != null)
                {
                    terrain.SetHeights(0, 0, oldHeightmap);
                    terrain.TerrainColliderWithoutHoles();
                }
                return true;
            });
        }

        private float[,] GetHeightmap()
        {
            int w = Terrain.terrainData.heightmapResolution;
            int h = Terrain.terrainData.heightmapResolution;
            return Terrain.terrainData.GetHeights(0, 0, w, h);
        }

        public override void Modify(Vector2Int minPos, Vector2Int maxPos, float value, float opacity)
        {
            float heightMapResoulution = Terrain.terrainData.heightmapResolution;
            int px = Mathf.Max(0, minPos.x);
            int py = Mathf.Max(0, minPos.y);
            float[,] hmap = Terrain.terrainData.GetHeights(
                px,
                py,
                Mathf.Min((int)heightMapResoulution, maxPos.x) - px,
                Mathf.Min((int)heightMapResoulution, maxPos.y) - py);

            int sizeY = maxPos.y - minPos.y;
            int sizeX = maxPos.x - minPos.x;

            int hmapY = hmap.GetLength(0);
            int hmapX = hmap.GetLength(1);

            if(minPos.x > 0)
            {
                minPos.x = 0;
            }
            if(minPos.y > 0)
            {
                minPos.y = 0;
            }

            for (int y = 0; y < hmapY; y++)
            {
                for (int x = 0; x < hmapX; x++)
                {
                    float u = (x - minPos.x) / (float)(sizeX - 1);
                    float v = (y - minPos.y) / (float)(sizeY - 1);
                    float f = Eval(u, v);
                    hmap[y, x] = m_blender(hmap[y, x], f * value * opacity);
                }
            }

            Terrain.SetHeights(px, py, hmap);
        }

        public override void Smooth(Vector2Int minPos, Vector2Int maxPos, float value, float opacity)
        {
            value *= 25;
            float heightMapResoulution = Terrain.terrainData.heightmapResolution;
            int px = Mathf.Max(0, minPos.x);
            int py = Mathf.Max(0, minPos.y);
            float[,] hmap = Terrain.terrainData.GetHeights(
                px,
                py,
                Mathf.Min((int)heightMapResoulution, maxPos.x) - px,
                Mathf.Min((int)heightMapResoulution, maxPos.y) - py);

            int sizeY = maxPos.y - minPos.y;
            int sizeX = maxPos.x - minPos.x;

            int hmapY = hmap.GetLength(0);
            int hmapX = hmap.GetLength(1);

            if (minPos.x > 0)
            {
                minPos.x = 0;
            }
            if (minPos.y > 0)
            {
                minPos.y = 0;
            }

            for (int y = 0; y < hmapY; y++)
            {
                for (int x = 0; x < hmapX; x++)
                {
                    float u = (x - minPos.x) / (float)(sizeX - 1);
                    float v = (y - minPos.y) / (float)(sizeY - 1);

                    float s = (hmap[Mathf.Max(y - 1, 0), x] + hmap[Mathf.Min(y + 1, hmapY - 1), x] + hmap[y, Mathf.Max(x - 1, 0)] + hmap[y, Mathf.Min(x + 1, hmapX - 1)]) * 0.25f;
                    float f = Eval(u, v);
                    hmap[y, x] += f * (s - hmap[y, x]) * value * opacity;
                }
            }

            Terrain.SetHeights(px, py, hmap);
        }    
    }
}

