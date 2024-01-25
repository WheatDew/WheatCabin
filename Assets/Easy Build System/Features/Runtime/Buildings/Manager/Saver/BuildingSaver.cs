/// <summary>
/// Project : Easy Build System
/// Class : BuildingSaver.cs
/// Namespace : EasyBuildSystem.Features.Runtime.Buildings.Manager.Saver
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using System;
using System.IO;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

using EasyBuildSystem.Features.Runtime.Bases;

using EasyBuildSystem.Features.Runtime.Buildings.Part;

using Debug = UnityEngine.Debug;

namespace EasyBuildSystem.Features.Runtime.Buildings.Manager.Saver
{
    [HelpURL("https://polarinteractive.gitbook.io/easy-build-system/components/building-manager/building-saver")]
    public class BuildingSaver : Singleton<BuildingSaver>
    {
        #region Fields

        [SerializeField] bool m_UseAutoSaver;
        [SerializeField] float m_AutoSaverInterval = 60f;

        bool m_LoadBuildingAtStart = true;
        public bool LoadBuildingAtStart { get { return m_LoadBuildingAtStart; } set { m_LoadBuildingAtStart = value; } }

        bool m_SaveBuildingAtExit = true;
        public bool SaveBuildingAtExit { get { return m_SaveBuildingAtExit; } set { m_SaveBuildingAtExit = value; } }

        float m_TimerAutoSave;
        bool m_Save;

        [Serializable]
        public class SaveData
        {
            public List<BuildingPart.SaveSettings> Data = new List<BuildingPart.SaveSettings>();
        }

        string m_SavePath;
         
        public string GetSavePath
        {
            get
            {
                if (m_SavePath == string.Empty)
                {
                    if (Application.platform == RuntimePlatform.Android)
                    {
                        m_SavePath = SceneManager.GetActiveScene().name + "_save";
                    }
                    else
                    {
                        m_SavePath = Application.persistentDataPath + "/data_" + SceneManager.GetActiveScene().name.Replace(" ", "") + "_save.txt";
                    }
                }

                return m_SavePath;
            }

            set
            {
                m_SavePath = value;
            }
        }

        #region Events

        /// <summary>
        /// Called when the save starts.
        /// </summary>
        [Serializable] public class StartSaveEvent : UnityEvent { }
        public StartSaveEvent OnStartSaveEvent = new StartSaveEvent();

        /// <summary>
        /// Called when the save ends.
        /// </summary>
        [Serializable] public class EndingSaveEvent : UnityEvent<BuildingPart[]> { }
        public EndingSaveEvent OnEndingSaveEvent = new EndingSaveEvent();

        /// <summary>
        /// Called when loading starts.
        /// </summary>
        [Serializable] public class StartLoadingEvent : UnityEvent { }
        public StartLoadingEvent OnStartLoadingEvent = new StartLoadingEvent();

        /// <summary>
        /// Called when loading ends.
        /// </summary>
        [Serializable] public class EndingLoadingEvent : UnityEvent<BuildingPart[], long> { }
        public EndingLoadingEvent OnEndingLoadingEvent = new EndingLoadingEvent();

        #endregion

        #endregion

        #region Unity Methods

        public virtual void Start()
        {
            m_SavePath = string.Empty;

            if (m_UseAutoSaver)
            {
                m_TimerAutoSave = m_AutoSaverInterval;
            }

            if (m_LoadBuildingAtStart)
            {
                StartCoroutine(Load(GetSavePath));
            }
        }

        public virtual void Update()
        {
            if (m_UseAutoSaver)
            {
                if (m_TimerAutoSave <= 0)
                {
                    StartCoroutine(Save(GetSavePath));
                    m_TimerAutoSave = m_AutoSaverInterval;
                }
                else
                {
                    m_TimerAutoSave -= Time.deltaTime;
                }
            }
        }

        public virtual void OnApplicationPause(bool pause)
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                StartCoroutine(Save(GetSavePath));
            }
        }

        public virtual void OnApplicationQuit()
        {
            if (m_SaveBuildingAtExit)
            {
                StartCoroutine(Save(GetSavePath));
            }
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Force save.
        /// </summary>
        public void ForceSave(string path = null)
        {
            StartCoroutine(Save(path == null ? GetSavePath : path));
        }

        /// <summary>
        /// Force loading.
        /// </summary>
        public void ForceLoad(string path = null)
        {
            StartCoroutine(Load(path == null ? GetSavePath : path));
        }

        /// <summary>
        /// Save all the Building Parts present in the current scene.
        /// </summary>
        IEnumerator Save(string path)
        {
            if (m_Save)
            {
                yield break;
            }

            m_Save = true;

            List<BuildingPart> savedBuildingParts = new List<BuildingPart>();

            OnStartSaveEvent.Invoke();

            if (Application.platform != RuntimePlatform.Android)
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                if (BuildingManager.Instance.RegisteredBuildingParts.Count > 0)
                {
                    SaveData saveData = new SaveData
                    {
                        Data = new List<BuildingPart.SaveSettings>()
                    };

                    BuildingPart[] registeredBuildingParts = BuildingManager.Instance.RegisteredBuildingParts.ToArray();

                    for (int i = 0; i < registeredBuildingParts.Length; i++)
                    {
                        if (registeredBuildingParts[i] != null)
                        {
                            if (registeredBuildingParts[i].State == BuildingPart.StateType.PLACED)
                            {
                                BuildingPart.SaveSettings saveSettings = registeredBuildingParts[i].GetSaveData();

                                if (saveSettings != null)
                                {
                                    saveData.Data.Add(saveSettings);
                                    savedBuildingParts.Add(registeredBuildingParts[i]);
                                }
                            }
                        }
                    }

                    File.AppendAllText(path, JsonUtility.ToJson(saveData));
                }
            }
            else if (Application.platform == RuntimePlatform.Android)
            {
                SaveData saveData = new SaveData();

                BuildingPart[] registeredBuildingParts = BuildingManager.Instance.RegisteredBuildingParts.ToArray();

                for (int i = 0; i < registeredBuildingParts.Length; i++)
                {
                    if (registeredBuildingParts[i] != null)
                    {
                        if (registeredBuildingParts[i].State != BuildingPart.StateType.PREVIEW)
                        {
                            BuildingPart.SaveSettings saveSettings = registeredBuildingParts[i].GetSaveData();

                            if (saveSettings != null)
                            {
                                saveData.Data.Add(saveSettings);
                                savedBuildingParts.Add(registeredBuildingParts[i]);
                            }
                        }
                    }
                }

                PlayerPrefs.SetString(path, JsonUtility.ToJson(saveData));
                PlayerPrefs.Save();
            }

            if (savedBuildingParts.Count != 0)
            {
                Debug.Log("<b>Easy Build System</b> : Save of " + savedBuildingParts.Count + " Building Parts.");
            }

            OnEndingSaveEvent.Invoke(savedBuildingParts.ToArray());

            m_Save = false;

            yield break;
        }

        bool m_Loading;

        /// <summary>
        /// Load all the Building Parts from a save file.
        /// </summary>
        IEnumerator Load(string path)
        {
            if (m_Loading)
            {
                yield break;
            }

            m_Loading = true;

            if (!File.Exists(path))
            {
                m_Loading = false;
                yield break;
            }

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            List<BuildingPart> loadedBuildingParts = new List<BuildingPart>();

            OnStartLoadingEvent.Invoke();

            if (Application.platform != RuntimePlatform.Android)
            {
                if (File.Exists(path))
                {
                    try
                    {
                        SaveData saveData = JsonUtility.FromJson<SaveData>(File.ReadAllText(path));

                        for (int i = 0; i < saveData.Data.Count; i++)
                        {
                            if (saveData.Data[i] != null)
                            {
                                BuildingPart buildingPart = BuildingManager.Instance.GetBuildingPartByIdentifier(saveData.Data[i].Identifier);

                                if (buildingPart != null)
                                {
#if MIRROR
                                    loadedBuildingParts.Add(BuildingManager.Instance.PlaceBuildingPart(buildingPart,
                                        saveData.Data[i].Position,
                                        saveData.Data[i].Rotation,
                                        saveData.Data[i].Scale, false));
#elif PUNV2
                                    loadedBuildingParts.Add(BuildingManager.Instance.PlaceBuildingPart(buildingPart,
                                        saveData.Data[i].Position,
                                        saveData.Data[i].Rotation,
                                        saveData.Data[i].Scale, false));
#else
                                    loadedBuildingParts.Add(BuildingManager.Instance.PlaceBuildingPart(buildingPart,
                                        saveData.Data[i].Position,
                                        saveData.Data[i].Rotation,
                                        saveData.Data[i].Scale));
#endif
                                }
                                else
                                {
                                    Debug.LogWarning("<b>Easy Build System</b> : The Building Part reference with the name: <b>" + saveData.Data[i].Name +
                                        "</b> does not exists in Building Manager.");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex.ToString());
                    }
                }
            }
            else if (Application.platform == RuntimePlatform.Android)
            {
                SaveData saveData = JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString(path));

                for (int i = 0; i < saveData.Data.Count; i++)
                {
                    if (saveData.Data[i] != null)
                    {
                        BuildingPart buildingPart = BuildingManager.Instance.GetBuildingPartByIdentifier(saveData.Data[i].Identifier);

                        if (buildingPart != null)
                        {
                            loadedBuildingParts.Add(BuildingManager.Instance.PlaceBuildingPart(buildingPart,
                                saveData.Data[i].Position,
                                saveData.Data[i].Rotation,
                                saveData.Data[i].Scale));
                        }
                        else
                        {
                            Debug.LogWarning("<b>Easy Build System</b> : The Building Part reference with the name: <b>" + saveData.Data[i].Name +
                                        "</b> does not exists in Building Manager.");
                        }
                    }
                }
            }

            stopWatch.Stop();

            OnEndingLoadingEvent.Invoke(loadedBuildingParts.ToArray(), stopWatch.ElapsedMilliseconds);

            if (loadedBuildingParts.Count != 0)
            {
                Debug.Log("Loading of " + loadedBuildingParts.Count + " Building Parts in " + stopWatch.Elapsed.TotalSeconds.ToString("0.00") + " seconds.");
            }

            m_Loading = false;

            yield break;
        }

        #endregion
    }
}