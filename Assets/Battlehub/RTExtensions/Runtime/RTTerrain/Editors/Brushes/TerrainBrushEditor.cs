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
    public class TerrainBrushEditor : MonoBehaviour
    {
        public event EventHandler SelectedBrushChanged;
        public event EventHandler BrushParamsChanged;
        public event EventHandler BrushCreated;
        public event EventHandler BrushDestroying;
        public event EventHandler BrushDestroyed;

        [SerializeField]
        private VirtualizingTreeView m_brushesList = null;

        [SerializeField]
        private Image m_brushPreview = null;

        [SerializeField]
        private RangeEditor m_brushSizeEditor = null;

        [SerializeField]
        private RangeIntEditor m_opacityEditor = null;

        [SerializeField]
        private GameObject m_buttonsPanel = null;

        [SerializeField]
        private Button m_beginCreateButton = null;

        [SerializeField]
        private GameObject m_createBrushEditor = null;

        [SerializeField]
        private Button m_createButton = null;

        [SerializeField]
        private Button m_cancelCreateButton = null;

        [SerializeField]
        private Button m_addButton = null;

        [SerializeField]
        private Button m_deleteButton = null;

        [SerializeField]
        private string m_selectTextureWindow = RuntimeWindowType.SelectObject.ToString();

        private float m_brushSize = 2.5f;
        public float BrushSize
        {
            get { return m_brushSize; }
            set
            {
                if(m_brushSize != value)
                {
                    m_brushSize = value;
                    RaiseBrushParamsChanged();
                }
            }
        }

        private int m_brushOpacity = 100;
        public int BrushOpacity
        {
            get { return m_brushOpacity; }
            set
            {
                if(m_brushOpacity != value)
                {
                    m_brushOpacity = value;
                    RaiseBrushParamsChanged();
                }
            }
        }

        public Sprite SelectedBrush
        {
            get { return (Sprite)m_brushesList.SelectedItem; }
        }

        public int SelectedBrushIndex
        {
            get { return PlayerPrefs.GetInt("Battlehub.RTTerrain.TerrainBrushEditor", 0); }
            set { PlayerPrefs.SetInt("Battlehub.RTTerrain.TerrainBrushEditor", value); }
        }

        public bool IsBuiltInBrush
        {
            get { return m_source.IsBuiltInBrush(SelectedBrush); }
        }

        private TerrainBrushSource m_source;
        private ITerrainCutoutMaskRenderer m_terrainCutoutRenderer;
        private ICustomSelectionComponent m_customSelection;
        private IRTE m_editor;
        private ILocalization m_localization;
        private TerrainEditor m_terrainEditor;
        
        private void Awake()
        {
            m_terrainEditor = GetComponentInParent<TerrainEditor>();

            m_localization = IOC.Resolve<ILocalization>();
            m_editor = IOC.Resolve<IRTE>();
            m_editor.Selection.SelectionChanged += OnEditorSelectionChanged;
            m_customSelection = IOC.Resolve<ICustomSelectionComponent>();

            m_terrainCutoutRenderer = IOC.Resolve<ITerrainCutoutMaskRenderer>();
            m_terrainCutoutRenderer.ObjectImageLayer = m_editor.CameraLayerSettings.ResourcePreviewLayer;

            m_source = FindObjectOfType<TerrainBrushSource>();
            if(m_source == null)
            {
                m_source = new GameObject("TerrainBrushSource").AddComponent<TerrainBrushSource>();
            }

            if (m_brushesList != null)
            {
                m_brushesList.SelectionChanged += OnBrushesSelectionChanged;
                m_brushesList.ItemDataBinding += OnBrushesDataBinding;
                m_brushesList.CanDrag = false;
                m_brushesList.CanEdit = false;
                m_brushesList.CanRemove = false;
                m_brushesList.CanReorder = false;
                m_brushesList.CanReparent = false;
                m_brushesList.CanSelectAll = false;
                m_brushesList.CanUnselectAll = false;
            }

            if(m_brushSizeEditor != null)
            {
                //BrushSize = 2.5f;
                RaiseBrushParamsChanged();

                m_brushSizeEditor.Min = 0.5f;
                m_brushSizeEditor.Max = 40;
                m_brushSizeEditor.Init(this, this, Strong.MemberInfo((TerrainBrushEditor x) => x.BrushSize), null, m_localization.GetString("ID_RTTerrain_TerrainBrushEditor_BrushSize", "Brush Size"), null, null, null, false);
            }

            if(m_opacityEditor != null)
            {
                //BrushOpacity = 100;
                RaiseBrushParamsChanged();

                m_opacityEditor.Min = 0;
                m_opacityEditor.Max = 100;
                m_opacityEditor.Init(this, this, Strong.MemberInfo((TerrainBrushEditor x) => x.BrushOpacity), null, m_localization.GetString("ID_RTTerrain_TerrainBrushEditor_BrushOpacity", "Brush Opacity"), null, null, null, false);
            }

            if (m_beginCreateButton != null)
            {
                m_beginCreateButton.onClick.AddListener(BeginCreateButtonClick);
            }

            if (m_addButton != null)
            {
                m_addButton.onClick.AddListener(OnAddButtonClick);
            }

            if(m_deleteButton != null)
            {
                m_deleteButton.onClick.AddListener(OnDeleteButtonClick);
            }
        }

        private void Start()
        {
            IEnumerable<Sprite> brushes = m_source.Brushes;
            m_brushesList.Items = brushes;

            int index = SelectedBrushIndex;
            if(index >= m_brushesList.ItemsCount)
            {
                m_brushesList.SelectedItem = brushes.FirstOrDefault();
                SelectedBrushIndex = 0;
            }
            else
            {
                m_brushesList.SelectedItem = brushes.ElementAtOrDefault(index);
            }
        }

        private void OnDisable()
        {
            if (m_customSelection != null)
            {
                m_customSelection.Enabled = false;
                EndCreateBrush();
            }
        }

        private void OnDestroy()
        {
            if(m_editor != null)
            {
                m_editor.Selection.SelectionChanged -= OnEditorSelectionChanged;
            }

            if(m_customSelection != null)
            {
                m_customSelection.Enabled = false;
                EndCreateBrush();
            }

            if (m_brushesList != null)
            {
                m_brushesList.SelectionChanged -= OnBrushesSelectionChanged;
                m_brushesList.ItemDataBinding -= OnBrushesDataBinding;
            }

            if (m_beginCreateButton != null)
            {
                m_beginCreateButton.onClick.RemoveListener(BeginCreateButtonClick);
            }

            if (m_addButton != null)
            {
                m_addButton.onClick.RemoveListener(OnAddButtonClick);
            }

            if (m_deleteButton != null)
            {
                m_deleteButton.onClick.RemoveListener(OnDeleteButtonClick);
            }
        }

        private void Update()
        {
            if (m_createBrushEditor != null && m_createBrushEditor.activeSelf)
            {
                if (m_editor.Input.GetKeyDown(KeyCode.Escape))
                {
                    EndCreateButtonClick();
                }
            }
        }

        private void OnBrushesDataBinding(object sender, VirtualizingTreeViewItemDataBindingArgs e)
        {
            Sprite sprite = (Sprite)e.Item;

            Image image = e.ItemPresenter.GetComponentInChildren<Image>();
            image.sprite = sprite;
        }

        private void OnBrushesSelectionChanged(object sender, SelectionChangedArgs e)
        {
            if (m_brushPreview != null)
            {
                m_brushPreview.sprite = (Sprite)m_brushesList.SelectedItem;
            }

            SelectedBrushIndex = m_brushesList.SelectedIndex;

            if (SelectedBrushChanged != null)
            {
                SelectedBrushChanged(this, EventArgs.Empty);
            }

            UpdateButtonsState();
        }

        private void RaiseBrushParamsChanged()
        {
            if(BrushParamsChanged != null)
            {
                BrushParamsChanged(this, EventArgs.Empty);
            }
        }

        private void UpdateButtonsState()
        {
            if (m_deleteButton != null)
            {
                m_deleteButton.interactable = !m_source.IsBuiltInBrush(SelectedBrush);
            }

            if(m_beginCreateButton != null)
            {
                m_beginCreateButton.interactable = m_editor.Selection.activeGameObject != null;
            }
        }

        private void BeginCreateButtonClick()
        {
            if(m_terrainEditor.Terrain == null || m_terrainEditor.Terrain.terrainData == null)
            {
                return;
            }
            
            if(m_createBrushEditor != null)
            {
                m_createBrushEditor.SetActive(true);
                m_buttonsPanel.SetActive(false);
                m_brushSizeEditor.gameObject.SetActive(false);
                m_opacityEditor.gameObject.SetActive(false);
                UnityEventHelper.AddListener(m_createButton, button => button.onClick, CreateButtonClick);
                UnityEventHelper.AddListener(m_cancelCreateButton, button => button.onClick, EndCreateButtonClick);
                if (m_customSelection != null)
                {
                    m_customSelection.Enabled = true;
                    if(m_terrainEditor.Projector != null)
                    {
                        m_terrainEditor.Projector.gameObject.SetActive(false);
                    }
                }
            }
        }

        private void CreateButtonClick()
        {
            Texture2D texture = m_terrainCutoutRenderer.CreateMask(m_terrainEditor.Terrain.terrainData, m_customSelection.Selection.gameObjects, false);
            CreateBrush(texture);
            Destroy(texture);
            EndCreateButtonClick();
        }

        private void EndCreateButtonClick()
        {
            EndCreateBrush();
            if(m_terrainEditor.Projector != null)
            {
                m_terrainEditor.Projector.gameObject.SetActive(true);
            }
        }

        private void EndCreateBrush()
        {
            if (m_createBrushEditor != null && m_createBrushEditor.activeSelf)
            {
                m_createBrushEditor.SetActive(false);
                m_buttonsPanel.SetActive(true);
                m_brushSizeEditor.gameObject.SetActive(true);
                m_opacityEditor.gameObject.SetActive(true);
                UnityEventHelper.RemoveListener(m_createButton, button => button.onClick, CreateButtonClick);
                UnityEventHelper.RemoveListener(m_cancelCreateButton, button => button.onClick, EndCreateButtonClick);
                if (m_customSelection != null)
                {
                    m_customSelection.Enabled = false;
                }
            }
        }

        private void OnAddButtonClick()
        {
            ISelectObjectDialog objectSelector = null;
            Transform dialogTransform = IOC.Resolve<IWindowManager>().CreateDialogWindow(m_selectTextureWindow, "Select Texture",
                 (sender, args) =>
                 {
                     if (!objectSelector.IsNoneSelected)
                     {
                         CreateBrush((Texture2D)objectSelector.SelectedObject);
                     }
                 });
            objectSelector = IOC.Resolve<ISelectObjectDialog>();
            objectSelector.ObjectType = typeof(Texture2D);
        }

        private void OnDeleteButtonClick()
        {
            BrushDestroying?.Invoke(this, EventArgs.Empty);

            Sprite selectedBrush = SelectedBrush;
            m_source.UserBrushes.Remove(selectedBrush);
            m_brushesList.RemoveSelectedItems();

            Destroy(selectedBrush.texture);
            Destroy(selectedBrush);

            m_brushesList.SelectedIndex = 0;

            BrushDestroyed?.Invoke(this, EventArgs.Empty);
        }

        private void CreateBrush(Texture2D texture)
        {
            Texture2D brushTexture = new Texture2D(texture.width + 2, texture.height + 2, texture.format, false);
            brushTexture.wrapMode = TextureWrapMode.Clamp;

            Color32[] pixels = brushTexture.GetPixels32();
            Color32 c = new Color32(255, 255, 255, 0);
            for(int i = 0; i < pixels.Length; ++i)
            {
                pixels[i] = c;
            }
            brushTexture.SetPixels32(pixels);

            Graphics.CopyTexture(texture, 0, 0, 0, 0, texture.width, texture.height, brushTexture, 0, 0, 1, 1);

            pixels = brushTexture.GetPixels32();
            for (int i = 0; i < pixels.Length; ++i)
            {
                byte a = pixels[i].a;
                pixels[i] = new Color32(255, 255, 255, a);
            }
            brushTexture.SetPixels32(pixels);
            brushTexture.Apply();

            Sprite sprite = Sprite.Create(brushTexture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            m_source.UserBrushes.Add(sprite);
            m_brushesList.Add(sprite);
            m_brushesList.SelectedItem = sprite;
            m_brushesList.ScrollIntoView(sprite);

            BrushCreated?.Invoke(this, EventArgs.Empty);
        }

        private void OnEditorSelectionChanged(UnityEngine.Object[] unselectedObjects)
        {
            UpdateButtonsState();
        }
    }
}

