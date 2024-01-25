/// <summary>
/// Project : Easy Build System
/// Class : BuildingTerrainCondition.cs
/// Namespace : EasyBuildSystem.Features.Runtime.Buildings.Part.Conditions
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using System.Collections.Generic;

using UnityEngine;

using EasyBuildSystem.Features.Runtime.Buildings.Manager;

namespace EasyBuildSystem.Features.Runtime.Buildings.Part.Conditions
{
    [BuildingCondition("Building Terrain Condition",
        "Check if the Building Part not colliding with trees and can apply some changes on the terrain at placement.\n\n" +
        "You can find more information on the Building Terrain Condition component in the documentation.", 4)]
    public class BuildingTerrainCondition : BuildingCondition
    {
        #region Fields

        class TerrainModificationData
        {
            public int XHeightIndex { get; }

            public int YHeightIndex { get; }

            public int XAlphaIndex { get; }

            public int YAlphaIndex { get; }

            public int XHeightScale { get; }

            public int YHeightScale { get; }

            public int XAlphaScale { get; }

            public int YAlphaScale { get; }

            public int[,] Details { get; }

            public float[,] Heights;

            public float[,,] Clone { get; }

            public float[,,] Original { get; }

            public int Layer { get; }

            public Terrain Terrain { get; }

            public TerrainModificationData(int _xIndex, int _yIndex, int _detailLayer, int[,] _details, Terrain _terrain)
            {
                XHeightIndex = _xIndex;
                YHeightIndex = _yIndex;

                Details = _details;

                Layer = _detailLayer;
                Terrain = _terrain;
            }

            public TerrainModificationData(int _xIndex, int _yIndex, int _xIndex_Alpha, int _yIndex_Alpha,
                float[,] _heights, float[,,] _clone, int xScale_Height, int zScale_Height, int xScale_Alpha, int zScale_Alpha, Terrain _terrain)
            {
                XHeightIndex = _xIndex;
                YHeightIndex = _yIndex;

                XAlphaIndex = _xIndex_Alpha;
                YAlphaIndex = _yIndex_Alpha;

                XHeightScale = xScale_Height;
                YHeightScale = zScale_Height;

                XAlphaScale = xScale_Alpha;
                YAlphaScale = zScale_Alpha;

                Heights = _heights;
                Clone = _clone;

                if (Clone != null)
                {
                    Original = (float[,,])Clone.Clone();
                }

                Terrain = _terrain;
            }
        }

        [SerializeField] bool m_CheckTreesCollision = true;
        [SerializeField] float m_CheckTreesCollisionDistance = 3;

        [SerializeField] bool m_ClearGrassDetails = true;

        [SerializeField] Vector3 m_ClearGrassBounds = new Vector3(1500f, 1f, 1500f);

        [SerializeField] bool m_ShowDebugs = false;
        [SerializeField] bool m_ShowGizmos = true;

        readonly List<TerrainModificationData> m_SavedData = new List<TerrainModificationData>();

        #endregion

        #region Unity Methods

        void Start()
        {
            if (GetBuildingPart != null)
            {
                if (GetBuildingPart.State == BuildingPart.StateType.PLACED)
                {
                    HandleTerrainModifications();
                }
            }
        }

        void OnDrawGizmosSelected()
        {
            if (!m_ShowGizmos)
            {
                return;
            }

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, transform.position + new Vector3(0, -m_ClearGrassBounds.y, 0));
            Gizmos.DrawWireCube(transform.position, new Vector3(m_ClearGrassBounds.x, 0, m_ClearGrassBounds.z) / 600f);

            Gizmos.color = !CheckPlacingCondition() ? Color.red : Color.cyan;
            Gizmos.DrawWireSphere(transform.position, m_CheckTreesCollisionDistance);
        }

        #endregion

        #region Internal Methods

        public override bool CheckPlacingCondition()
        {
            if (m_CheckTreesCollision)
            {
                RaycastHit[] hits = Physics.RaycastAll(new Ray(transform.position + Vector3.up, Vector3.down), m_ClearGrassBounds.y * 1.1f);

                if (hits.Length == 0)
                {
                    return true;
                }

                for (int hitsIndex = 0; hitsIndex < hits.Length; hitsIndex++)
                {
                    RaycastHit hit = hits[hitsIndex];

                    Terrain terrain = hit.transform.GetComponent<Terrain>();

                    if (terrain != null)
                    {
                        for (int i = 0; i < terrain.terrainData.treeInstances.Length; i++)
                        {
                            Vector3 treePosition =
                                Vector3.Scale(terrain.terrainData.treeInstances[i].position, terrain.terrainData.size) + terrain.transform.position;

                            if (Vector3.Distance(transform.position, treePosition) <= m_CheckTreesCollisionDistance)
                            {
                                if (!Application.isPlaying)
                                {
                                    Debug.LogWarning("<b>Easy Build System</b> : Can't be placed cause: too close to trees.");
                                }
                                else
                                {
                                    if (m_ShowDebugs)
                                    {
                                        Debug.LogWarning("<b>Easy Build System</b> : Can't be placed cause: too close to trees.");
                                    }
                                }

                                return false;
                            }
                        }
                    }
                }
            }

            return true;
        }

        void HandleTerrainModifications()
        {
            if (TerrainManager.Instance == null)
            {
                return;
            }

            RaycastHit[] hits = Physics.RaycastAll(new Ray(transform.position + Vector3.up, Vector3.down), m_ClearGrassBounds.y);

            for (int hitsIndex = 0; hitsIndex < hits.Length; hitsIndex++)
            {
                RaycastHit hit = hits[hitsIndex];

                Terrain terrain = hit.transform.GetComponent<Terrain>();

                if (terrain != null)
                {
                    if (m_ClearGrassDetails)
                    {
                        ClearGrassDetails(terrain);
                    }

                    break;
                }
            }
        }

        void ClearGrassDetails(Terrain terrain)
        {
            TerrainData terrainData = terrain.terrainData;

            Vector3 multiplyScale = transform.lossyScale * 2;

            int detailResolution = terrainData.detailResolution > 1024 ? Mathf.FloorToInt(terrainData.detailResolution / 600) : 0;

            int actualXScale = Mathf.CeilToInt((m_ClearGrassBounds.x * multiplyScale.x) / terrainData.size.x) + detailResolution;
            int actualZScale = Mathf.CeilToInt((m_ClearGrassBounds.z * multiplyScale.z) / terrainData.size.z) + detailResolution;

            Vector3 terrainPoint = transform.position - (multiplyScale / 1.5f);

            Vector3 terrainLocalPosition = terrainPoint - terrain.transform.position;

            Vector3 normalizedPos = new Vector3(Mathf.InverseLerp(0, terrainData.size.x, terrainLocalPosition.x),
                                                Mathf.InverseLerp(0, terrainData.size.y, terrainLocalPosition.y),
                                                Mathf.InverseLerp(0, terrainData.size.z, terrainLocalPosition.z));

            int xBase = (int)(normalizedPos.x * terrainData.detailResolution);
            int zBase = (int)(normalizedPos.z * terrainData.detailResolution);

            for (int detailIndex = 0; detailIndex < terrainData.detailPrototypes.Length; detailIndex++)
            {
                TerrainManager.Instance.AddDetailsModifications(terrain,
                    terrainData.GetDetailLayer(0, 0, terrainData.detailWidth, terrainData.detailHeight, detailIndex), detailIndex);

                m_SavedData.Add(new TerrainModificationData(xBase, zBase, detailIndex,
                    terrainData.GetDetailLayer(xBase, zBase, actualXScale, actualZScale, detailIndex), terrain));
            }

            int[,] _details;

            TerrainModificationData data;

            for (int i = 0; i < m_SavedData.Count; i++)
            {
                data = m_SavedData[i];

                _details = (int[,])data.Details.Clone();

                for (int x = 0; x < actualXScale; x++)
                {
                    for (int z = 0; z < actualZScale; z++)
                    {
                        _details[x, z] = 0;
                    }
                }

                terrainData.SetDetailLayer(data.XHeightIndex, data.YHeightIndex, data.Layer, _details);
            }
        }

        #endregion
    }
}
