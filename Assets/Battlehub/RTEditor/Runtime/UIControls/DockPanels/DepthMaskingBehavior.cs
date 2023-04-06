using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.UIControls.DockPanels
{
    public class DepthMaskingBehavior : MonoBehaviour
    {
        [SerializeField]
        private Material m_depthMaskMaterial = null;

        [SerializeField]
        private float m_depthMaskOffset = 1.0f;

        private DockPanel m_root;

        private RectTransform m_depthMasks;

        [SerializeField]
        private RectTransform[] m_foregroundLayerObjects = null;

        private class DepthMask
        {
            public RectTransform Transform;
            public Image Graphic;
            public int Depth;

            public DepthMask(RectTransform rt, Image image)
            {
                Transform = rt;
                Graphic = image;
            }
        }

        private Dictionary<Region, DepthMask> m_regionToDepthMask = new Dictionary<Region, DepthMask>();

        private DepthMask[] m_dragDepthMasks;
        private Vector3 m_previousDragPosition;
        
        private void Awake()
        {
            m_root = GetComponent<DockPanel>();

            GameObject depthMasks = new GameObject();
            depthMasks.name = "DepthMasks";
            RectTransform depthMasksRT = depthMasks.AddComponent<RectTransform>();

            depthMasksRT.SetParent(m_root.Free.parent, false);
            depthMasksRT.SetSiblingIndex(0);
            depthMasksRT.Stretch();
            m_depthMasks = depthMasksRT;

            Region[] regions = m_root.GetComponentsInChildren<Region>();
            for(int i = 0; i < regions.Length; ++i)
            {
                Region region = regions[i];
                CreateDepthMask(region);
            }

            m_root.RegionEnabled += OnRegionEnabled;
            m_root.RegionDisabled += OnRegionDisabled;
            m_root.RegionCreated += OnRegionCreated;
            m_root.RegionDestroyed += OnRegionDestroyed;
            m_root.RegionBeginResize += OnRegionBeginResize;
            m_root.RegionResize += OnRegionResize;
            m_root.RegionEndResize += OnRegionEndResize;
            m_root.RegionBeginDrag += OnRegionBeginDrag;
            m_root.RegionDrag += OnRegionDrag;
            m_root.RegionEndDrag += OnRegionEndDrag;
            m_root.RegionTranformChanged += OnRegionTransformChanged;
            m_root.RegionDepthChanged += OnRegionDepthChanged;

            foreach(RectTransform rt in m_foregroundLayerObjects)
            {
                rt.localPosition = -Vector3.forward * 1;
            }
        }

        private void OnDestroy()
        {
            if(m_depthMasks != null)
            {
                Destroy(m_depthMasks.gameObject);
            }

            m_regionToDepthMask = null;

            if(m_root != null)
            {
                m_root.RegionEnabled -= OnRegionEnabled;
                m_root.RegionDisabled -= OnRegionDisabled;
                m_root.RegionCreated -= OnRegionCreated;
                m_root.RegionDestroyed -= OnRegionDestroyed;
                m_root.RegionBeginResize -= OnRegionBeginResize;
                m_root.RegionResize -= OnRegionResize;
                m_root.RegionEndResize -= OnRegionEndResize;
                m_root.RegionBeginDrag -= OnRegionBeginDrag;
                m_root.RegionDrag -= OnRegionDrag;
                m_root.RegionEndDrag -= OnRegionEndDrag;
                m_root.RegionTranformChanged -= OnRegionTransformChanged;
                m_root.RegionDepthChanged -= OnRegionDepthChanged;
            }
        }

        private void CreateDepthMask(Region region)
        {
            if(m_regionToDepthMask.ContainsKey(region))
            {
                return;
            }

            GameObject depthMaskGO = new GameObject();
            depthMaskGO.name = "DepthMask";

            RectTransform depthMaskRT = depthMaskGO.AddComponent<RectTransform>();
            depthMaskRT.SetParent(m_depthMasks);

            Image image = depthMaskGO.AddComponent<Image>();
            image.material = m_depthMaskMaterial;
            image.enabled = region.GetDragRegion() == region.transform;
            image.raycastTarget = false;

            DepthMask depthMask = new DepthMask(depthMaskRT, image);
            m_regionToDepthMask.Add(region, depthMask);

            UpdateDepthMaskTransform(region, depthMask);
        }

        private void DestroyDepthMask(Region region)
        {
            DepthMask depthMask;
            if(m_regionToDepthMask.TryGetValue(region, out depthMask))
            {
                Destroy(depthMask.Transform.gameObject);
                m_regionToDepthMask.Remove(region);
            }
        }

        private void UpdateDepthMaskTransform(Region region, DepthMask depthMask)
        {
            RectTransform regionRT = (RectTransform)region.transform;
            depthMask.Transform.pivot = regionRT.pivot;
            depthMask.Transform.anchorMin = regionRT.anchorMin;
            depthMask.Transform.anchorMax = regionRT.anchorMax;
            depthMask.Transform.offsetMin = regionRT.offsetMin;
            depthMask.Transform.offsetMax = regionRT.offsetMax;
            depthMask.Transform.position = regionRT.transform.position;
            depthMask.Transform.localScale = Vector3.one;

            ApplyDepth(region, depthMask);
        }

        private void OnRegionCreated(Region region)
        {
            CreateDepthMask(region);
        }

        private void OnRegionDestroyed(Region region)
        {
            DestroyDepthMask(region);
        }

        private void OnRegionEnabled(Region region)
        {
            DepthMask depthMask;
            if (m_regionToDepthMask.TryGetValue(region, out depthMask))
            {
                if (depthMask != null)
                {
                    depthMask.Transform.gameObject.SetActive(true);
                }
            }
        }

        private void OnRegionDisabled(Region region)
        {
            DepthMask depthMask;
            if (m_regionToDepthMask.TryGetValue(region, out depthMask))
            {
                if(depthMask != null)
                {
                    depthMask.Transform.gameObject.SetActive(false);
                }
            }
        }


        private void OnRegionDepthChanged(Region region, int depth)
        {
            DepthMask depthMask = null;
            if(!m_regionToDepthMask.TryGetValue(region, out depthMask))
            {
                return;
            }
            if (depth == 0)
            {
                depthMask.Graphic.enabled = false;
            }
            else
            {
                depthMask.Graphic.enabled = region.GetDragRegion() == region.transform;
            }

            depthMask.Depth = depth;
            ApplyDepth(region, depthMask);
        }

        private void ApplyDepth(Region region, DepthMask depthMask)
        {
            Vector3 pos = depthMask.Transform.localPosition;
            pos.z = -depthMask.Depth * 0.05f * m_depthMaskOffset;
            depthMask.Transform.localPosition = pos;

            if (region.transform.parent.GetComponentInParent<Region>() == null)
            {
                pos = region.transform.localPosition;
                pos.z = -(0.025f * m_depthMaskOffset + depthMask.Depth * 0.05f * m_depthMaskOffset);
                region.transform.localPosition = pos;
            }
            else
            {
                pos = region.transform.localPosition;
                pos.z = 0;
                region.transform.localPosition = pos;
            }   

            foreach(Transform content in region.ContentPanel)
            {
                Vector3 contentPos = content.localPosition;
                contentPos.z = 0;
                content.localPosition = contentPos;
            }
        }

        private void OnRegionBeginDrag(Region region)
        {
            Transform dragRegion = region.GetDragRegion();
            if(dragRegion == null)
            {
                m_dragDepthMasks = new[] { m_regionToDepthMask[region] };
            }
            else
            {
                Region[] regions = dragRegion.GetComponentsInChildren<Region>();
                m_dragDepthMasks = regions.Where(r => r.Root == m_root).Select(r => m_regionToDepthMask[r]).ToArray();
            }

            m_previousDragPosition = region.transform.position;
        }

        private void OnRegionDrag(Region region)
        {
            for(int i = 0; i < m_dragDepthMasks.Length; ++i)
            {
                DepthMask depthMask = m_dragDepthMasks[i];
                depthMask.Transform.position += (region.transform.position - m_previousDragPosition);
                m_previousDragPosition = region.transform.position;
                ApplyDepth(region, depthMask);
            }
        }

        private void OnRegionEndDrag(Region region)
        {
            m_dragDepthMasks = null;
        }

        private void OnRegionBeginResize(Resizer resizer, Region region)
        {
            m_dragDepthMasks = new[] { m_regionToDepthMask[region] };
        }

        private void OnRegionResize(Resizer resizer, Region region)
        {
            UpdateDepthMaskTransform(region, m_dragDepthMasks[0]);
        }

        private void OnRegionEndResize(Resizer resizer, Region region)
        {
            m_dragDepthMasks = null;
        }

        private void OnRegionTransformChanged(Region region)
        {
            DepthMask depthMask = m_regionToDepthMask[region];
            UpdateDepthMaskTransform(region, depthMask);
        }

    }
}

