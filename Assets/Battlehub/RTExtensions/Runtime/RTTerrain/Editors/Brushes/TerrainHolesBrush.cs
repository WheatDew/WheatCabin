using Battlehub.RTCommon;
using UnityEngine;

namespace Battlehub.RTTerrain
{
    public class TerrainHolesBrush : Brush
    {
        private bool[,] m_oldHoles;
        private IRTE m_editor;
        private bool[,] GetHoles()
        {
            int res = Terrain.terrainData.holesResolution;
            return Terrain.terrainData.GetHoles(0, 0, res, res);
        }

        public TerrainHolesBrush()
        {
            m_editor = IOC.Resolve<IRTE>();
        }

        public override void BeginPaint()
        {
            base.BeginPaint();

            Terrain terrain = Terrain;
            terrain.TerrainColliderWithoutHoles();

            m_oldHoles = GetHoles();
        }

        public override void EndPaint()
        {
            base.EndPaint();
            
            Terrain terrain = Terrain;
            terrain.TerrainColliderWithoutHoles();

            bool[,] oldHoles = m_oldHoles;
            bool[,] newHoles = GetHoles();
            m_oldHoles = null;

            m_editor.Undo.CreateRecord(record =>
            {
                if (terrain.terrainData != null)
                {
                    terrain.SetHoles(0, 0, newHoles);
                    terrain.TerrainColliderWithoutHoles();
                }

                return true;
            },
            record =>
            {
                if (terrain.terrainData != null)
                {
                    terrain.SetHoles(0, 0, oldHoles);
                    terrain.TerrainColliderWithoutHoles();
                }

                return true;
            });
        }

        public override void Modify(Vector2Int minPos, Vector2Int maxPos, float value, float opacity)
        {
            int holesResolution = Terrain.terrainData.holesResolution;
            int px = Mathf.Max(0, minPos.x);
            int py = Mathf.Max(0, minPos.y);
            bool[,] holes = Terrain.terrainData.GetHoles(
                px,
                py,
                Mathf.Min(holesResolution, maxPos.x) - px,
                Mathf.Min(holesResolution, maxPos.y) - py);

            int sizeY = maxPos.y - minPos.y;
            int sizeX = maxPos.x - minPos.x;

            int hmapY = holes.GetLength(0);
            int hmapX = holes.GetLength(1);

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
                    float f = Eval(u, v);
                    if(f >= opacity)
                    {
                        holes[y, x] = value < 0;
                    }
                }
            }

            Terrain.SetHoles(px, py, holes);

            
        }
    }

}
