using Battlehub.RTCommon;
using Battlehub.RTEditor;
using Battlehub.UIControls;
using Battlehub.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTTerrain
{
    public class TerrainDetailEditor : MonoBehaviour
    {
        public class DetailPrototypeWrapper
        {
            public readonly DetailPrototype Prototype;

            public DetailPrototypeWrapper(DetailPrototype prototype)
            {
                Prototype = prototype;
            }
        }

        public event EventHandler SelectedDetailChanged;

        [SerializeField]
        private VirtualizingTreeView m_detailsList = null;

        [SerializeField]
        private Button m_createDetail = null;

        [SerializeField]
        private Button m_removeDetail = null;

        [SerializeField]
        private CanvasGroup m_detailPropertiesGroup = null;

        [SerializeField]
        private ObjectEditor m_detailTextureEditor = null;

        [SerializeField]
        private ObjectEditor m_detailEditor = null;

        [SerializeField]
        private EnumEditor m_renderModeEditor = null;

        [SerializeField]
        private FloatEditor m_bendFactorEditor = null;

        [SerializeField]
        private ColorEditor m_dryColorEditor = null;

        [SerializeField]
        private ColorEditor m_healthyColorEditor = null;

        [SerializeField]
        private FloatEditor m_maxHeightEditor = null;

        [SerializeField]
        private FloatEditor m_maxWidthEditor = null;

        [SerializeField]
        private FloatEditor m_minHeightEditor = null;

        [SerializeField]
        private FloatEditor m_minWidthEditor = null;

        [SerializeField]
        private FloatEditor m_noiseSpreadEditor = null;

        private Terrain m_terrain;
        public Terrain Terrain
        {
            get { return m_terrain; }
            set
            {
                m_terrain = value;
                if (m_terrain == null)
                {
                    TerrainData = null;
                }
                else
                {
                    TerrainData = m_terrain.terrainData;
                }
            }
        }

        private TerrainData m_terrainData;
        private TerrainData TerrainData
        {
            get { return m_terrainData; }
            set
            {
                if(m_terrainData != value)
                {
                    m_terrainData = value;
                    if (m_detailsList != null)
                    {
                        if(m_terrainData != null)
                        {
                            m_detailsList.Items = m_terrainData.detailPrototypes;
                        }
                        else
                        {
                            m_detailsList.Items = null;
                        }
                        
                        UpdateVisualState();
                    }
                }
            }
        }

        public int SelectedIndex
        {
            get { return m_detailsList.SelectedIndex; }
        }


        public DetailPrototype SelectedDetail
        {
            get
            {
                if(m_detailsList.SelectedItem == null)
                {
                    return null;
                }

                return ((DetailPrototypeWrapper)m_detailsList.SelectedItem).Prototype;
            }
        }


        public Texture2D DetailTexture
        {
            get
            {
                if (SelectedDetail == null) { return null;}
                return SelectedDetail.prototypeTexture;
            }
            set
            {
                if (SelectedDetail == null) { return; }
                SelectedDetail.prototypeTexture = value;
                Refresh(Terrain, SelectedIndex, SelectedDetail);
            }
        }

        public GameObject Detail
        {
            get
            {
                if (SelectedDetail == null) { return null; }
                return SelectedDetail.prototype;
            }
            set
            {
                if (SelectedDetail == null) { return; }
                SelectedDetail.prototype = value;
                SelectedDetail.usePrototypeMesh = SelectedDetail.prototype != null && SelectedDetail.prototype.GetComponent<MeshFilter>() != null;
                Refresh(Terrain, SelectedIndex, SelectedDetail);
            }
        }

        public DetailRenderMode RenderMode
        {
            get
            {
                if (SelectedDetail == null) { return  DetailRenderMode.GrassBillboard; }
                return SelectedDetail.renderMode;
            }
            set
            {
                if (SelectedDetail == null) { return; }
                SelectedDetail.renderMode = value;
                Refresh(Terrain, SelectedIndex, SelectedDetail);
            }
        }

#if UNITY_2020_1_OR_NEWER
        public float BendFactor
        {
            get { return 0; }
            set { }
        }
#else
        public float BendFactor
        {
            get
            {
                if (SelectedDetail == null) { return 0.0f; }
                return SelectedDetail.bendFactor;
            }
            set
            {
                if (SelectedDetail == null) { return; }
                SelectedDetail.bendFactor = value;
                Refresh(Terrain, SelectedIndex, SelectedDetail);
            }
        }
#endif
        
        public Color DryColor
        {
            get
            {
                if (SelectedDetail == null) { return Color.white; }
                return SelectedDetail.dryColor;
            }
            set
            {
                if (SelectedDetail == null) { return; }
                SelectedDetail.dryColor = value;
                Refresh(Terrain, SelectedIndex, SelectedDetail);
            }
        }

        public Color HealthyColor
        {
            get
            {
                if (SelectedDetail == null) { return Color.white; }
                return SelectedDetail.healthyColor;
            }
            set
            {
                if (SelectedDetail == null) { return; }
                SelectedDetail.healthyColor = value;
                Refresh(Terrain, SelectedIndex, SelectedDetail);
            }
        }

        public float MaxHeight
        {
            get
            {
                if (SelectedDetail == null) { return 0.0f; }
                return SelectedDetail.maxHeight;
            }
            set
            {
                if (SelectedDetail == null) { return; }
                SelectedDetail.maxHeight = value;
                Refresh(Terrain, SelectedIndex, SelectedDetail);
            }
        }

        public float MaxWidth
        {
            get
            {
                if (SelectedDetail == null) { return 0.0f; }
                return SelectedDetail.maxWidth;
            }
            set
            {
                if (SelectedDetail == null) { return; }
                SelectedDetail.maxWidth = value;
                Refresh(Terrain, SelectedIndex, SelectedDetail);
            }
        }

        public float MinHeight
        {
            get
            {
                if (SelectedDetail == null) { return 0.0f; }
                return SelectedDetail.minHeight;
            }
            set
            {
                if (SelectedDetail == null) { return; }
                SelectedDetail.minHeight = value;
                Refresh(Terrain, SelectedIndex, SelectedDetail);
            }
        }

        public float MinWidth
        {
            get
            {
                if (SelectedDetail == null) { return 0.0f; }
                return SelectedDetail.minWidth;
            }
            set
            {
                if (SelectedDetail == null) { return; }
                SelectedDetail.minWidth = value;
                Refresh(Terrain, SelectedIndex, SelectedDetail);
            }
        }

        public float NoiseSpread
        {
            get
            {
                if (SelectedDetail == null) { return 0.0f; }
                return SelectedDetail.noiseSpread;
            }
            set
            {
                if (SelectedDetail == null) { return; }
                SelectedDetail.noiseSpread = value;
                Refresh(Terrain, SelectedIndex, SelectedDetail);
            }
        }

        private Texture2D m_initialDetailTexture;
        private GameObject m_initialDetail;
        private DetailRenderMode m_initialRenderMode;
        private float m_initialBendFactor;
        private Color m_initialDryColor;
        private Color m_initialHealthyColor;
        private float m_initialMaxHeight;
        private float m_initialMaxWidth;
        private float m_initialMinHeight;
        private float m_initialMinWidth;
        private float m_initialNoiseSpread;

        private void BeginRecordDetailProperties()
        {
            m_initialDetailTexture = DetailTexture;
            m_initialDetail = Detail;
            m_initialRenderMode = RenderMode;
            m_initialBendFactor = BendFactor;
            m_initialDryColor = DryColor;
            m_initialHealthyColor = HealthyColor;
            m_initialMaxHeight = MaxHeight;
            m_initialMaxWidth = MaxWidth;
            m_initialMinHeight = MinHeight;
            m_initialMinWidth = MinWidth;
            m_initialNoiseSpread = NoiseSpread;
        }

        private void EndRecordDetailProperties()
        {
            IRTE editor = IOC.Resolve<IRTE>();
            Terrain terrain = Terrain;
            int index = m_detailsList.SelectedIndex;

            Texture2D detailTexture = DetailTexture;
            GameObject detail = Detail;
            DetailRenderMode renderMode = RenderMode;
            float bendFactor = BendFactor;
            Color dryColor = DryColor;
            Color healthyColor = HealthyColor;
            float maxHeight = MaxHeight;
            float maxWidth = MaxWidth;
            float minHeight = MinHeight;
            float minWidth = MinWidth;
            float noiseSpread = NoiseSpread;

            m_detailsList.DataBindVisible();
            
            editor.Undo.CreateRecord(record =>
            {
                DetailPrototype selectedDetail = terrain.terrainData.detailPrototypes[index];
                if (selectedDetail != null)
                {
                    selectedDetail.prototypeTexture = detailTexture;
                    selectedDetail.prototype = detail;
                    selectedDetail.renderMode = renderMode;
#if !UNITY_2020_1_OR_NEWER
                    selectedDetail.bendFactor = bendFactor;
#endif
                    selectedDetail.dryColor = dryColor;
                    selectedDetail.healthyColor = healthyColor;
                    selectedDetail.maxHeight = maxHeight;
                    selectedDetail.maxWidth = maxWidth;
                    selectedDetail.minHeight = minHeight;
                    selectedDetail.minWidth = minWidth;
                    selectedDetail.noiseSpread = noiseSpread;
                    Refresh(terrain, index, selectedDetail);

                    if (m_detailsList != null)
                    {
                        m_detailsList.Items = terrain.terrainData.detailPrototypes.Select(p => new DetailPrototypeWrapper(p));
                        m_detailsList.SelectedIndex = index;
                    }
                }
                return true;
            },
           record =>
           {
               DetailPrototype selectedDetail = terrain.terrainData.detailPrototypes[index];
               if (selectedDetail != null)
               {
                   selectedDetail.prototypeTexture = m_initialDetailTexture;
                   selectedDetail.prototype = m_initialDetail;
                   selectedDetail.renderMode = m_initialRenderMode;
#if !UNITY_2020_1_OR_NEWER
                   selectedDetail.bendFactor = m_initialBendFactor;
#endif
                   selectedDetail.dryColor = m_initialDryColor;
                   selectedDetail.healthyColor = m_initialHealthyColor;
                   selectedDetail.maxHeight = m_initialMaxHeight;
                   selectedDetail.maxWidth = m_initialMaxWidth;
                   selectedDetail.minHeight = m_initialMinHeight;
                   selectedDetail.minWidth = m_initialMinWidth;
                   selectedDetail.noiseSpread = m_initialNoiseSpread;
                   Refresh(terrain, index, selectedDetail); 

                   if (m_detailsList != null)
                   {
                       m_detailsList.Items = terrain.terrainData.detailPrototypes.Select(p => new DetailPrototypeWrapper(p));
                       m_detailsList.SelectedIndex = index;
                   }
               }

               return true;
           });
        }

        private void Refresh(Terrain terrain, int index, DetailPrototype detail)
        {
            DetailPrototype[] details = terrain.terrainData.detailPrototypes;
            details[index] = detail;
            terrain.terrainData.detailPrototypes = details;
        }

        private void Awake()
        {
            if (m_createDetail != null) m_createDetail.onClick.AddListener(OnCreateDetail);
            if (m_removeDetail != null) m_removeDetail.onClick.AddListener(OnRemoveDetail);

            if (m_detailsList != null)
            {
                m_detailsList.SelectionChanged += OnDetailsSelectionChanged;
                m_detailsList.ItemDataBinding += OnDetailsDatabinding;
                m_detailsList.CanDrag = false;
                m_detailsList.CanEdit = false;
                m_detailsList.CanRemove = false;
                m_detailsList.CanReorder = false;
                m_detailsList.CanReparent = false;
                m_detailsList.CanSelectAll = false;
            }

            ILocalization lc = IOC.Resolve<ILocalization>();
            if (m_detailTextureEditor != null) m_detailTextureEditor.Init(this, this, Strong.PropertyInfo((TerrainDetailEditor x) => x.DetailTexture), null, lc.GetString("ID_RTTerrain_TerrainDetailEditor_DetailTexture", "Detail Texture"), null, null, null, false, null, BeginRecordDetailProperties, EndRecordDetailProperties);
            if (m_detailEditor != null) m_detailEditor.Init(this, this, Strong.PropertyInfo((TerrainDetailEditor x) => x.Detail), null, lc.GetString("ID_RTTerrain_TerrainDetailEditor_Detail", "Detail"), null, null, null, false, null, BeginRecordDetailProperties, EndRecordDetailProperties);
            if (m_renderModeEditor != null) m_renderModeEditor.Init(this, this, Strong.PropertyInfo((TerrainDetailEditor x) => x.RenderMode), null, lc.GetString("ID_RTTerrain_TerrainDetailEditor_RenderMode", "Render Mode"), null, null, null, false, null, BeginRecordDetailProperties, EndRecordDetailProperties);
            if (m_bendFactorEditor != null)
            {
#if UNITY_2020_1_OR_NEWER
                m_bendFactorEditor.Init(this, this, Strong.PropertyInfo((TerrainDetailEditor x) => x.BendFactor), null, lc.GetString("ID_RTTerrain_TerrainDetailEditor_BendFactor", "Bend Factor"), null, null, null, false, null, BeginRecordDetailProperties, EndRecordDetailProperties);
#else
                m_bendFactorEditor.gameObject.SetActive(false);
#endif
            }
            if (m_dryColorEditor != null) m_dryColorEditor.Init(this, this, Strong.PropertyInfo((TerrainDetailEditor x) => x.DryColor), null, lc.GetString("ID_RTTerrain_TerrainDetailEditor_DryColor", "Dry Color"), null, null, null, false, null, BeginRecordDetailProperties, EndRecordDetailProperties);
            if (m_healthyColorEditor != null) m_healthyColorEditor.Init(this, this, Strong.PropertyInfo((TerrainDetailEditor x) => x.HealthyColor), null, lc.GetString("ID_RTTerrain_TerrainDetailEditor_HealthyColor", "Healthy Color"), null, null, null, false, null, BeginRecordDetailProperties, EndRecordDetailProperties);
            if (m_maxHeightEditor != null) m_maxHeightEditor.Init(this, this, Strong.PropertyInfo((TerrainDetailEditor x) => x.MaxHeight), null, lc.GetString("ID_RTTerrain_TerrainDetailEditor_MaxHeight", "Max Height"), null, null, null, false, null, BeginRecordDetailProperties, EndRecordDetailProperties);
            if (m_maxWidthEditor != null) m_maxWidthEditor.Init(this, this, Strong.PropertyInfo((TerrainDetailEditor x) => x.MaxWidth), null, lc.GetString("ID_RTTerrain_TerrainDetailEditor_MaxWidth", "Max Width"), null, null, null, false, null, BeginRecordDetailProperties, EndRecordDetailProperties);
            if (m_minHeightEditor != null) m_minHeightEditor.Init(this, this, Strong.PropertyInfo((TerrainDetailEditor x) => x.MinHeight), null, lc.GetString("ID_RTTerrain_TerrainDetailEditor_MinHeight", "Min Height"), null, null, null, false, null, BeginRecordDetailProperties, EndRecordDetailProperties);
            if (m_minWidthEditor != null) m_minWidthEditor.Init(this, this, Strong.PropertyInfo((TerrainDetailEditor x) => x.MinWidth), null, lc.GetString("ID_RTTerrain_TerrainDetailEditor_MinWidth", "Min Width"), null, null, null, false, null, BeginRecordDetailProperties, EndRecordDetailProperties);
            if (m_noiseSpreadEditor != null) m_noiseSpreadEditor.Init(this, this, Strong.PropertyInfo((TerrainDetailEditor x) => x.NoiseSpread), null, lc.GetString("ID_RTTerrain_TerrainDetailEditor_NoiseSpread", "Noise Spread"), null, null, null, false, null, BeginRecordDetailProperties, EndRecordDetailProperties);
        }

        private void Start()
        {
            if(m_detailsList != null)
            {
                if(TerrainData != null)
                {
                    m_detailsList.Items = TerrainData.detailPrototypes.Select(p => new DetailPrototypeWrapper(p)); 
                }
                
                UpdateVisualState();
            }
        }

        private void OnDestroy()
        {
            if (m_detailsList != null)
            {
                m_detailsList.SelectionChanged -= OnDetailsSelectionChanged;
                m_detailsList.ItemDataBinding -= OnDetailsDatabinding;
            }

            if(m_createDetail != null) m_createDetail.onClick.RemoveListener(OnCreateDetail);
            if(m_removeDetail != null) m_removeDetail.onClick.RemoveListener(OnRemoveDetail);
        }

        private void OnDetailsDatabinding(object sender, VirtualizingTreeViewItemDataBindingArgs e)
        {
            DetailPrototypeWrapper details = (DetailPrototypeWrapper)e.Item;
            RawImage image = e.ItemPresenter.GetComponentInChildren<RawImage>();
            if(image != null)
            {
                if(image.texture != null && image.texture.name == "TemporaryPreview")
                {
                    Destroy(image.texture);
                }

                if(details.Prototype.usePrototypeMesh && details.Prototype.prototype != null)
                {
                    IResourcePreviewUtility previewUtil = IOC.Resolve<IResourcePreviewUtility>();
                    Texture2D texture = new Texture2D(1, 1, TextureFormat.ARGB32, true);
                    texture.name = "TemporaryPreview";
                    texture.LoadImage(previewUtil.CreatePreviewData(details.Prototype.prototype));
                    image.texture = texture;
                }
                else
                {
                    image.texture = details.Prototype.prototypeTexture;
                }
            }
        }

        private void OnDetailsSelectionChanged(object sender, SelectionChangedArgs e)
        {
            UpdateVisualState();

            if (SelectedDetailChanged != null)
            {
                SelectedDetailChanged(this, EventArgs.Empty);
            }
        }

        private void UpdateVisualState()
        {
            if (m_removeDetail != null)
            {
                m_removeDetail.interactable = m_detailsList.SelectedItem != null;
            }

            if(m_detailPropertiesGroup != null)
            {
                m_detailPropertiesGroup.interactable = m_detailsList.SelectedItem != null;
            }
        }

        private void OnCreateDetail()
        {
            DetailPrototype[] oldDetails = TerrainData.detailPrototypes.ToArray();

            DetailPrototype detail = new DetailPrototype();
            detail.renderMode = DetailRenderMode.Grass;
            detail.dryColor = Color.white;
            detail.healthyColor = Color.white;

            List<DetailPrototype> newDetails = TerrainData.detailPrototypes.ToList();
            newDetails.Add(detail);
            TerrainData.detailPrototypes = newDetails.ToArray();
            DetailPrototypeWrapper wrapper = new DetailPrototypeWrapper(detail);
            m_detailsList.Add(wrapper);
            m_detailsList.SelectedItem = wrapper;

            RecordState(oldDetails);
        }

        private void OnRemoveDetail()
        {
            DetailPrototype[] oldDetails = TerrainData.detailPrototypes.ToArray();

            List<DetailPrototype> details = TerrainData.detailPrototypes.ToList();
            DetailPrototype selectedDetail = ((DetailPrototypeWrapper)m_detailsList.SelectedItem).Prototype;
            details.Remove(selectedDetail);
            TerrainData.detailPrototypes = details.ToArray();
            m_detailsList.RemoveSelectedItems();

            RecordState(oldDetails);

            UpdateVisualState();

            if (SelectedDetailChanged != null)
            {
                SelectedDetailChanged(this, EventArgs.Empty);
            }
        }

        private void RecordState(DetailPrototype[] oldDetails)
        {
            Terrain terrain = Terrain;

            DetailPrototype[] newDetails = terrain.terrainData.detailPrototypes.ToArray();

            IRTE editor = IOC.Resolve<IRTE>();
            editor.Undo.CreateRecord(record =>
            {
                if(terrain.terrainData != null)
                {
                    terrain.terrainData.detailPrototypes = newDetails;
                    UpdateDetailsList(terrain);
                }

                return true;
            },
            record =>
            {
                if(terrain.terrainData != null)
                {
                    terrain.terrainData.detailPrototypes = oldDetails;
                    UpdateDetailsList(terrain);
                }
                
                return true;
            });   
        }

        private void UpdateDetailsList(Terrain terrain)
        {
            VirtualizingTreeView detailsList = m_detailsList;
            detailsList.Items = terrain.terrainData.detailPrototypes;
            if (detailsList.Items == null || !detailsList.Items.OfType<DetailPrototype>().Contains(detailsList.SelectedItem))
            {
                detailsList.SelectedItem = null;
            }
        }
    }
}

