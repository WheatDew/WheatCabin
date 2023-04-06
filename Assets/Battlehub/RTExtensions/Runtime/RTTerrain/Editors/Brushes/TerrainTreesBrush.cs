using Battlehub.RTCommon;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Battlehub.RTTerrain
{
    public struct Vector3ToHash
    {
        private int m_hashCode;
        private Vector3 m_v;
    
        public Vector3ToHash(Vector3 v)
        {
            m_v = v;
            m_v.x = Mathf.RoundToInt(v.x);
            m_v.y = Mathf.RoundToInt(v.y);
            m_v.z = Mathf.RoundToInt(v.z);
            m_hashCode = new
            {
                x = m_v.x,
                y = m_v.y,
                z = m_v.z,
            }.GetHashCode();
        }

        public override int GetHashCode()
        {
            return m_hashCode;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            Vector3ToHash other = (Vector3ToHash)obj;
            return Mathf.Approximately(m_v.x, other.m_v.x) && Mathf.Approximately(m_v.y, other.m_v.y) && Mathf.Approximately(m_v.z, other.m_v.z);
        }

    }

    public class TerrainTreesBrush : Brush
    {
        public int TerrainTreeIndex
        {
            get;
            set;
        }

        public float MinHeight
        {
            get;
            set;
        }

        public float MaxHeight
        {
            get;
            set;
        }

        public float MinWidth
        {
            get;
            set;
        }

        public float MaxWidth
        {
            get;
            set;
        }
       
        public bool LockWidthToHeight
        {
            get;
            set;
        }


        protected override Vector2 Scale
        {
            get { return Vector2.one; }
        }

        private TreeInstance[] m_oldInstances;
        private IRTE m_editor;
        private bool m_firstModification;
        private float m_nextModificationTime;
        private Vector3 m_pos;

        public TerrainTreesBrush()
        {
            m_editor = IOC.Resolve<IRTE>();
            Blend = BlendFunction.Add;
            MaxWidth = MinWidth = MaxHeight = MinHeight = 1;
            LockWidthToHeight = true;
        }

        public override void BeginPaint()
        {
            base.BeginPaint();
            m_oldInstances = GetInstances();
            m_firstModification = true;
        }

        public override void EndPaint()
        {
            base.EndPaint();

            Terrain terrain = Terrain;
            
            TreeInstance[] oldInstances = m_oldInstances;
            TreeInstance[] newInstances = GetInstances();
            m_oldInstances = null;

            m_editor.Undo.CreateRecord(record =>
            {
                if (terrain.terrainData != null)
                {
                    terrain.SetTreeInstances(newInstances, false);
                }

                return true;
            },
            record =>
            {
                if (terrain.terrainData != null)
                {
                    terrain.SetTreeInstances(oldInstances, false);
                }

                return true;
            });
        }

        private TreeInstance[] GetInstances()
        {
            return Terrain.terrainData.treeInstances;
        }

        public override void Paint(Vector3 pos, float value, float opacity)
        {
            if (TerrainTreeIndex < 0)
            {
                return;
            }

            var data = Terrain.terrainData;

            Vector2 center = new Vector2(pos.x / Scale.x, pos.z / Scale.y);
            Vector2 radius_ext = new Vector2(Radius / Scale.x, Radius / Scale.y);

            Vector2Int minPos = Floor(center - radius_ext);
            Vector2Int maxPos = Ceil(center + radius_ext);

            if (value > 0)
            {
                if (m_firstModification)
                {
                    m_nextModificationTime = Time.time + 0.3f;
                    m_firstModification = false;
                    m_pos = pos;
                }
                else
                {
                    if (Time.time < m_nextModificationTime && m_pos == pos)
                    {
                        return;
                    }
                }
            }

            Dictionary<Vector3ToHash, TreeInstance> treeInstances = new Dictionary<Vector3ToHash, TreeInstance>();
            foreach(TreeInstance tree in data.treeInstances)
            {
                Vector3ToHash vth = new Vector3ToHash(new Vector3(tree.position.x * data.size.x, 0, tree.position.z * data.size.z));
                if(!treeInstances.ContainsKey(vth))
                {
                    treeInstances.Add(vth, tree);
                }
            }

            int sizeY = maxPos.y - minPos.y;
            int sizeX = maxPos.x - minPos.x;

            if (minPos.x > 0)
            {
                minPos.x = 0;
            }
            if (minPos.y > 0)
            {
                minPos.y = 0;
            }

            bool any = false;
            for (int y = 0; y < sizeY; y++)
            {
                for (int x = 0; x < sizeX; x++)
                {
                    float u = (x - minPos.x) / (float)(sizeX - 1);
                    float v = (y - minPos.y) / (float)(sizeY - 1);
                    float f = Eval(u, v);

                    if (f > 0.5f && Random.value < opacity * 0.05f)
                    {
                        Vector3 position = new Vector3((center.x + (x - minPos.x) - sizeX / 2), 0, (center.y + (y - minPos.y) - sizeY / 2));
                        if (value > 0)
                        {
                            CreateTreeInstance(treeInstances, position, data.size);
                        }
                        else
                        {
                            treeInstances.Remove(new Vector3ToHash(position));
                        }

                        any = true;
                    }
                }
            }

            if (!any)
            {
                Vector3 position = new Vector3(pos.x, 0, pos.z);
                if(value > 0)
                {
                    CreateTreeInstance(treeInstances, position, data.size);
                }
                else
                {
                    treeInstances.Remove(new Vector3ToHash(position));
                }
            }

            Terrain.SetTreeInstances(treeInstances.Values.ToArray(), true);
        }

        private void CreateTreeInstance(Dictionary<Vector3ToHash, TreeInstance> instances, Vector3 position, Vector3 size)
        {
            position.y = 0;

            if(instances.ContainsKey(new Vector3ToHash(position)))
            {
                return;
            }

            TreeInstance instance = new TreeInstance();
            instance.prototypeIndex = TerrainTreeIndex;
            instance.heightScale = Random.Range(MinHeight, MaxHeight);
            instance.widthScale = LockWidthToHeight ? instance.heightScale : Random.Range(MinWidth, MaxWidth);
            instance.color = Color.white;
            instance.lightmapColor = Color.white;
            instance.position = new Vector3(position.x / size.x, 0, position.z / size.z);
            instances.Add(new Vector3ToHash(position), instance);
        }
    }
}


