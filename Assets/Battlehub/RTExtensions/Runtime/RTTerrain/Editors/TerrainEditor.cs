using Battlehub.RTCommon;
using Battlehub.RTEditor;
using Battlehub.RTHandles;
using Battlehub.UIControls;
using Battlehub.Utils;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTTerrain
{
    public delegate void TerrainEvent<T>(T data);
    public delegate void TerrainEvent();

    public static class TerrainDataExt
    {
        public static event TerrainEvent<Terrain> SizeChanged;
        public static event TerrainEvent<Terrain> HeightsChanged;
        public static event TerrainEvent<Terrain> AlphamapsChanged;
        public static event TerrainEvent<Terrain> HolesChanged;
        public static event TerrainEvent<Terrain> DetailsChanged;
        public static event TerrainEvent<Terrain> TreeInstancesChanged;
        public static event TerrainEvent<Terrain> TerrainDataChanged;
        public static event TerrainEvent<Terrain> TerrainModified;

        public static void SetSize(this Terrain terrain, Vector3 size)
        {
            if (terrain.terrainData == null)
            {
                return;
            }

            terrain.terrainData.size = size;
            if(SizeChanged != null)
            {
                SizeChanged(terrain);
            }
            if(TerrainModified != null)
            {
                TerrainModified(terrain);
            }
        }

        public static void SetHeights(this Terrain terrain, int xBase, int yBase, float[,] heights)
        {
            if(terrain.terrainData == null)
            {
                return;
            }

            terrain.terrainData.SetHeights(xBase, yBase, heights);
            if(HeightsChanged != null)
            {
                HeightsChanged(terrain);
            }
            if (TerrainModified != null)
            {
                TerrainModified(terrain);
            }
        }

        public static float[,] GetHeights(this Terrain terrain, int xBase, int yBase, int width, int height)
        {
            if (terrain.terrainData == null)
            {
                return null;
            }

           return terrain.terrainData.GetHeights(xBase, yBase, width, height);  
        }

        public static void SetAlphamaps(this Terrain terrain, int x, int y, float[,,] alphamaps)
        {
            if(terrain.terrainData == null)
            {
                return;
            }

            terrain.terrainData.SetAlphamaps(x, y, alphamaps);
            if(AlphamapsChanged != null)
            {
                AlphamapsChanged(terrain);
            }
            if (TerrainModified != null)
            {
                TerrainModified(terrain);
            }
        }

        public static void SetHoles(this Terrain terrain, int xBase, int yBase, bool[,] holes)
        {
            if (terrain.terrainData == null)
            {
                return;
            }

            terrain.terrainData.SetHoles(xBase, yBase, holes);
            if (HolesChanged != null)
            {
                HolesChanged(terrain);
            }
            if (TerrainModified != null)
            {
                TerrainModified(terrain);
            }
        }

        public static void SetDetails(this Terrain terrain, int xBase, int yBase, int layer, int[,] details)
        {
            if(terrain.terrainData == null)
            {
                return;
            }

            terrain.terrainData.SetDetailLayer(xBase, yBase, layer, details);
            if(DetailsChanged != null)
            {
                DetailsChanged(terrain);
            }

            if (TerrainModified != null)
            {
                TerrainModified(terrain);
            }
        }

        public static void SetTreeInstances(this Terrain terrain, TreeInstance[] instances, bool snapToHeightmap)
        {
            if (terrain.terrainData == null)
            {
                return;
            }

            terrain.terrainData.SetTreeInstances(instances, snapToHeightmap);
            if (TreeInstancesChanged != null)
            {
                TreeInstancesChanged(terrain);
            }

            if (TerrainModified != null)
            {
                TerrainModified(terrain);
            }
        }

        public static void SetTerrainData(this Terrain terrain, TerrainData terrainData)
        {
            terrain.terrainData = terrainData;
            if(TerrainDataChanged != null)
            {
                TerrainDataChanged(terrain);
            }
            if (TerrainModified != null)
            {
                TerrainModified(terrain);
            }
        }

        public static TerrainData CopyTerrainData(this TerrainData terrainData, bool copyHoles = true)
        {
            TerrainData copyTerrainData = new TerrainData();
            copyTerrainData.SetDetailResolution(terrainData.detailResolution, terrainData.detailResolutionPerPatch);
            copyTerrainData.heightmapResolution = terrainData.heightmapResolution;
            copyTerrainData.size = terrainData.size;
            copyTerrainData.SetHeights(0, 0, terrainData.GetHeights(0, 0, terrainData.heightmapResolution, terrainData.heightmapResolution));

            if(copyHoles)
            {
                int holRes = terrainData.holesResolution;
                copyTerrainData.SetHoles(0, 0, terrainData.GetHoles(0, 0, holRes, holRes));
            }

            copyTerrainData.terrainLayers = terrainData.terrainLayers;

            float[,,] alphaMaps = terrainData.GetAlphamaps(0, 0, terrainData.alphamapWidth, terrainData.alphamapHeight);
            copyTerrainData.SetAlphamaps(0, 0, alphaMaps);
            return copyTerrainData;
        }

        public static void TerrainColliderWithoutHoles(this Terrain terrain)
        {
            TerrainCollider collider = terrain.GetComponent<TerrainCollider>();
            if(collider != null && terrain.terrainData != null)
            {
                collider.terrainData = CopyTerrainData(terrain.terrainData, false);
                if(TerrainDataChanged != null)
                {
                    TerrainDataChanged(terrain);
                }
                if (TerrainModified != null)
                {
                    TerrainModified(terrain);
                }
            }
        }

        public static void CreateTerrain()
        {
            GameObject go = new GameObject("Terrain");
            Terrain terrain = go.AddComponent<Terrain>();
            TerrainCollider collider = go.AddComponent<TerrainCollider>();
            TerrainData terrainData = DefaultTerrainData();

            terrain.terrainData = terrainData;
            collider.terrainData = terrainData;

            go.transform.position = Vector3.Scale(-terrainData.size / 2, new Vector3(1, 0, 1));
        }

        public static TerrainData DefaultTerrainData(Vector3 size, int resoultion)
        {
            TerrainData terrainData = new TerrainData();
            terrainData.SetDetailResolution(1024, 32);
            terrainData.heightmapResolution = resoultion;
            terrainData.size = new Vector3(size.x, size.y, size.z);
            terrainData.SetHoles(0, 0, new bool[,] { { true } });

            ITerrainSettings terrainSettings = IOC.Resolve<ITerrainSettings>();
            Texture2D texture = terrainSettings != null ? terrainSettings.DefaultTexture : (Texture2D)Resources.Load("Textures/RTT_DefaultGrass");
            if(RenderPipelineInfo.Type == RPType.URP)
            {
                texture.mipMapBias = 0;
                texture.anisoLevel = 9;
                texture.filterMode = FilterMode.Trilinear;
            }
            terrainData.terrainLayers = new[]
            {
                new TerrainLayer() { diffuseTexture = texture }
            };

            if (terrainData.terrainLayers[0].diffuseTexture != null)
            {
                terrainData.terrainLayers[0].diffuseTexture.hideFlags = HideFlags.None;
            }

            float[,,] alphaMaps = terrainData.GetAlphamaps(0, 0, terrainData.alphamapWidth, terrainData.alphamapHeight);
            int amapY = alphaMaps.GetLength(0);
            int amapX = alphaMaps.GetLength(1);

            for (int y = 0; y < amapY; y++)
            {
                for (int x = 0; x < amapX; x++)
                {
                    alphaMaps[y, x, 0] = 1;
                }
            }

            terrainData.SetAlphamaps(0, 0, alphaMaps);
            return terrainData;
        }

        public static TerrainData DefaultTerrainData()
        {
            return DefaultTerrainData(new Vector3(200, 64, 200), 513);
        }
    }

    public class TerrainEditor : MonoBehaviour
    {
        public event TerrainEvent TerrainChanged;

        public enum EditorType
        {
            Empty = 0,
            Paint = 1,
            Area = 2,
            Grid = 3,
            Settings = 4,
        }

        public enum PaintTool
        {
            Raise_Or_Lower_Terrain = 0,
            Paint_Texture = 1,
            Stamp_Terrain = 2,
            Set_Height = 3,
            Smooth_Height = 4,
            Paint_Holes = 5,
            Paint_Details = 6,
            Paint_Trees = 7,
        }

        [SerializeField]
        private Toggle[] m_toggles = null;
        [SerializeField]
        private GameObject[] m_editors = null;
        [SerializeField]
        private EnumEditor m_paintToolSelector = null;
        [SerializeField]
        private GameObject[] m_paintTools = null;

        public TerrainProjectorBase Projector
        {
            get;
            private set;
        }

        private Terrain m_terrain;
        public Terrain Terrain
        {
            get { return m_terrain; }
            set
            {                
                if (m_terrain != value)
                {
                    m_terrain = value;
                    if (TerrainChanged != null)
                    {
                        TerrainChanged();
                    }

                    EditorType editorType = EditorType.Empty;
                    for (int i = 1; i < m_toggles.Length; ++i)
                    {
                        if (m_toggles[i] != null && m_toggles[i].isOn)
                        {
                            editorType = (EditorType)i;
                        }
                    }

                    UpdateProjectorState(editorType);
                }
            }
        }

        private PaintTool m_selectedPaintTool;
        public PaintTool SelectedPaintTool
        {
            get { return m_selectedPaintTool; }
            set
            {
                if(m_selectedPaintTool != value)
                {
                    m_selectedPaintTool = value;

                    UpdatePaintToolVisibility();

                    PlayerPrefs.SetInt("TerrainEditor.SelectedPaintTool", (int)m_selectedPaintTool);
                }
            }
        }

        public void UpdatePaintToolVisibility()
        {
            UpdateProjectorState(EditorType.Paint);

            for (int i = 0; i < m_paintTools.Length; ++i)
            {
                m_paintTools[i].SetActive(false);
            }

            m_paintTools[(int)SelectedPaintTool].SetActive(true);
        }

        private IRTE m_editor;
        private IWindowManager m_wm;
        private ILocalization m_localization;

        private void Awake()
        {
            m_localization = IOC.Resolve<ILocalization>();
            m_editor = IOC.Resolve<IRTE>();
            m_editor.Tools.ToolChanging += OnEditorToolChanging;
            m_wm = IOC.Resolve<IWindowManager>();
            if (m_wm != null)
            {
                m_wm.WindowCreated += OnWindowCreated;
                m_wm.AfterLayout += OnAfterLayout;
            }
            else
            {
                SubscribeSelectionChangingEvent(false);
                SubscribeSelectionChangingEvent(true);
            }

            Projector = IOC.Resolve<TerrainProjectorBase>();
            Projector.transform.SetParent(m_editor.Root, false);
            Projector.gameObject.SetActive(false);

            if(IOC.Resolve<ITerrainGridTool>() == null)
            {
                if(m_toggles[(int)EditorType.Grid])
                {
                    m_toggles[(int)EditorType.Grid].gameObject.SetActive(false);
                }
            }

            if (IOC.Resolve<ITerrainAreaTool>() == null)
            {
                if (m_toggles[(int)EditorType.Area])
                {
                    m_toggles[(int)EditorType.Area].gameObject.SetActive(false);
                }
            }

            for (int i = 0; i < m_toggles.Length; ++i)
            {
                Toggle toggle = m_toggles[i];
                if(toggle != null)
                {
                    EditorType editorType = ToEditorType(i);
                    UnityEventHelper.AddListener(toggle, tog => tog.onValueChanged, v => OnToggleValueChanged(editorType, v));
                }
            }

            for(int i = 0; i < m_editors.Length; ++i)
            {
                m_editors[i].SetActive(false);
            }

            EditorType toolType = (m_editor.Tools.Custom is EditorType) ? (EditorType)m_editor.Tools.Custom : EditorType.Empty;
            Toggle selectedToggle = m_toggles[(int)toolType];
            if(selectedToggle != null)
            {
                selectedToggle.isOn = true;
            }
            else
            {
                GameObject emptyEditor = m_editors[(int)EditorType.Empty];
                if (emptyEditor)
                {
                    emptyEditor.gameObject.SetActive(true);
                }
            }
            
            SubscribeSelectionChangingEvent(true);

            SelectedPaintTool = (PaintTool)PlayerPrefs.GetInt("TerrainEditor.SelectedPaintTool", (int)PaintTool.Raise_Or_Lower_Terrain);
            UpdatePaintToolVisibility();
            if (m_paintToolSelector != null)
            {
                m_paintToolSelector.Init(this, this, Strong.PropertyInfo((TerrainEditor x) => x.SelectedPaintTool), null, m_localization.GetString("ID_RTTerrain_TerrainEditor_Tool", "Tool:"), null, null, null, false);
            }
        }

        private void Start()
        {
           
        }

        private void OnDestroy()
        {
            if(m_wm != null)
            {
                m_wm.WindowCreated -= OnWindowCreated;
                m_wm.AfterLayout -= OnAfterLayout;
            }

            if (m_editor != null)
            {
                m_editor.Tools.ToolChanging -= OnEditorToolChanging;
            }

            if(Projector != null)
            {
                Destroy(Projector.gameObject);
            }

            for (int i = 0; i < m_toggles.Length; ++i)
            {
                Toggle toggle = m_toggles[i];
                UnityEventHelper.RemoveAllListeners(toggle, tog => tog.onValueChanged);
            }

            SubscribeSelectionChangingEvent(false);
        }

        private void OnToggleValueChanged(EditorType editorType,  bool value)
        {
            GameObject emptyEditor = m_editors[(int)EditorType.Empty];
            if (emptyEditor)
            {
                emptyEditor.gameObject.SetActive(!value);
            }

            GameObject editor = m_editors[(int)editorType];
            UpdateProjectorState(editorType);

            if (editor)
            {
                editor.SetActive(value);
                if (value)
                {
                    m_editor.Tools.Custom = editorType;
                }
            }

            StartCoroutine(CoUpdateSize());
        }

        private IEnumerator CoUpdateSize()
        {
            //Dirty workaround to fix incorrect TreeView layout (Brushes, Layers)
            yield return new WaitForEndOfFrame();
            RectTransform rt = GetComponent<RectTransform>();
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rt.rect.width + 0.01f);
            yield return new WaitForEndOfFrame();
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rt.rect.width - 0.01f);
        }

        private void UpdateProjectorState(EditorType editorType)
        {
            if (Projector != null)
            {
                if (Terrain == null || editorType != EditorType.Paint || !m_toggles[(int)editorType].isOn)
                {
                    Projector.gameObject.SetActive(false);
                }
                else
                {
                    Projector.gameObject.SetActive(true);
                }
            }
        }

        private static EditorType ToEditorType(int value)
        {
            if (!Enum.IsDefined(typeof(EditorType), value))
            {
                return EditorType.Empty;
            }
            return (EditorType)value;
        }

        private void SubscribeSelectionChangingEvent(bool subscribe)
        {
            if (m_editor != null)
            {
                foreach (RuntimeWindow window in m_editor.Windows)
                {
                    SubscribeSelectionChangingEvent(subscribe, window);
                }
            }
        }

        private void SubscribeSelectionChangingEvent(bool subscribe, RuntimeWindow window)
        {
            if (window != null && window.WindowType == RuntimeWindowType.Scene)
            {
                IRuntimeSelectionComponent selectionComponent = window.IOCContainer.Resolve<IRuntimeSelectionComponent>();

                if (selectionComponent != null)
                {
                    if (subscribe)
                    {
                        selectionComponent.SelectionChanging += OnSelectionChanging;
                    }
                    else
                    {
                        selectionComponent.SelectionChanging -= OnSelectionChanging;
                    }
                }
            }
        }

        private void OnEditorToolChanging(RuntimeTool toolType, object customTool)
        {
            if (!(customTool is EditorType))
            {
                foreach (Toggle toggle in m_toggles)
                {
                    if (toggle != null)
                    {
                        toggle.isOn = false;
                    }
                }
            }
            else
            {
                EditorType editorType = (EditorType)customTool;
                m_editor.Tools.IsBoxSelectionEnabled = editorType == EditorType.Grid;
            }
        }

        private void OnSelectionChanging(object sender, RuntimeSelectionChangingArgs e)
        {
            IRuntimeSelectionComponent selectionComponent = (IRuntimeSelectionComponent)sender;
            if (selectionComponent.Selection != m_editor.Selection)
            {
                return;
            }

            if (m_editor.Tools.Custom is EditorType)
            {
                EditorType editorType = (EditorType)m_editor.Tools.Custom;
                if(editorType != EditorType.Empty && editorType != EditorType.Grid)
                {
                    IRuntimeSelectionComponent component = (IRuntimeSelectionComponent)sender;
                    RaycastHit[] hits = Physics.RaycastAll(component.Window.Pointer);
                    
                    if(Terrain != null && hits.Any(hit => hit.collider.gameObject == Terrain.gameObject))
                    {
                        e.Cancel = true;
                    }
                }
            }
        }

        private void OnAfterLayout(IWindowManager wm)
        {
            SubscribeSelectionChangingEvent(false);
            SubscribeSelectionChangingEvent(true);
        }

        private void OnWindowCreated(Transform windowTransform)
        {
            RuntimeWindow window = windowTransform.GetComponent<RuntimeWindow>();
            if(window != null && window.WindowType == RuntimeWindowType.Scene)
            {
                SubscribeSelectionChangingEvent(false, window);
                SubscribeSelectionChangingEvent(true, window);
            }
        }

    }
}
