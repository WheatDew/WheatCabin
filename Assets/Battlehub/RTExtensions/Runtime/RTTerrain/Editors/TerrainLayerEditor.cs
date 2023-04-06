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
    public class TerrainLayerEditor : MonoBehaviour
    {
        public event EventHandler SelectedLayerChanged;

        [SerializeField]
        private VirtualizingTreeView m_layersList = null;

        [SerializeField]
        private Button m_createLayer = null;

        [SerializeField]
        private Button m_replaceLayer = null;

        [SerializeField]
        private Button m_removeLayer = null;

        [SerializeField]
        private CanvasGroup m_tileEditorGroup = null;

        [SerializeField]
        private Vector2Editor m_tileSizeEditor = null;

        [SerializeField]
        private Vector2Editor m_tileOffsetEditor = null;

        [SerializeField]
        private string m_selectTextureWindow = RuntimeWindowType.SelectObject.ToString();
        public string SelectTextureWindowName
        {
            get { return m_selectTextureWindow; }
            set { m_selectTextureWindow = value; }
        }

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
                if (m_terrainData != value)
                {
                    m_terrainData = value;
                    if (m_layersList != null)
                    {
                        if (m_terrainData != null)
                        {
                            m_layersList.Items = m_terrainData.terrainLayers;
                        }
                        else
                        {
                            m_layersList.Items = null;
                        }

                        UpdateVisualState();
                    }
                }
            }
        }

        public TerrainLayer SelectedLayer
        {
            get
            {
                if (m_layersList.SelectedItem == null)
                {
                    return null;
                }

                return (TerrainLayer)m_layersList.SelectedItem;
            }
            set
            {
                m_layersList.SelectedItem = value;
            }
        }

        public int DefaultSelectedIndex
        {
            private get;
            set;
        }

        public Vector2 TileSize
        {
            get
            {
                if (SelectedLayer == null)
                {
                    return Vector2.zero;
                }
                return SelectedLayer.tileSize;
            }
            set
            {
                if (SelectedLayer != null)
                {
                    SelectedLayer.tileSize = value;
                }


            }
        }

        public Vector2 TileOffset
        {
            get
            {
                if (SelectedLayer == null)
                {
                    return Vector2.zero;
                }
                return SelectedLayer.tileOffset;
            }
            set
            {
                if (SelectedLayer != null)
                {
                    SelectedLayer.tileOffset = value;
                }
            }
        }

        private Vector2 m_initialTileSize;
        private Vector2 m_initialTileOffset;
        private void BeginRecordLayerProperties()
        {
            m_initialTileOffset = TileOffset;
            m_initialTileSize = TileSize;
        }

        private void EndRecordLayerProperties()
        {
            IRTE editor = IOC.Resolve<IRTE>();
            Terrain terrain = Terrain;
            int layerIndex = m_layersList.SelectedIndex;

            Vector2 tileSize = SelectedLayer.tileSize;
            Vector2 tileOffset = SelectedLayer.tileOffset;

            editor.Undo.CreateRecord(record =>
            {
                TerrainLayer layer = terrain.terrainData.terrainLayers[layerIndex];
                if(layer != null)
                {
                    layer.tileOffset = tileOffset;
                    layer.tileSize = tileSize;
                }
                return true;
            },
           record =>
           {
               TerrainLayer layer = terrain.terrainData.terrainLayers[layerIndex];
               if (layer != null)
               {
                   layer.tileOffset = m_initialTileOffset;
                   layer.tileSize = m_initialTileSize;
               }

               return true;
           });
        }

        private ILocalization m_localization;

        private void Awake()
        {
            m_localization = IOC.Resolve<ILocalization>();

            if (m_createLayer != null) m_createLayer.onClick.AddListener(OnCreateLayer);
            if (m_replaceLayer != null) m_replaceLayer.onClick.AddListener(OnReplaceLayer);
            if (m_removeLayer != null) m_removeLayer.onClick.AddListener(OnRemoveLayer);

            if (m_layersList != null)
            {
                m_layersList.SelectionChanged += OnLayersSelectionChanged;
                m_layersList.ItemDataBinding += OnLayersDataBinding;
                m_layersList.CanDrag = false;
                m_layersList.CanEdit = false;
                m_layersList.CanRemove = false;
                m_layersList.CanReorder = false;
                m_layersList.CanReparent = false;
                m_layersList.CanSelectAll = false;
            }

            if (m_tileSizeEditor != null) m_tileSizeEditor.Init(this, this, Strong.PropertyInfo((TerrainLayerEditor x) => x.TileSize), null, m_localization.GetString("ID_RTTerrain_TerrainLayerEditor_TileSize", "Tile Size"), null, null, null, false, null, BeginRecordLayerProperties, EndRecordLayerProperties);
            if (m_tileOffsetEditor != null) m_tileOffsetEditor.Init(this, this, Strong.PropertyInfo((TerrainLayerEditor x) => x.TileOffset), null, m_localization.GetString("ID_RTTerrain_TerrainLayerEditor_TileOffset", "Tile Offset"), null, null, null, false, null, BeginRecordLayerProperties, EndRecordLayerProperties);

        }

        private void Start()
        {
            if(m_layersList != null)
            {
                if(TerrainData != null)
                {
                    m_layersList.Items = TerrainData.terrainLayers;
                    int len = TerrainData.terrainLayers.Length;
                    if (len > 0)
                    {
                        if (DefaultSelectedIndex < len)
                        {
                            m_layersList.SelectedIndex = DefaultSelectedIndex;
                        }
                        else
                        {
                            m_layersList.SelectedIndex = 0;
                        }
                    }
                }
                
                UpdateVisualState();
            }
        }

        private void OnDestroy()
        {
            if(m_layersList != null)
            {
                m_layersList.SelectionChanged -= OnLayersSelectionChanged;
                m_layersList.ItemDataBinding -= OnLayersDataBinding;
            }

            if(m_createLayer != null) m_createLayer.onClick.RemoveListener(OnCreateLayer);
            if(m_replaceLayer != null) m_replaceLayer.onClick.RemoveListener(OnReplaceLayer);
            if(m_removeLayer != null) m_removeLayer.onClick.RemoveListener(OnRemoveLayer);
        }

        private void OnEnable()
        {
            IOC.Register("TerrainLayerEditor", this);
        }

        private void OnDisable()
        {
            IOC.Unregister("TerrainLayerEditor", this);
        }

        private void OnLayersDataBinding(object sender, VirtualizingTreeViewItemDataBindingArgs e)
        {
            TerrainLayer layer = (TerrainLayer)e.Item;
            RawImage image = e.ItemPresenter.GetComponentInChildren<RawImage>();
            if(image != null)
            {
                image.texture = layer.diffuseTexture;
            }
        }

        private void OnLayersSelectionChanged(object sender, SelectionChangedArgs e)
        {
            UpdateVisualState();

            if (SelectedLayerChanged != null)
            {
                SelectedLayerChanged(this, EventArgs.Empty);
            }
        }

        private void UpdateVisualState()
        {
            if (m_replaceLayer != null)
            {
                m_replaceLayer.interactable = m_layersList.SelectedItem != null;
            }

            if (m_removeLayer != null)
            {
                m_removeLayer.interactable = m_layersList.SelectedItem != null;
            }

            if(m_tileEditorGroup != null)
            {
                m_tileEditorGroup.interactable = m_layersList.SelectedItem != null;
            }
        }

        private void OnCreateLayer()
        {
            SelectTexture(true);
        }

        private void OnReplaceLayer()
        {
            SelectTexture(false);
        }

        private void OnRemoveLayer()
        {
            float[,,] oldAlphamaps = GetAlphamaps();
            TerrainLayer[] oldLayers = TerrainData.terrainLayers.ToArray();

            List<TerrainLayer> layers = TerrainData.terrainLayers.ToList();
            TerrainLayer selectedLayer = (TerrainLayer)m_layersList.SelectedItem;
            layers.Remove(selectedLayer);
            TerrainData.terrainLayers = layers.ToArray();
            m_layersList.RemoveSelectedItems();

            RecordState(oldAlphamaps, oldLayers);

            UpdateVisualState();

            if (SelectedLayerChanged != null)
            {
                SelectedLayerChanged(this, EventArgs.Empty);
            }
        }

        private void SelectTexture(bool create)
        {
            ISelectObjectDialog objectSelector = null;
            Transform dialogTransform = IOC.Resolve<IWindowManager>().CreateDialogWindow(m_selectTextureWindow, "Select Texture", 
                 (sender, args) =>
                 {
                     if(!objectSelector.IsNoneSelected)
                     {
                         OnTextureSelected((Texture2D)objectSelector.SelectedObject, create);
                     }
                 });
            objectSelector = IOC.Resolve<ISelectObjectDialog>();
            objectSelector.ObjectType = typeof(Texture2D);
        }

        private void OnTextureSelected(Texture2D texture, bool create)
        {
            float[,,] oldAlphamaps = GetAlphamaps();
            TerrainLayer[] oldLayers = TerrainData.terrainLayers.ToArray();

            TerrainLayer layer;
            if(create)
            {
                layer = new TerrainLayer() { name = "TerrainLayer" };
                layer.diffuseTexture = texture;
                                
                List<TerrainLayer> layers = TerrainData.terrainLayers.ToList();
                layers.Add(layer);
                TerrainData.terrainLayers = layers.ToArray();

                m_layersList.Add(layer);

                if(layers.Count == 1)
                {
                    float[,,] alphaMaps = TerrainData.GetAlphamaps(0, 0, TerrainData.alphamapWidth, TerrainData.alphamapHeight);
                    int amapY = alphaMaps.GetLength(0);
                    int amapX = alphaMaps.GetLength(1);

                    for (int y = 0; y < amapY; y++)
                    {
                        for (int x = 0; x < amapX; x++)
                        {
                            alphaMaps[y, x, 0] = 1;
                        }
                    }

                    Terrain.SetAlphamaps(0, 0, alphaMaps);
                }
            }
            else
            {
                layer = (TerrainLayer)m_layersList.SelectedItem;
                layer.diffuseTexture = texture;

                m_layersList.DataBindItem(layer);
            }

            RecordState(oldAlphamaps, oldLayers);
        }

        private float[,,] GetAlphamaps()
        {
            int w = Terrain.terrainData.alphamapWidth;
            int h = Terrain.terrainData.alphamapHeight;
            return Terrain.terrainData.GetAlphamaps(0, 0, w, h);
        }

        private void RecordState(float[,,] oldAlphamap, TerrainLayer[] oldLayers)
        {
            Terrain terrain = Terrain;

            float[,,] newAlphamap = GetAlphamaps();
            TerrainLayer[] newLayers = terrain.terrainData.terrainLayers.ToArray();

            IRTE editor = IOC.Resolve<IRTE>();
            editor.Undo.CreateRecord(record =>
            {
                if(terrain.terrainData != null)
                {
                    terrain.terrainData.terrainLayers = newLayers;
                    terrain.SetAlphamaps(0, 0, newAlphamap);
                    UpdateLayersList(terrain);
                }

                return true;
            },
            record =>
            {
                if(terrain.terrainData != null)
                {
                    terrain.terrainData.terrainLayers = oldLayers;
                    terrain.SetAlphamaps(0, 0, oldAlphamap);
                    UpdateLayersList(terrain);
                }
                
                return true;
            });

            
        }

        public static void UpdateLayersList(Terrain terrain)
        {
            TerrainLayerEditor layerEditor = IOC.Resolve<TerrainLayerEditor>("TerrainLayerEditor");
            if (layerEditor != null)
            {
                VirtualizingTreeView layerList = layerEditor.m_layersList;
                layerList.Items = terrain.terrainData.terrainLayers;
                if (layerList.Items == null || !layerList.Items.OfType<TerrainLayer>().Contains(layerList.SelectedItem))
                {
                    layerList.SelectedItem = null;
                }
            }
        }
    }
}

