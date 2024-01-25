/// <summary>
/// Project : Easy Build System
/// Class : TerrainManager.cs
/// Namespace : EasyBuildSystem.Features.Runtime.Buildings.Manager
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using System.Collections.Generic;

using UnityEngine;

namespace EasyBuildSystem.Features.Runtime.Buildings.Manager
{
    public class TerrainManager : MonoBehaviour
    {
        #region Fields

        static TerrainManager m_Instance;
        public static TerrainManager Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    m_Instance = new GameObject("(Instance) TerrainManager").AddComponent<TerrainManager>();
                }

                return m_Instance;
            }
        }

        class RestoreData
        {
            public Dictionary<int, int[,]> Details = new Dictionary<int, int[,]>();

            public float[,] Heights { get; set; }

            public float[,,] Clone { get; set; }
        }

        readonly Dictionary<Terrain, TerrainData> m_DefaultData = new Dictionary<Terrain, TerrainData>();
        readonly Dictionary<Terrain, RestoreData> m_RestoreData = new Dictionary<Terrain, RestoreData>();

        #endregion

        #region Unity Methods

        void Awake()
        {
            Terrain[] terrains = FindObjectsOfType<Terrain>();

            for (int i = 0; i < terrains.Length; i++)
            {
                if (!m_DefaultData.ContainsKey(terrains[i]))
                {
                    if (terrains[i].terrainData != null)
                    {
                        m_DefaultData.Add(terrains[i], Instantiate(terrains[i].terrainData));
                    }
                }
            }

            DontDestroyOnLoad(gameObject);
        }

        void OnApplicationQuit()
        {
            RevertModifications();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Add heights terrain modifications.
        /// </summary>
        public void AddHeightsModifications(Terrain terrain, float[,] heights, float[,,] maps)
        {
            RestoreData currentData;

            if (!m_RestoreData.ContainsKey(terrain))
            {
                currentData = new RestoreData();
                m_RestoreData.Add(terrain, currentData);
            }

            currentData = m_RestoreData[terrain];

            if (currentData.Heights != null)
            {
                return;
            }

            currentData.Heights = heights;
            currentData.Clone = maps;
        }

        /// <summary>
        /// Add heights terrain modifications.
        /// </summary>
        public void AddDetailsModifications(Terrain terrain, int[,] details, int layersIndex)
        {
            RestoreData currentData;

            if (!m_RestoreData.ContainsKey(terrain))
            {
                currentData = new RestoreData();
                m_RestoreData.Add(terrain, currentData);
            }

            currentData = m_RestoreData[terrain];

            if (currentData.Details.ContainsKey(layersIndex))
            {
                return;
            }

            currentData.Details.Add(layersIndex, details);
        }

        /// <summary>
        /// Revert all the terrain modifications.
        /// </summary>
        public void RevertModifications()
        {
            Terrain currentTerrain;

            foreach (KeyValuePair<Terrain, RestoreData> currentData in m_RestoreData)
            {
                currentTerrain = currentData.Key;

                if (currentData.Value.Heights != null)
                {
                    currentTerrain.terrainData.SetHeights(0, 0, currentData.Value.Heights);
                }

                if (currentData.Value.Clone != null)
                {
                    currentTerrain.terrainData.SetAlphamaps(0, 0, currentData.Value.Clone);
                }

                foreach (KeyValuePair<int, int[,]> currentLayerIndex in currentData.Value.Details)
                {
                    currentTerrain.terrainData.SetDetailLayer(0, 0, currentLayerIndex.Key, currentLayerIndex.Value);
                }
            }
        }

        #endregion
    }
}