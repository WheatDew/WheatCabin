using Battlehub.RTCommon;
using UnityEngine;

namespace Battlehub.RTTerrain
{
    public class TerrainDetailsBrush : Brush
    {
        public int TerrainDetailIndex
        {
            get;
            set;
        }

        protected override Vector2 Scale
        {
            get
            {
                if (Terrain == null || Terrain.terrainData == null)
                {
                    return Vector2.one;
                }

                TerrainData terrainData = Terrain.terrainData;
                return new Vector2(terrainData.size.x / terrainData.detailWidth, terrainData.size.z / terrainData.detailHeight);
            }
        }

        private int[,] m_oldDetails;
        private IRTE m_editor;
        private bool m_firstModification;
        private float m_nextModificationTime;
        private Vector2Int m_minPos;
        private Vector2Int m_maxPos;

        public TerrainDetailsBrush()
        {
            m_editor = IOC.Resolve<IRTE>();
            Blend = BlendFunction.Add;
        }

        public override void BeginPaint()
        {
            base.BeginPaint();
            m_oldDetails = GetDetails();
            m_firstModification = true;
        }

        public override void EndPaint()
        {
            base.EndPaint();

            Terrain terrain = Terrain;
            
            int[,] oldDetails = m_oldDetails;
            int[,] newDetails = GetDetails();
            int layerIndex = TerrainDetailIndex;
            m_oldDetails = null;

            m_editor.Undo.CreateRecord(record =>
            {
                if (terrain.terrainData != null)
                {
                    terrain.SetDetails(0, 0, layerIndex, newDetails);
                }

                return true;
            },
            record =>
            {
                if (terrain.terrainData != null)
                {
                    terrain.SetDetails(0, 0, layerIndex, oldDetails);
                }

                return true;
            });
        }

        private int[,] GetDetails()
        {
            int w = Terrain.terrainData.detailWidth;
            int h = Terrain.terrainData.detailHeight;
            return Terrain.terrainData.GetDetailLayer(0, 0, w, h, TerrainDetailIndex);
        }

        public override void Modify(Vector2Int minPos, Vector2Int maxPos, float value, float opacity)
        {
            if (TerrainDetailIndex < 0)
            {
                return;
            }

            if (value > 0)
            {
                if (m_firstModification)
                {
                    m_nextModificationTime = Time.time + 0.3f;
                    m_firstModification = false;
                    m_minPos = minPos;
                    m_maxPos = maxPos;
                }
                else
                {
                    if (Time.time < m_nextModificationTime && m_minPos == minPos && m_maxPos == maxPos)
                    {
                        return;
                    }
                }
            }
           

            int px = Mathf.Max(0, minPos.x);
            int py = Mathf.Max(0, minPos.y);
            int[,] details = Terrain.terrainData.GetDetailLayer(
                px,
                py,
                Mathf.Min(Terrain.terrainData.detailWidth, maxPos.x) - px,
                Mathf.Min(Terrain.terrainData.detailHeight, maxPos.y) - py,
                TerrainDetailIndex);

            int sizeY = maxPos.y - minPos.y;
            int sizeX = maxPos.x - minPos.x;

            int dY = details.GetLength(0);
            int dX = details.GetLength(1);

            if (minPos.x > 0)
            {
                minPos.x = 0;
            }
            if (minPos.y > 0)
            {
                minPos.y = 0;
            }

            bool any = false;
            for (int y = 0; y < dY; y++)
            {
                for (int x = 0; x < dX; x++)
                {
                    float u = (x - minPos.x) / (float)(sizeX - 1);
                    float v = (y - minPos.y) / (float)(sizeY - 1);
                    float f = Eval(u, v);

                    if(f > 0.5f && Random.value < opacity)
                    {
                        if (value > 0)
                        {
                            details[y, x] = 1;
                        }
                        else
                        {
                            details[y, x] = 0;
                        }

                        any = true;
                    }
                }
            }


            if (!any)
            {
                details[dY / 2, dX / 2] = value > 0 ? 1 : 0;
            }
            

            Terrain.SetDetails(px, py, TerrainDetailIndex, details);
        }
    }
}


