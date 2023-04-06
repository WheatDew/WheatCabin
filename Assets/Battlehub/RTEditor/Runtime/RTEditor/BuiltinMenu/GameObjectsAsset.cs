using Battlehub.RTCommon;
using Battlehub.RTSL;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Battlehub.RTEditor
{
    [DefaultExecutionOrder(-100)]
    [CreateAssetMenu(menuName = "Runtime Editor/Game Objects Asset")]
    public class GameObjectsAsset : ScriptableObject
    {
        [Serializable]
        public struct Prefab
        {
            public GameObject GameObject;
            public Func<Transform, GameObject> Func;
            public string MenuPath;
            
        }

        [SerializeField]
        private Prefab[] m_prefabs = null;

        private Dictionary<string, (GameObject, Func<Transform, GameObject>)> m_menuPathToPrefab;
        
        public string[] MenuPath
        {
            get
            {
                return m_prefabs.Select(p => p.MenuPath).ToArray();
            }
        }

        public void Register(GameObject go, string path)
        {
            if (m_prefabs != null)
            {
                Array.Resize(ref m_prefabs, m_prefabs.Length + 1);
                m_prefabs[m_prefabs.Length - 1] = new Prefab
                {
                    GameObject = go,
                    MenuPath = path,
                };

                if (m_menuPathToPrefab != null)
                {
                    if(!m_menuPathToPrefab.ContainsKey(path))
                    {
                        m_menuPathToPrefab.Add(path, (go, null));
                    }
                    else
                    {
                        Debug.LogWarning($"path {path} already registered");
                    }
                }
            }
        }

        public void Register(Func<Transform, GameObject> func, string path)
        {
            if (m_prefabs != null)
            {
                Array.Resize(ref m_prefabs, m_prefabs.Length + 1);
                m_prefabs[m_prefabs.Length - 1] = new Prefab
                {
                    Func = func,
                    MenuPath = path,
                };

                if (m_menuPathToPrefab != null)
                {
                    if (!m_menuPathToPrefab.ContainsKey(path))
                    {
                        m_menuPathToPrefab.Add(path, (null, func));
                    }
                    else
                    {
                        Debug.LogWarning($"path {path} already registered");
                    }
                }
            }
        }

        public virtual GameObject Instantiate(string path)
        {
            InitMenuPathToPrefab();

            GameObject prefab = m_menuPathToPrefab[path].Item1;
            Func<Transform, GameObject> instantiateFunc = null;
            if(prefab == null)
            {
                instantiateFunc = m_menuPathToPrefab[path].Item2;
                if(instantiateFunc == null)
                {
                    return null;
                }
            }

            if (prefab != null && prefab.GetComponent<RectTransform>())
            {
                Canvas canvas = FindObjectsOfType<Canvas>().Where(c => (c.GetComponentInParent<RTSLIgnore>() == null) && c.hideFlags == HideFlags.None).FirstOrDefault();
                if (canvas != null && !prefab.GetComponent<Canvas>())
                {
                    return Instantiate(prefab, canvas.transform);
                }
            }


            GameObject go = prefab != null ? Instantiate(prefab) : instantiateFunc(null);

            if (RenderPipelineInfo.Type != RPType.Standard)
            {
                Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
                foreach (Renderer renderer in renderers)
                {
                    Material[] materials = renderer.sharedMaterials;
                    for (int i = 0; i < materials.Length; ++i)
                    {
                        Material material = materials[i];
                        if (material.shader != null)
                        {
                            if (material.shader.name == "Standard")
                            {
                                materials[i] = RenderPipelineInfo.DefaultMaterial;
                            }
                        }
                    }
                    renderer.sharedMaterials = materials;
                }
            }

            string[] parts = path.Split('/');
            string id = parts[parts.Length - 1];

            ILocalization localization = IOC.Resolve<ILocalization>();
            ExposeToEditor exposeToEditor = go.GetComponent<ExposeToEditor>();
            if (exposeToEditor != null)
            {
                exposeToEditor.SetName(localization.GetString(id, go.name));
            }
            else
            {
                go.name = localization.GetString(id, go.name);
            }
            return go;
        }

        private void InitMenuPathToPrefab()
        {
            if(m_menuPathToPrefab != null)
            {
                return;
            }

            m_menuPathToPrefab = new Dictionary<string, (GameObject, Func<Transform, GameObject>)>();
            for (int i = 0; i < m_prefabs.Length; ++i)
            {
                Prefab prefab = m_prefabs[i];
                if (m_menuPathToPrefab.ContainsKey(prefab.MenuPath))
                {
                    Debug.LogWarning("Duplicated prefab.MenuPath " + prefab.MenuPath);
                    continue;
                }

                m_menuPathToPrefab.Add(prefab.MenuPath, (prefab.GameObject, prefab.Func));
            }
        }
    }
}
