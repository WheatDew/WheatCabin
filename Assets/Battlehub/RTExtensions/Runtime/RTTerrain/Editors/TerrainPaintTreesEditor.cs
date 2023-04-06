using Battlehub.RTCommon;
using Battlehub.RTEditor;
using Battlehub.Utils;
using System;
using UnityEngine;

namespace Battlehub.RTTerrain
{
    public class TerrainPaintTreesEditor : TerrainPaintEditor
    {
        [SerializeField]
        private TerrainTreeEditor m_terrainTreeEditor = null;

        [SerializeField]
        private FloatEditor m_minHeightEditor = null;

        [SerializeField]
        private FloatEditor m_maxHeightEditor = null;

        [SerializeField]
        private FloatEditor m_minWidthEditor = null;

        [SerializeField]
        private FloatEditor m_maxWidthEditor = null;

        [SerializeField]
        private BoolEditor m_lockWidthToHeightEditor = null;

        private float m_minHeight = 1;
        public float MinHeight
        {
            get { return m_minHeight; }
            set
            {
                m_minHeight = value;

                TerrainTreesBrush brush = (TerrainTreesBrush)TerrainEditor.Projector.TerrainBrush;
                if(brush != null)
                {
                    brush.MinHeight = value;
                }
            }
        }

        private float m_maxHeight = 1;
        public float MaxHeight
        {
            get { return m_maxHeight; }
            set
            {
                m_maxHeight = value;

                TerrainTreesBrush brush = (TerrainTreesBrush)TerrainEditor.Projector.TerrainBrush;
                if (brush != null)
                {
                    brush.MaxHeight = value;
                }
            }
        }

        private float m_minWidth = 1;
        public float MinWidth
        {
            get { return m_minWidth; }
            set
            {
                m_minWidth = value;

                TerrainTreesBrush brush = (TerrainTreesBrush)TerrainEditor.Projector.TerrainBrush;
                if (brush != null)
                {
                    brush.MinWidth = value;
                }
            }
        }

        private float m_maxWidth = 1;
        public float MaxWidth
        {
            get { return m_maxWidth; }
            set
            {
                m_maxWidth = value;

                TerrainTreesBrush brush = (TerrainTreesBrush)TerrainEditor.Projector.TerrainBrush;
                if (brush != null)
                {
                    brush.MaxWidth = value;
                }
            }
        }

        private bool m_lockWidthToHeight = true;
        public bool LockWidthToHeight
        {
            get { return m_lockWidthToHeight; }
            set
            {
                m_lockWidthToHeight = value;
                TerrainTreesBrush brush = (TerrainTreesBrush)TerrainEditor.Projector.TerrainBrush;
                if (brush != null)
                {
                    brush.LockWidthToHeight = value;
                }
            }
        }

        protected override void Awake()
        {
            base.Awake();

            ILocalization lc = IOC.Resolve<ILocalization>();

            m_lockWidthToHeightEditor.Init(this, this, Strong.PropertyInfo((TerrainPaintTreesEditor x) => x.LockWidthToHeight), null, lc.GetString("ID_RTTerrain_TerrainPaintTreesEditor_LockWidthToHeight", "Lock Width to Height"), null, null, null, false);
            m_minHeightEditor.Init(this, this, Strong.PropertyInfo((TerrainPaintTreesEditor x) => x.MinHeight), null, lc.GetString("ID_RTTerrain_TerrainPaintTreesEditor_MinTreeHeight", "Min Tree Height"), null, null, null, false);
            m_maxHeightEditor.Init(this, this, Strong.PropertyInfo((TerrainPaintTreesEditor x) => x.MaxHeight), null, lc.GetString("ID_RTTerrain_TerrainPaintTreesEditor_MaxTreeHeight", "Max Tree Height"), null, null, null, false);
            m_minWidthEditor.Init(this, this, Strong.PropertyInfo((TerrainPaintTreesEditor x) => x.MinWidth), null, lc.GetString("ID_RTTerrain_TerrainPaintTreesEditor_MinTreeWidth", "Min Tree Width"), null, null, null, false);
            m_maxWidthEditor.Init(this, this, Strong.PropertyInfo((TerrainPaintTreesEditor x) => x.MaxWidth), null, lc.GetString("ID_RTTerrain_TerrainPaintTreesEditor_MaxTreeWidth", "Max Tree Width"), null, null, null, false);
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (m_terrainTreeEditor != null)
            {
                m_terrainTreeEditor.SelectedTreeChanged += OnSelectedTreeChanged;
            }

            if (TerrainEditor.Terrain != null)
            {
                m_terrainTreeEditor.Terrain = TerrainEditor.Terrain;
            }

            if (TerrainEditor.Projector != null)
            {
                TerrainEditor.Projector.gameObject.SetActive(GetTerrainTreeIndex() >= 0);
            }

        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if (m_terrainTreeEditor != null)
            {
                m_terrainTreeEditor.SelectedTreeChanged -= OnSelectedTreeChanged;
            }
        }

        private void OnSelectedTreeChanged(object sender, EventArgs e)
        {
            InitializeTerrainBrush();
            TerrainTreesBrush brush = (TerrainTreesBrush)TerrainEditor.Projector.TerrainBrush;
            brush.TerrainTreeIndex = GetTerrainTreeIndex();
            brush.MinWidth = MinWidth;
            brush.MaxWidth = MaxWidth;
            brush.MinHeight = MinHeight;
            brush.MaxHeight = MaxHeight;
            brush.LockWidthToHeight = LockWidthToHeight;
            TerrainEditor.Projector.gameObject.SetActive(brush.TerrainTreeIndex >= 0);
        }

        private int GetTerrainTreeIndex()
        {
            return m_terrainTreeEditor.SelectedTree != null ? Array.IndexOf(m_terrainTreeEditor.Terrain.terrainData.treePrototypes, m_terrainTreeEditor.SelectedTree) : -1;
        }

        protected override void OnTerrainChanged()
        {
            if (m_terrainTreeEditor != null)
            {
                if (TerrainEditor.Terrain == null || TerrainEditor.Terrain.terrainData == null)
                {
                    m_terrainTreeEditor.Terrain = null;
                }
                else
                {
                    m_terrainTreeEditor.Terrain = TerrainEditor.Terrain;
                }
            }
        }

        protected override Brush CreateBrush()
        {
            TerrainEditor.Projector.gameObject.SetActive(GetTerrainTreeIndex() >= 0);
            return new TerrainTreesBrush()
            {
                TerrainTreeIndex = GetTerrainTreeIndex(),
                MinWidth = MinWidth,
                MaxWidth = MaxWidth,
                MinHeight = MinHeight,
                MaxHeight = MaxHeight,
                LockWidthToHeight = LockWidthToHeight,
            };
        }
    }
}
