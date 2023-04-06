using System;
using UnityEngine;

namespace Battlehub.UIControls.DockPanels
{
    public class RegionPopupBehaviour : MonoBehaviour
    {
        private Region m_region;

        private RegionPopupBackground m_regionPopupBackground;
            
        private void Awake()
        {
            m_region = GetComponent<Region>();
            m_region.IsHeaderVisible = false;
            Region.Unselected += OnUnselected;
        }

        private void Start()
        {
            GameObject background = new GameObject("Background");
            background.transform.SetParent(transform.parent, false);
            background.transform.SetSiblingIndex(transform.GetSiblingIndex());

            m_regionPopupBackground = background.AddComponent<RegionPopupBackground>();
            m_regionPopupBackground.Close += OnClose;
        }

        private void OnDestroy()
        {
            DestroyBackground();
            Region.Unselected -= OnUnselected;
        }

        private void Update()
        {
            Vector3 pos = m_regionPopupBackground.transform.localPosition;
            pos.z = transform.localPosition.z;
            m_regionPopupBackground.transform.localPosition = pos;
        }

        private void OnClose(object sender, EventArgs e)
        {
            DestroyBackground();
            m_region.Destroy();
        }

        private void OnUnselected(Region region)
        {
            if (region.Root.Modal.gameObject.activeSelf)
            {
                return;
            }

            if(region == m_region)
            {
                DestroyBackground();
                region.Destroy();
            }
        }

        private void DestroyBackground()
        {
            if (m_regionPopupBackground != null)
            {
                Destroy(m_regionPopupBackground.gameObject);
                m_regionPopupBackground.Close -= OnClose;
                m_regionPopupBackground = null;
            }
        }

    }
}
