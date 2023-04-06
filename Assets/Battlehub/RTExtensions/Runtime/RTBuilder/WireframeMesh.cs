using Battlehub.ProBuilderIntegration;
using Battlehub.Utils;
using System;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Rendering;

namespace Battlehub.RTBuilder
{
    public class WireframeMesh : MonoBehaviour
    {
        private PBMesh m_pbMesh;
        private MeshFilter m_filter;
        private Color m_color;
        private bool m_update;
        public bool IsIndividual
        {
            get;
            set;
        }

        private static readonly MD5 s_hashAlgo = MD5.Create();
        
        private Color m_selectionColor = new Color(1, 0.35f, 0, 1);
        private Color[] m_selectionColorsCache;
        private Color[] m_normalColorsCache;

        private bool m_isSelected;
        public bool IsSelected
        {
            get { return m_isSelected; }
            set
            {
                if(m_isSelected != value)
                {
                    m_isSelected = value;
                    if(m_filter != null && m_filter.sharedMesh != null)
                    {
                        if (m_isSelected)
                        {
                            m_filter.sharedMesh.colors = m_selectionColorsCache;
                        }
                        else
                        {
                            m_filter.sharedMesh.colors = m_normalColorsCache;
                        }
                    }
                }
            }
        }

        private void Awake()
        {
            m_pbMesh = GetComponentInParent<PBMesh>();
            m_pbMesh.Selected += OnPBMeshSelected;
            m_pbMesh.Changed += OnPBMeshChanged;
            m_pbMesh.Unselected += OnPBMeshUnselected;

            int instanceId = Mathf.Abs(BitConverter.ToInt32(s_hashAlgo.ComputeHash(BitConverter.GetBytes(m_pbMesh.GetInstanceID())), 0));
            Color32[] colors = Colors.Kellys;
            m_color = colors[instanceId % colors.Length];
            m_filter = GetComponent<MeshFilter>();
            if (!m_filter)
            {
                m_filter = gameObject.AddComponent<MeshFilter>();
            }

            if (!m_filter.sharedMesh)
            {
                m_filter.sharedMesh = new Mesh();
            }

            MeshRenderer renderer = GetComponent<MeshRenderer>();
            if (renderer == null)
            {
                renderer = gameObject.AddComponent<MeshRenderer>();
            }

            renderer.sharedMaterial = PBBuiltinMaterials.LinesMaterial;
            renderer.sharedMaterial.SetColor("_Color", Color.white);
            renderer.sharedMaterial.SetInt("_HandleZTest", (int)CompareFunction.LessEqual);
            renderer.sharedMaterial.SetFloat("_Scale", 0.5f);

            m_pbMesh.BuildEdgeMesh(m_filter.sharedMesh, IsSelected ? m_selectionColor : m_color, false);
            UpdateColorCache();
        }

        private void OnDestroy()
        {
            if(m_pbMesh != null)
            {
                m_pbMesh.Selected -= OnPBMeshSelected;
                m_pbMesh.Changed -= OnPBMeshChanged;
                m_pbMesh.Unselected -= OnPBMeshUnselected;
            }
        }

        private void OnPBMeshSelected(bool clear)
        {
            if(clear)
            {
                if (m_filter.sharedMesh != null)
                {
                    Destroy(m_filter.sharedMesh);
                    m_filter.sharedMesh = new Mesh();
                    TryUpdateColorCache();
                }
            }

            m_update = !clear;
            
        }

        private void OnPBMeshChanged(bool positionsOnly, bool forceUpdate)
        {
            if(m_update || forceUpdate)
            {
                m_pbMesh.BuildEdgeMesh(m_filter.sharedMesh, IsSelected ? m_selectionColor : m_color, positionsOnly);
                TryUpdateColorCache();
            }
        }

    
        private void OnPBMeshUnselected()
        {
            m_pbMesh.BuildEdgeMesh(m_filter.sharedMesh, IsSelected ? m_selectionColor : m_color, false);
            TryUpdateColorCache();
            m_update = false;
        }

        private void UpdateColorCache()
        {
            m_selectionColorsCache = m_filter.sharedMesh.colors;
            for (int i = 0; i < m_selectionColorsCache.Length; ++i)
            {
                m_selectionColorsCache[i] = m_selectionColor;
            }
            m_normalColorsCache = m_filter.sharedMesh.colors;
            for(int i = 0; i < m_normalColorsCache.Length; ++i)
            {
                m_normalColorsCache[i] = m_color;
            }
        }

        private void TryUpdateColorCache()
        {
            if (m_filter.sharedMesh.colors.Length != m_normalColorsCache.Length)
            {
                UpdateColorCache();
            }
        }
    }

}
