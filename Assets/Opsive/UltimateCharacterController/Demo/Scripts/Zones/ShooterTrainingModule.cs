/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Demo.Zones
{
    using Opsive.Shared.Game;
    using Opsive.Shared.Utility;
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    
    /// <summary>
    /// This module is used in the demo scene to showcase the shooter weapon functionalities.
    /// </summary>
    public class ShooterTrainingModule : MonoBehaviour
    {
        [Serializable]
        public enum SpawnOption
        {
            Sequence,       // Spawn the prefabs in sequence.
            Random,         // Spawn the prefabs in any order.
            RandomNoRepeat, // Spawn the prefabs in any order but making sure not to repeat the same one twice in a row.
            PingPong        // Spawn the prefabs in sequence and once the last one is reached, spawn in reverse order.
        }
        
        [Serializable]
        public enum SpawnPositionOption
        {
            Grid,   // Spawn the prefabs in a grid.
            Random, // Spawn the prefabs randomly
            Circle  // spawn the prefabs on a circle circumference.
        }
        
        [Tooltip("Choose what prefab is selected to be spawned next.")]
        [SerializeField] private SpawnOption m_SpawnOption;
        [Tooltip("Choose how the objects should be spawned.")]
        [SerializeField] private SpawnPositionOption m_SpawnPositionOption;
        [Tooltip("The Initial object used to start the Training.")]
        [SerializeField] private ShooterTrainingModuleTarget m_InitialObject;
        [Tooltip("The prefabs to spawn.")]
        [SerializeField] private GameObject[] m_Prefabs;
        [Tooltip("The Spawned objects parent.")]
        [SerializeField] private Transform m_SpawnPoint;
        [Tooltip("The maximum number of targets active at any time.")]
        [SerializeField] private int m_MaxActive = 1;
        [Tooltip("Min Max distance of that the prefab can spawn from the initial position.")]
        [SerializeField] private MinMaxFloat m_MinMaxDistance = new MinMaxFloat(0, 1.5f);
        [Tooltip("The number of targets hit before the initial object is turned back on.")]
        [SerializeField] private int m_TargetGoal;
        [Tooltip("Reset After Delay if no hit.")]
        [SerializeField] private int m_TimeLimit;
        [Tooltip("Should a target be spawned when one is destroyed.")]
        [SerializeField] private bool m_SpawnOnDestroy = true;
        [Tooltip("The number of spawned targets per second.")]
        [SerializeField] private float m_SpawnFrequency = 0.5f;

        private bool m_WaitingForInitialTarget;
        
        private List<int> m_GridIndexAvailable;
        private GameObject[] m_GridObjects;
        
        private int m_TargetShotCountSinceInitial;
        private List<GameObject> m_SpawnedTargets;
        private int m_LastSpawnedPrefabIndex = -1;

        private bool m_PingPongForward = true;

        private float m_InitialTime;
        private float m_LastSpawnTime;
        private float m_LastKillTime;
        
        /// <summary>
        /// Initialize once in awake.
        /// </summary>
        private void Awake()
        {
            m_GridIndexAvailable = new List<int>();
            m_GridIndexAvailable.AddRange(new []{0,1,2,3,4,5,6,7,8});
            m_GridObjects = new GameObject[9];
            m_SpawnedTargets = new List<GameObject>();
            m_InitialObject.OnDeath += HandleInitialDeath;
        }

        /// <summary>
        /// Handle the initial object dying.
        /// </summary>
        /// <param name="target"></param>
        private void HandleInitialDeath(ShooterTrainingModuleTarget target)
        {
            m_TargetShotCountSinceInitial = 0;
            m_InitialTime = Time.time;
            m_LastKillTime = Time.time;
            m_WaitingForInitialTarget = false;
            target.gameObject.SetActive(false);
            SpawnNext();
        }

        /// <summary>
        /// Initialize in enable as module are enabled and disabled constatly.
        /// </summary>
        private void OnEnable()
        {
            ResetToInitial();
            m_LastSpawnTime = 0;
            m_WaitingForInitialTarget = true;
        }

        /// <summary>
        /// Update every frame.
        /// </summary>
        private void Update()
        {
            if(m_WaitingForInitialTarget){ return; }

            var period = 1 / m_SpawnFrequency;
            if (Time.time > period + m_LastSpawnTime) {
                SpawnNext();
            }

            if (m_TimeLimit > 0 && Time.time > m_LastKillTime + m_TimeLimit) {
                ResetToInitial();
            }
        }

        /// <summary>
        /// Reset to the initial state.
        /// </summary>
        private void ResetToInitial()
        {
            // Revive target.
            m_WaitingForInitialTarget = true;
            
            // Force a disable/enable for the visual effects
            m_InitialObject.gameObject.SetActive(false);
            m_GridIndexAvailable.Clear();
            m_GridIndexAvailable.AddRange(new []{0,1,2,3,4,5,6,7,8});

            for (int i = 0; i < m_SpawnedTargets.Count; i++) {
                ObjectPool.Destroy(m_SpawnedTargets[i].gameObject);
            }
            m_SpawnedTargets.Clear();
            
            m_InitialObject.Respawn();
            m_InitialObject.gameObject.SetActive(true);
        }
        
        /// <summary>
        /// Spawn the next prefab.
        /// </summary>
        private void SpawnNext()
        {
            m_LastSpawnTime = Time.time;
            if (m_MaxActive > 0 && m_SpawnedTargets.Count >= m_MaxActive) {
                return;
            }

            var targetIndex = GetNextTargetIndex();
            var prefabToSpawn = GetNextPrefabToSpawn(targetIndex);
            var spawnPosition = GetSpawnPosition(targetIndex);
            var spawnedObject = ObjectPool.Instantiate(prefabToSpawn, m_SpawnPoint);
            spawnedObject.transform.localPosition = spawnPosition;

            var target = spawnedObject.GetCachedComponent<ShooterTrainingModuleTarget>();
            target.Index = targetIndex;
            target.OnDeath += HandleTargetShot;
            target.Respawn();

            m_SpawnedTargets.Add(spawnedObject);
        }

        /// <summary>
        /// Handle a target being shot down.
        /// </summary>
        /// <param name="target">The target that was shot.</param>
        private void HandleTargetShot(ShooterTrainingModuleTarget target)
        {
            // The object can go back in the grid
            if (m_GridIndexAvailable.Contains(target.Index) == false) {
                m_GridIndexAvailable.Add(target.Index);
            }

            target.OnDeath -= HandleTargetShot;
            // Schedule to allow destory effects to play.
            Scheduler.Schedule(0.5f, () => ObjectPool.Destroy(target.gameObject));

            m_LastKillTime = Time.time;
            m_SpawnedTargets.Remove(target.gameObject);
            
            m_TargetShotCountSinceInitial++;
            if (m_TargetGoal > 0 && m_TargetShotCountSinceInitial >= m_TargetGoal) {
                ResetToInitial();
                return;
            }

            if (m_SpawnOnDestroy) {
                SpawnNext();
            }
        }

        /// <summary>
        /// Get the index of the next target.
        /// </summary>
        /// <returns>The next target index.</returns>
        private int GetNextTargetIndex()
        {
            switch (m_SpawnPositionOption) {
                case SpawnPositionOption.Grid:
                    if (m_GridIndexAvailable.Count == 0) {
                        Debug.LogWarning("The grid is full already");
                        return 4;
                    }
                    var randomAvailableIndex = m_GridIndexAvailable[UnityEngine.Random.Range(0, m_GridIndexAvailable.Count - 1)];
                    m_GridIndexAvailable.Remove(randomAvailableIndex);
                    return randomAvailableIndex;
                case SpawnPositionOption.Random:
                    return 0;
                case SpawnPositionOption.Circle:
                    return 0;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Get the next prefab to spawn.
        /// </summary>
        /// <param name="index">The index of the target.</param>
        /// <returns>The prefab to spawn.</returns>
        private GameObject GetNextPrefabToSpawn(int index)
        {
            int indexOfPrefabToSpawn = 0;
            switch (m_SpawnOption) {
                case SpawnOption.Sequence:
                    indexOfPrefabToSpawn = m_LastSpawnedPrefabIndex + 1;
                    if (indexOfPrefabToSpawn >= m_Prefabs.Length) { indexOfPrefabToSpawn = 0; }
                    break;
                case SpawnOption.Random:
                    indexOfPrefabToSpawn = UnityEngine.Random.Range(0, m_Prefabs.Length);
                    break;
                case SpawnOption.RandomNoRepeat:
                    indexOfPrefabToSpawn = UnityEngine.Random.Range(0, m_Prefabs.Length);
                    if (indexOfPrefabToSpawn == m_LastSpawnedPrefabIndex) {
                        // To avoid repeat simply add or remove a random amount within limit
                        if (indexOfPrefabToSpawn == 0) {
                            indexOfPrefabToSpawn += UnityEngine.Random.Range(1, m_Prefabs.Length);
                        }else if (indexOfPrefabToSpawn == m_Prefabs.Length - 1) {
                            indexOfPrefabToSpawn -= UnityEngine.Random.Range(1, m_Prefabs.Length);
                        } else {
                            if (UnityEngine.Random.Range(0, 2) % 2 == 0) {
                                indexOfPrefabToSpawn = UnityEngine.Random.Range(0, indexOfPrefabToSpawn);
                            } else {
                                indexOfPrefabToSpawn = UnityEngine.Random.Range(indexOfPrefabToSpawn+1, m_Prefabs.Length);
                            }
                        }
                    }

                    break;
                case SpawnOption.PingPong:
                    if (m_PingPongForward) {
                        indexOfPrefabToSpawn = m_LastSpawnedPrefabIndex + 1;
                        if (indexOfPrefabToSpawn >= m_Prefabs.Length) {
                            indexOfPrefabToSpawn = m_Prefabs.Length-2;
                            m_PingPongForward = false;
                        }
                    } else {
                        indexOfPrefabToSpawn = m_LastSpawnedPrefabIndex - 1;
                        if (indexOfPrefabToSpawn < 0) {
                            indexOfPrefabToSpawn = 1;
                            m_PingPongForward = true;
                        }
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (indexOfPrefabToSpawn < 0) {
                indexOfPrefabToSpawn = 0;
            }else if (indexOfPrefabToSpawn >= m_Prefabs.Length) {
                indexOfPrefabToSpawn = m_Prefabs.Length - 1;
            }
            var prefabToSpawn = m_Prefabs[indexOfPrefabToSpawn];
            return prefabToSpawn;
        }

        /// <summary>
        /// Get the spawn position for the prefab.
        /// </summary>
        /// <param name="index">The index of the target.</param>
        /// <returns>The spawn position.</returns>
        private Vector3 GetSpawnPosition(int index)
        {
            switch (m_SpawnPositionOption) {
                case SpawnPositionOption.Grid:
                    return GetPositionOnGrid(index);
                case SpawnPositionOption.Random:
                    return GetRandomPosition();
                case SpawnPositionOption.Circle:
                    return GetPositionOnCircle();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Get the position within the grid.
        /// </summary>
        /// <param name="index">The target index.</param>
        /// <returns>The position in the grid.</returns>
        private Vector3 GetPositionOnGrid(int index)
        {
            // The grid is of 9 elements from top left to bottom right, horizontal first.
            var x = index % 3;
            var y = index / 3;

            var xPos = (x - 1) * m_MinMaxDistance.MaxValue;
            var yPos = (y - 1) * m_MinMaxDistance.MaxValue;
            return new Vector3(xPos, yPos, 0);
        }

        /// <summary>
        /// Get the position on the circle.
        /// </summary>
        /// <returns>The position on the circle.</returns>
        private Vector3 GetPositionOnCircle()
        {
            var randomVector2 = UnityEngine.Random.insideUnitCircle.normalized;
            var magnitude = m_MinMaxDistance.MaxValue;
            return randomVector2 * magnitude;
        }

        /// <summary>
        /// Get a random position.
        /// </summary>
        /// <returns>The random position.</returns>
        private Vector3 GetRandomPosition()
        {
            var randomVector2 = UnityEngine.Random.insideUnitCircle.normalized;
            var magnitude = UnityEngine.Random.Range(m_MinMaxDistance.MinValue, m_MinMaxDistance.MaxValue);
            return randomVector2 * magnitude;
        }
    }
}
