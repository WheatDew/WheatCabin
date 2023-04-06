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
    public class TerrainTreeEditor : MonoBehaviour
    {
        public event EventHandler SelectedTreeChanged;

        [SerializeField]
        private VirtualizingTreeView m_treeList = null;

        [SerializeField]
        private Button m_createTree = null;

        [SerializeField]
        private Button m_removeTree = null;

        [SerializeField]
        private CanvasGroup m_treePropertiesGroup = null;

        [SerializeField]
        private ObjectEditor m_treeEditor = null;

        [SerializeField]
        private FloatEditor m_bendFactorEditor = null;

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
                    if (m_treeList != null)
                    {
                        if(m_terrainData != null)
                        {
                            m_treeList.Items = m_terrainData.treePrototypes;
                        }
                        else
                        {
                            m_treeList.Items = null;
                        }
                        
                        UpdateVisualState();
                    }
                }
            }
        }

        public int SelectedIndex
        {
            get { return m_treeList.SelectedIndex; }
        }


        public TreePrototype SelectedTree
        {
            get
            {
                if(m_treeList.SelectedItem == null)
                {
                    return null;
                }

                return (TreePrototype)m_treeList.SelectedItem;
            }
        }

        public GameObject Prefab
        {
            get
            {
                if (SelectedTree == null) { return null; }
                return SelectedTree.prefab;
            }
            set
            {
                if (SelectedTree == null) { return; }
                SelectedTree.prefab = value;                
                Refresh(Terrain, SelectedIndex, SelectedTree);
            }
        }

        public float BendFactor
        {
            get
            {
                if (SelectedTree == null) { return 0.0f; }
                return SelectedTree.bendFactor;
            }
            set
            {
                if (SelectedTree == null) { return; }
                SelectedTree.bendFactor = value;
                Refresh(Terrain, SelectedIndex, SelectedTree);
            }
        }


        private GameObject m_initialPrefab;
        private float m_initialBendFactor;
        
        private void BeginRecordDetailProperties()
        {
            m_initialPrefab = Prefab;
            m_initialBendFactor = BendFactor;
        }

        private void EndRecordDetailProperties()
        {
            IRTE editor = IOC.Resolve<IRTE>();
            Terrain terrain = Terrain;
            int index = m_treeList.SelectedIndex;
            GameObject prefab = Prefab;
            float bendFactor = BendFactor;
            m_treeList.DataBindVisible();
            
            editor.Undo.CreateRecord(record =>
            {
                TreePrototype selectedTree = terrain.terrainData.treePrototypes[index];
                if (selectedTree != null)
                {                    
                    selectedTree.prefab = prefab;
                    selectedTree.bendFactor = bendFactor;
                    
                    Refresh(terrain, index, selectedTree);

                    if (m_treeList != null)
                    {
                        m_treeList.Items = terrain.terrainData.treePrototypes;
                        m_treeList.SelectedIndex = index;
                    }
                }
                return true;
            },
           record =>
           {
               TreePrototype selectedTree = terrain.terrainData.treePrototypes[index];
               if (selectedTree != null)
               {
                   selectedTree.prefab = m_initialPrefab;
                   selectedTree.bendFactor = m_initialBendFactor;
         
                   Refresh(terrain, index, selectedTree); 

                   if (m_treeList != null)
                   {
                       m_treeList.Items = terrain.terrainData.detailPrototypes;
                       m_treeList.SelectedIndex = index;
                   }
               }

               return true;
           });
        }

        private void Refresh(Terrain terrain, int index, TreePrototype detail)
        {
            TreePrototype[] trees = terrain.terrainData.treePrototypes;
            trees[index] = detail;
            terrain.terrainData.treePrototypes = trees;
        }

        private void Awake()
        {
            if (m_createTree != null) m_createTree.onClick.AddListener(OnCreateTree);
            if (m_removeTree != null) m_removeTree.onClick.AddListener(OnRemoveTree);

            if (m_treeList != null)
            {
                m_treeList.SelectionChanged += OnTreesSelectionChanged;
                m_treeList.ItemDataBinding += OnTreesDatabinding;
                m_treeList.CanDrag = false;
                m_treeList.CanEdit = false;
                m_treeList.CanRemove = false;
                m_treeList.CanReorder = false;
                m_treeList.CanReparent = false;
                m_treeList.CanSelectAll = false;
            }

            ILocalization lc = IOC.Resolve<ILocalization>();
            if (m_treeEditor != null) m_treeEditor.Init(this, this, Strong.PropertyInfo((TerrainTreeEditor x) => x.Prefab), null, lc.GetString("ID_RTTerrain_TerrainTreeEditor_Prefab", "Prefab"), null, null, null, false, null, BeginRecordDetailProperties, EndRecordDetailProperties);
            if (m_bendFactorEditor != null) m_bendFactorEditor.Init(this, this, Strong.PropertyInfo((TerrainTreeEditor x) => x.BendFactor), null, lc.GetString("ID_RTTerrain_TerrainTreeEditor_BendFactor", "Bend Factor"), null, null, null, false, null, BeginRecordDetailProperties, EndRecordDetailProperties);
         
        }

        private void Start()
        {
            if(m_treeList != null)
            {
                if(TerrainData != null)
                {
                    m_treeList.Items = TerrainData.treePrototypes;
                }
                
                UpdateVisualState();
            }
        }

        private void OnDestroy()
        {
            if (m_treeList != null)
            {
                m_treeList.SelectionChanged -= OnTreesSelectionChanged;
                m_treeList.ItemDataBinding -= OnTreesDatabinding;
            }

            if(m_createTree != null) m_createTree.onClick.RemoveListener(OnCreateTree);
            if(m_removeTree != null) m_removeTree.onClick.RemoveListener(OnRemoveTree);
        }

        private void OnTreesDatabinding(object sender, VirtualizingTreeViewItemDataBindingArgs e)
        {
            TreePrototype tree = (TreePrototype)e.Item;
            RawImage image = e.ItemPresenter.GetComponentInChildren<RawImage>();
            if(image != null)
            {
                if(image.texture != null && image.texture.name == "TemporaryPreview")
                {
                    Destroy(image.texture);
                }

                if(tree.prefab != null)
                {
                    IResourcePreviewUtility previewUtil = IOC.Resolve<IResourcePreviewUtility>();
                    Texture2D texture = new Texture2D(1, 1, TextureFormat.ARGB32, true);
                    texture.name = "TemporaryPreview";
                    texture.LoadImage(previewUtil.CreatePreviewData(tree.prefab));
                    image.texture = texture;
                }
            }
        }

        private void OnTreesSelectionChanged(object sender, SelectionChangedArgs e)
        {
            UpdateVisualState();

            if (SelectedTreeChanged != null)
            {
                SelectedTreeChanged(this, EventArgs.Empty);
            }
        }

        private void UpdateVisualState()
        {
            if (m_removeTree != null)
            {
                m_removeTree.interactable = m_treeList.SelectedItem != null;
            }

            if(m_treePropertiesGroup != null)
            {
                m_treePropertiesGroup.interactable = m_treeList.SelectedItem != null;
            }
        }

        private void OnCreateTree()
        {
            TreePrototype[] oldTrees = TerrainData.treePrototypes.ToArray();

            TreePrototype tree = new TreePrototype();

            tree.prefab =  Resources.Load<GameObject>("Tree/RTT_DefaultTree");
            if (RenderPipelineInfo.Type == RPType.URP)
            {
                GameObject prefab = Resources.Load<GameObject>("Tree/RTT_DefaultTree_URP");
                if (prefab != null)
                {
                    tree.prefab = prefab;
                }
            }

            List<TreePrototype> newTrees = TerrainData.treePrototypes.ToList();
            newTrees.Add(tree);
            TerrainData.treePrototypes = newTrees.ToArray();
            m_treeList.Add(tree);
            m_treeList.SelectedItem = tree;

            RecordState(oldTrees);
        }

        private void OnRemoveTree()
        {
            TreePrototype[] oldTrees = TerrainData.treePrototypes.ToArray();

            List<TreePrototype> newTrees = TerrainData.treePrototypes.ToList();
            TreePrototype selectedTree = (TreePrototype)m_treeList.SelectedItem;
            newTrees.Remove(selectedTree);
            TerrainData.treePrototypes = newTrees.ToArray();
            m_treeList.RemoveSelectedItems();

            RecordState(oldTrees);

            UpdateVisualState();

            if (SelectedTreeChanged != null)
            {
                SelectedTreeChanged(this, EventArgs.Empty);
            }
        }

        private void RecordState(TreePrototype[] oldTrees)
        {
            Terrain terrain = Terrain;

            TreePrototype[] newTrees = terrain.terrainData.treePrototypes.ToArray();

            IRTE editor = IOC.Resolve<IRTE>();
            editor.Undo.CreateRecord(record =>
            {
                if(terrain.terrainData != null)
                {
                    terrain.terrainData.treePrototypes = newTrees;
                    UpdateTreeList(terrain);
                }

                return true;
            },
            record =>
            {
                if(terrain.terrainData != null)
                {
                    terrain.terrainData.treePrototypes = oldTrees;
                    UpdateTreeList(terrain);
                }
                
                return true;
            });   
        }

        private void UpdateTreeList(Terrain terrain)
        {
            VirtualizingTreeView treeList = m_treeList;
            treeList.Items = terrain.terrainData.treePrototypes;
            if (treeList.Items == null || !treeList.Items.OfType<TreePrototype>().Contains(treeList.SelectedItem))
            {
                treeList.SelectedItem = null;
            }
        }
    }
}

