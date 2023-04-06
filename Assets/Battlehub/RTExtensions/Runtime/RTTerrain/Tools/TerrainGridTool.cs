using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Battlehub.RTHandles;
using Battlehub.RTCommon;
using System.Linq;
using System;

using UnityObject = UnityEngine.Object;
using Battlehub.RTEditor;
using System.Collections;

namespace Battlehub.RTTerrain
{
    public interface ITerrainGridTool
    {
        float ZSpacing
        {
            get;
            set;
        }

        float XSpacing
        {
            get;
            set;
        }

        bool EnableZTest
        {
            get;
            set;
        }

        bool IsActive
        {
            get;
            set;
        }

        bool SelectObjectsMode
        {
            get;
            set;
        }

        bool Refresh(Action redo = null, Action undo = null);
        void ResetPosition();
        void CutHoles();
        void ClearHoles();
    }

    public class TerrainGridTool : EditorExtension, ITerrainGridTool
    {
        private float m_zCount;
        private float m_xCount;
        private float[,] m_lerpGrid;
        private float[,] m_additiveHeights;
        private float[,] m_interpolatedHeights;

        private float[,] m_oldHeightmap;
        private TerrainToolState.Record m_oldState;

        private Dictionary<GameObject, int> m_handleToKey;
        private Dictionary<int, TerrainGridHandle> m_keyToHandle;
        private int[] m_selectedHandles;
        private TerrainToolState m_state;

        private Transform m_handlesRoot;
        [SerializeField]
        public TerrainGridHandle m_handlePrefab = null;
        [SerializeField]
        private PositionHandle m_positionHandlePrefab = null;

        private bool m_isDragging;
        private TerrainGridHandle m_pointerOverHandle;

        public enum Interpolation
        {
            Bilinear,
            Bicubic
        }

        private Interpolation m_prevInterpolation;
        private CachedBicubicInterpolator m_interpolator;

        private IRTE m_editor;
      
        private ITerrainCutoutMaskRenderer m_cutoutMaskRenderer;
        private ICustomSelectionComponent m_terrainHandlesSelection;
      
        [SerializeField]
        private bool m_enableZTest = true;

        public bool EnableZTest
        {
            get { return m_enableZTest; }
            set
            {
                m_enableZTest = value;
                if (Terrain == null || m_handleToKey == null)
                {
                    return;
                }

                foreach (GameObject go in m_handleToKey.Keys)
                {
                    TerrainGridHandle handle = go.GetComponent<TerrainGridHandle>();
                    handle.ZTest = value;
                }
            }
        }

        public float XSpacing
        {
            get;
            set;
        }

        public float ZSpacing
        {
            get;
            set;
        }

        private bool m_isActive;
        public bool IsActive
        {
            get { return m_isActive; }
            set
            {
                if(m_isActive != value)
                {
                    Deactivate();
                    m_isActive = value;
                    if(m_isActive)
                    {
                        Activate();
                    }
                }
            }
        }

        private Terrain m_terrain;
        private Terrain Terrain
        {
            get { return m_terrain; }
            set { m_terrain = value; }
        }

        private TerrainData TerrainData
        {
            get
            {
                if (Terrain == null)
                {
                    return null;
                }

                return Terrain.terrainData;
            }
        }

        private bool m_selectObjectsMode;
        public bool SelectObjectsMode
        {
            get { return m_selectObjectsMode; }
            set
            {
                if(m_selectObjectsMode != value)
                {
                    m_selectObjectsMode = value;
                    m_terrainHandlesSelection.Selection.activeGameObject = null;
                    m_handlesRoot.gameObject.SetActive(!m_selectObjectsMode);
                }
            }
        }

        protected override void OnEditorExist()
        {
            base.OnEditorExist();

            m_editor = IOC.Resolve<IRTE>();
            
            m_handlesRoot = new GameObject("Handles").transform;
            m_handlesRoot.SetParent(transform, false);

            m_terrainHandlesSelection = IOC.Resolve<ICustomSelectionComponent>();
         
            IOC.RegisterFallback<ITerrainGridTool>(this);

            OnEditorSelectionChanged(null);
            m_editor.Selection.SelectionChanged += OnEditorSelectionChanged;
        }

        protected override void OnEditorClosed()
        {
            base.OnEditorClosed();
            Cleanup();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Cleanup();
        }

        private void Cleanup()
        {
            if (m_editor != null)
            {
                m_editor.Selection.SelectionChanged -= OnEditorSelectionChanged;
            }

            if (m_terrainHandlesSelection != null)
            {
                m_terrainHandlesSelection.Enabled = false;
                m_terrainHandlesSelection.CreateCustomHandle -= OnCreateCustomHandle;
                m_terrainHandlesSelection.DestroyCustomHandle -= OnDestroyCustomHandle;
            }

            if (m_state != null)
            {
                if (m_state.CutoutTexture != null)
                {
                    Destroy(m_state.CutoutTexture);
                }
            }

            Deactivate();

            IOC.UnregisterFallback<ITerrainGridTool>(this);
        }

        private void OnEditorSelectionChanged(UnityObject[] unselectedObjects)
        {
            if(m_editor.Selection.activeGameObject != null)
            {
                Terrain = m_editor.Selection.activeGameObject.GetComponent<Terrain>();
            }
            else
            {
                Terrain = null;
            }
        }

        private void Activate()
        {
            m_cutoutMaskRenderer = IOC.Resolve<ITerrainCutoutMaskRenderer>();
            m_cutoutMaskRenderer.ObjectImageLayer = m_editor.CameraLayerSettings.ResourcePreviewLayer;

            m_terrainHandlesSelection.Selection.activeGameObject = null;
            m_terrainHandlesSelection.Selection.SelectionChanged += OnSelectionChanged;
            m_terrainHandlesSelection.CreateCustomHandle += OnCreateCustomHandle;
            m_terrainHandlesSelection.DestroyCustomHandle += OnDestroyCustomHandle;
            m_terrainHandlesSelection.Enabled = true;

            m_interpolator = new CachedBicubicInterpolator();

            TerrainData data = TerrainData;
            
            const float preferredSpace = 10;

            m_state = Terrain.GetComponent<TerrainToolState>();
            if (m_state == null)
            {
                m_additiveHeights = GetHeightmap();
                m_interpolatedHeights = new float[data.heightmapResolution, data.heightmapResolution];

                m_state = Terrain.gameObject.GetComponent<TerrainToolState>();
                if (m_state == null)
                {
                    m_state = Terrain.gameObject.AddComponent<TerrainToolState>();
                }

                m_state.ZSize = data.size.z;
                m_state.XSize = data.size.x;

                m_zCount = Mathf.RoundToInt(Mathf.Max(2, data.size.z / preferredSpace) + 1);
                m_xCount = Mathf.RoundToInt(Mathf.Max(2, data.size.x / preferredSpace) + 1);

                m_state.ZSpacing = m_state.ZSize / (m_zCount - 1);
                m_state.XSpacing = m_state.XSize / (m_xCount - 1);

                m_state.Grid = new float[Mathf.FloorToInt(m_zCount) * Mathf.FloorToInt(m_xCount)];
                m_state.HeightMap = new float[data.heightmapResolution * data.heightmapResolution];
                m_state.CutoutTexture = m_cutoutMaskRenderer.CreateMask(data, null);

                XSpacing = m_state.XSpacing;
                ZSpacing = m_state.ZSpacing;
            }
            else
            {
                InitAdditiveAndInterpolatedHeights();
            }

            CreateHandles();
            EnableZTest = EnableZTest;

            m_editor.Undo.BeforeUndo += OnBeforeUndoRedo;
            m_editor.Undo.BeforeRedo += OnBeforeUndoRedo;
            m_editor.Undo.UndoCompleted += OnUndoRedoCompleted;
            m_editor.Undo.RedoCompleted += OnUndoRedoCompleted;
        }



        private void Deactivate()
        {
            if(m_terrainHandlesSelection != null)
            {
                m_terrainHandlesSelection.Selection.activeGameObject = null;
                m_terrainHandlesSelection.Enabled = false;
                m_terrainHandlesSelection.Selection.SelectionChanged -= OnSelectionChanged;
                m_terrainHandlesSelection.CreateCustomHandle -= OnCreateCustomHandle;
                m_terrainHandlesSelection.DestroyCustomHandle -= OnDestroyCustomHandle;
            }

            DestroyHandles();

            if (m_editor != null && m_editor.Undo != null)
            {
                m_editor.Undo.BeforeUndo -= OnBeforeUndoRedo;
                m_editor.Undo.BeforeRedo -= OnBeforeUndoRedo;
                m_editor.Undo.UndoCompleted -= OnUndoRedoCompleted;
                m_editor.Undo.RedoCompleted -= OnUndoRedoCompleted;
            }

            if (m_coHandleExternalUndoRedo != null)
            {
                StopCoroutine(m_coHandleExternalUndoRedo);
                m_coHandleExternalUndoRedo = null;
            }
        }

        private void OnBeforeUndoRedo()
        {
            TerrainDataExt.TerrainDataChanged += OnTerrainChangesReverted;
            TerrainDataExt.HeightsChanged += OnTerrainChangesReverted;
            TerrainDataExt.HolesChanged += OnTerrainChangesReverted;
            TerrainDataExt.SizeChanged += OnTerrainChangesReverted;
        }

        private void OnUndoRedoCompleted()
        {
            TerrainDataExt.TerrainDataChanged -= OnTerrainChangesReverted;
            TerrainDataExt.HeightsChanged -= OnTerrainChangesReverted;
            TerrainDataExt.HolesChanged -= OnTerrainChangesReverted;
            TerrainDataExt.SizeChanged -= OnTerrainChangesReverted;
        }

        private bool m_isInternalUndoRedo;
        private void OnTerrainChangesReverted(Terrain data)
        {
            if(m_isInternalUndoRedo)
            {
                return;
            }

            if(m_coHandleExternalUndoRedo == null)
            {
                m_coHandleExternalUndoRedo = CoHandleExternalUndoRedo();
                StartCoroutine(m_coHandleExternalUndoRedo);
            }
        }


        private IEnumerator m_coHandleExternalUndoRedo;
        private IEnumerator CoHandleExternalUndoRedo()
        {
            yield return new WaitForEndOfFrame();

            HandleExternalUndoRedo();

            m_coHandleExternalUndoRedo = null;
        }

        private void HandleExternalUndoRedo()
        {
            m_terrainHandlesSelection.Selection.activeGameObject = null;

            Debug.Log("Handling external undo redo");

            XSpacing = m_state.XSpacing;
            ZSpacing = m_state.ZSpacing;
            m_xCount = m_state.XSize / XSpacing + 1;
            m_zCount = m_state.ZSize / ZSpacing + 1;

            InitAdditiveAndInterpolatedHeights();
            if (IsActive)
            {
                CreateHandles();
            }
        }

        public bool Refresh(Action redo = null, Action undo = null)
        {
            TerrainData data = TerrainData;
            if(m_state == null || m_state.HeightMap == null  || data == null)
            {
                return false;
            }

            if (m_state.HeightMap.Length == data.heightmapResolution * data.heightmapResolution &&
                m_state.Grid.Length == Mathf.FloorToInt(m_state.ZSize / ZSpacing + 1) * Mathf.FloorToInt(m_state.XSize / XSpacing + 1) &&
                m_state.ZSize == data.size.z && m_state.XSize == data.size.x)
            {
                return false;
            }

            m_terrainHandlesSelection.Selection.activeGameObject = null;

            TerrainToolState.Record oldState = m_state.Save();

            m_state.ZSize = data.size.z;
            m_state.XSize = data.size.x;

            m_xCount = m_state.XSize / XSpacing + 1;
            m_zCount = m_state.ZSize / ZSpacing + 1;

            m_state.Grid = new float[Mathf.FloorToInt(m_zCount) * Mathf.FloorToInt(m_xCount)];
            m_state.HeightMap = new float[data.heightmapResolution * data.heightmapResolution];
            m_state.CutoutTexture = m_cutoutMaskRenderer.CreateMask(data, null);

            m_state.ZSpacing = m_state.ZSize / (m_zCount - 1);
            m_state.XSpacing = m_state.XSize / (m_xCount - 1);

            InitAdditiveAndInterpolatedHeights();
            if(IsActive)
            {
                CreateHandles();
            }

            TerrainToolState.Record newState = m_state.Save();
            m_editor.Undo.CreateRecord(redoRecord =>
            {
                m_isInternalUndoRedo = true;

                if (redo != null)
                {
                    redo();
                }

                if (m_state != null)
                {
                    m_state.Load(newState);
                }

                m_terrainHandlesSelection.Selection.activeGameObject = null;

                XSpacing = m_state.XSpacing;
                ZSpacing = m_state.ZSpacing;
                m_xCount = m_state.XSize / XSpacing + 1;
                m_zCount = m_state.ZSize / ZSpacing + 1;

                InitAdditiveAndInterpolatedHeights();
                if (IsActive)
                {
                    CreateHandles();
                }

                m_isInternalUndoRedo = false;

                return true;
            },
            undoRecord =>
            {
                m_isInternalUndoRedo = true;

                if (undo != null)
                {
                    undo();
                }

                if (m_state != null)
                {
                    m_state.Load(oldState);
                }

                m_terrainHandlesSelection.Selection.activeGameObject = null;

                XSpacing = m_state.XSpacing;
                ZSpacing = m_state.ZSpacing;
                m_xCount = m_state.XSize / XSpacing + 1;
                m_zCount = m_state.ZSize / ZSpacing + 1;

                InitAdditiveAndInterpolatedHeights();
                if (IsActive)
                {
                    CreateHandles();
                }

                m_isInternalUndoRedo = false;

                return true;
            });

            return true;
        }


        public void ResetPosition()
        {
            if (m_selectedHandles != null)
            {
                foreach (int hid in m_selectedHandles)
                {
                    TerrainGridHandle handle;
                    if (!m_keyToHandle.TryGetValue(hid, out handle))
                    {
                        Debug.LogWarningFormat("Handle {0} was not found", hid);
                        continue;
                    }

                    Vector3 pos = handle.GetLocalPosition();
                    pos.y = 0;

                    TerrainData data = TerrainData;
                    Vector2Int hPos = new Vector2Int(
                        (int)(pos.x / data.heightmapScale.x),
                        (int)(pos.z / data.heightmapScale.z));

                    handle.SetTerrainHeight(data.GetHeight(hPos.x, hPos.y));
                    handle.SetLocalPostion(pos);
                    UpdateTerrain(hid, pos, GetInterpolatedHeights, SetTerrainHeights);
                }

                IWindowManager wm = IOC.Resolve<IWindowManager>();
                foreach (Transform windowTransform in wm.GetWindows(RuntimeWindowType.Scene.ToString()))
                {
                    RuntimeWindow window = windowTransform.GetComponent<RuntimeWindow>();
                    if(window == null)
                    {
                        continue;
                    }
                    IRuntimeSelectionComponent selectionComponent = window.IOCContainer.Resolve<IRuntimeSelectionComponent>();
                    if (selectionComponent != null && selectionComponent.CustomHandle != null)
                    {
                        selectionComponent.CustomHandle.Refresh();
                    }
                }
            }
        }

        public void ClearHoles()
        {
            CreateAndApplyCutoutTexture(new GameObject[0]);
        }

        public void CutHoles()
        {
            GameObject[] objects = m_terrainHandlesSelection.Selection.gameObjects;

            if(objects != null && objects.Length > 0)
            {
                float[,] heightMap = GetHeightmap();
                TerrainToolState.Record state = m_state.Save();

                CreateAndApplyCutoutTexture(objects);
                RecordState(heightMap, state);
            }
        }

        private GameObject[] CreateAndApplyCutoutTexture(GameObject[] objects)
        {
            if (objects != null)
            {
                objects = objects.Where(o => !m_handleToKey.ContainsKey(o) && !o.GetComponent<Terrain>()).ToArray();
            }

            if (m_state.CutoutTexture != null)
            {
                Destroy(m_state.CutoutTexture);
            }

            m_state.CutoutTexture = m_cutoutMaskRenderer.CreateMask(TerrainData, objects);

            float[,] hmap = m_interpolatedHeights;
            SetTerrainHeights(0, 0, hmap);
            return objects;
        }

        private void CreateHandles()
        {
            m_prevInterpolation = m_state.Interpolation;
            InitLerpGrid();
            DestroyHandles();
            m_handleToKey = new Dictionary<GameObject, int>(m_state.Grid.Length);
            m_keyToHandle = new Dictionary<int, TerrainGridHandle>(m_state.Grid.Length);
            m_handlePrefab.gameObject.SetActive(false);

            TerrainData data = TerrainData;
            int xCount = Mathf.FloorToInt(m_xCount);
            int zCount = Mathf.FloorToInt(m_zCount);
            for (int x = 0; x < xCount; ++x)
            {
                for (int z = 0; z < zCount; ++z)
                {
                    TerrainGridHandle handle;
                    LockAxes lockAxes;

                    handle = Instantiate(m_handlePrefab, m_handlesRoot);
                    lockAxes = handle.gameObject.AddComponent<LockAxes>();

                    lockAxes.PositionZ = false;
                    lockAxes.PositionX = true;
                    lockAxes.PositionZ = true;
                    lockAxes.ScaleX = lockAxes.ScaleY = lockAxes.ScaleZ = true;
                    lockAxes.RotationX = lockAxes.RotationY = lockAxes.RotationZ = lockAxes.RotationScreen = lockAxes.RotationFree = true;

                    handle.ZTest = EnableZTest;
                    handle.gameObject.hideFlags = HideFlags.HideInHierarchy;

                    float y = m_state.Grid[z * xCount + x] * data.heightmapScale.y;
                    Vector2Int hPos = new Vector2Int(
                        (int)(x * m_state.XSpacing / data.heightmapScale.x),
                        (int)(z * m_state.ZSpacing / data.heightmapScale.z));

                    handle.SetTerrainHeight(data.GetHeight(hPos.x, hPos.y) - y);
                    handle.SetLocalPostion(new Vector3(x * m_state.XSpacing, y, z * m_state.ZSpacing));
                    handle.name = "h " + x + "," + z;
                    handle.gameObject.SetActive(true);

                    int key = z * xCount + x;
                    m_handleToKey.Add(handle.gameObject, key);
                    m_keyToHandle.Add(key, handle);
                }
            }
        }

        private void UpdateHandlePositions()
        {
            if (m_keyToHandle == null)
            {
                return;
            }

            TerrainData data = TerrainData;
            int xCount = Mathf.FloorToInt(m_xCount);
            int zCount = Mathf.FloorToInt(m_zCount);
            for (int x = 0; x < xCount; ++x)
            {
                for (int z = 0; z < zCount; ++z)
                {
                    int hid = z * xCount + x;

                    TerrainGridHandle handle = m_keyToHandle[hid].GetComponent<TerrainGridHandle>();
                    float y = m_state.Grid[hid] * data.heightmapScale.y;
                    Vector2Int hPos = new Vector2Int(
                      (int)(x * m_state.XSpacing / data.heightmapScale.x),
                      (int)(z * m_state.ZSpacing / data.heightmapScale.z));

                    handle.SetTerrainHeight(data.GetHeight(hPos.x, hPos.y) - y);
                    handle.SetLocalPostion(new Vector3(x * m_state.XSpacing, y, z * m_state.ZSpacing));
                }
            }
        }

        private void DestroyHandles()
        {
            if (m_handleToKey != null)
            {
                foreach (KeyValuePair<GameObject, int> kvp in m_handleToKey)
                {
                    GameObject handle = kvp.Key;
                    Destroy(handle);
                }
                m_handleToKey = null;
                m_keyToHandle = null;
            }
            m_selectedHandles = null;
        }

        private void InitAdditiveAndInterpolatedHeights()
        {
            TerrainData data = TerrainData;

            m_additiveHeights = GetHeightmap();
            m_interpolatedHeights = new float[data.heightmapResolution, data.heightmapResolution];

            for (int i = 0; i < data.heightmapResolution; ++i)
            {
                for (int j = 0; j < data.heightmapResolution; ++j)
                {
                    m_interpolatedHeights[i, j] = m_state.HeightMap[i * data.heightmapResolution + j];
                    if (!IsCutout(data, j, i))
                    {
                        m_additiveHeights[i, j] -= m_interpolatedHeights[i, j];
                    }
                }
            }
        }

        private bool IsCutout(TerrainData data, int x, int y)
        {
            int width = data.heightmapResolution;
            int height = data.heightmapResolution;

            float u = (float)(x) / width;
            float v = (float)(y) / height;
            if (u >= 0 && u <= 1 && v >= 0 && v <= 1)
            {
                Color color = m_state.CutoutTexture.GetPixelBilinear(u, v);
                if (Mathf.Approximately(color.a, 1))
                {
                    return true;
                }
            }
            return false;
        }

        private void SetTerrainHeights(int x, int y, float[,] heights)
        {
            TerrainData data = Terrain.terrainData;

            int h = heights.GetLength(0);
            int w = heights.GetLength(1);

            float[,] currentHeights = data.GetHeights(x, y, w, h);
            for (int i = 0; i < h; i++)
            {
                for (int j = 0; j < w; j++)
                {
                    if (IsCutout(data, j, i))
                    {
                        currentHeights[i, j] = m_additiveHeights[y + i, x + j];
                    }
                    else
                    {
                        currentHeights[i, j] = heights[i, j] + m_additiveHeights[y + i, x + j];
                    }
                }
            }

            Terrain.SetHeights(x, y, currentHeights);
        }

        private float[,] GetInterpolatedHeights(int x, int y, int w, int h)
        {
            float[,] result = new float[h, w];
            for (int i = 0; i < h; i++)
            {
                for (int j = 0; j < w; j++)
                {
                    result[i, j] = m_interpolatedHeights[y + i, x + j];
                }
            }
            return result;
        }


        private float[,] GetHeightmap()
        {
            int w = Terrain.terrainData.heightmapResolution;
            int h = Terrain.terrainData.heightmapResolution;
            return Terrain.terrainData.GetHeights(0, 0, w, h);
        }

        private void OnSelectionChanged(UnityObject[] unselectedObjects)
        {
            if (unselectedObjects != null)
            {
                foreach (UnityObject obj in unselectedObjects)
                {
                    GameObject go = obj as GameObject;
                    if (go != null)
                    {
                        TerrainGridHandle handle = go.GetComponent<TerrainGridHandle>();
                        if (handle != null)
                        {
                            handle.IsSelected = false;
                        }
                    }
                }
            }

            if (m_terrainHandlesSelection.Selection.gameObjects == null || m_terrainHandlesSelection.Selection.gameObjects.Length == 0)
            {
                m_selectedHandles = null;
            }
            else
            {
                IEnumerable<GameObject> selectedHandles = m_terrainHandlesSelection.Selection.gameObjects.Where(go => go != null && m_handleToKey.ContainsKey(go));
                foreach (GameObject go in selectedHandles)
                {
                    TerrainGridHandle handle = go.GetComponent<TerrainGridHandle>();
                    handle.IsSelected = true;
                }

                m_selectedHandles = selectedHandles.Select(go => m_handleToKey[go]).ToArray();
                if (m_selectedHandles.Length == 0)
                {
                    m_selectedHandles = null;
                }
            }
        }

        private void OnCreateCustomHandle(IRuntimeSelectionComponent selectionComponent)
        {
            if (selectionComponent != null && selectionComponent.CustomHandle == null)
            {
                m_positionHandlePrefab.gameObject.SetActive(false);

                selectionComponent.CustomHandle = Instantiate(m_positionHandlePrefab, transform);
                selectionComponent.Filtering += OnSelectionFiltering;
                selectionComponent.SelectionChanging += OnSelectionChanging;
                selectionComponent.CustomHandle.BeforeDrag.AddListener(OnBeforeDrag);
                selectionComponent.CustomHandle.Drop.AddListener(OnDrop);
            }
        }

        private void OnDestroyCustomHandle(IRuntimeSelectionComponent selectionComponent)
        {
            if (selectionComponent != null && selectionComponent.CustomHandle != null)
            {
                selectionComponent.Filtering -= OnSelectionFiltering;
                selectionComponent.SelectionChanging -= OnSelectionChanging;
                selectionComponent.CustomHandle.BeforeDrag.RemoveListener(OnBeforeDrag);
                selectionComponent.CustomHandle.Drop.RemoveListener(OnDrop);
                selectionComponent.Selection = null;
                Destroy(selectionComponent.CustomHandle.gameObject);
                selectionComponent.CustomHandle = null;
            }
        }   

        private void OnSelectionFiltering(object sender, RuntimeSelectionFilteringArgs e)
        {
            if(EnableZTest)
            {
                return;
            }

            if(SelectObjectsMode)
            {
                return;
            }

            IList<RaycastHit> hits = e.Hits;
            for(int i = hits.Count - 1; i >= 0; i--)
            {
                RaycastHit hit = hits[i];
                if(!m_handleToKey.ContainsKey(hit.collider.gameObject))
                {
                    hits.RemoveAt(i);
                }
            }
        }

        private void OnSelectionChanging(object sender, RuntimeSelectionChangingArgs e)
        {
            if (SelectObjectsMode)
            {
                return;
            }

            IList<UnityObject> selected = e.Selected;
            for (int i = selected.Count - 1; i >= 0; i--)
            {
                if(!m_handleToKey.ContainsKey(selected[i] as GameObject))
                {
                    selected.RemoveAt(i);
                }
            }
        }

        private void OnBeforeDrag(BaseHandle handle)
        {
            m_isDragging = m_selectedHandles != null;
            if (m_isDragging)
            {
                handle.EnableUndo = false;

                m_oldHeightmap = GetHeightmap();
                m_oldState = m_state.Save();
            }
        }

        private void OnDrop(BaseHandle handle)
        {
            if (m_isDragging)
            {
                m_isDragging = false;
                if (m_selectedHandles != null)
                {
                    int[] selectedHandles = m_selectedHandles.ToArray();

                    for (int i = 0; i < selectedHandles.Length; ++i)
                    {
                        TerrainGridHandle selectedHandle = m_keyToHandle[selectedHandles[i]];
                        UpdateTerrain(selectedHandles[i], selectedHandle.GetLocalPosition(), GetInterpolatedHeights, SetTerrainHeights);
                    }

                    RecordState(m_oldHeightmap, m_oldState);
                }
                handle.EnableUndo = true;
            }
        }

        private void RecordState(float[,] oldHeightmap, TerrainToolState.Record oldState)
        {
            Terrain terrain = Terrain;
            terrain.TerrainColliderWithoutHoles();
            float[,] newHeightmap = GetHeightmap();
            TerrainToolState.Record newState = m_state.Save();

            m_oldHeightmap = null;
            m_oldState = null;

            m_editor.Undo.CreateRecord(redoRecord =>
            {
                m_isInternalUndoRedo = true;

                if (terrain.terrainData != null)
                {
                    terrain.SetHeights(0, 0, newHeightmap);
                    terrain.TerrainColliderWithoutHoles();
                }

                if (m_state != null)
                {
                    m_state.Load(newState);
                }

                UpdateHandlePositions();
                InitAdditiveAndInterpolatedHeights();

                m_isInternalUndoRedo = false;

                return true;
            },
            undoRecord =>
            {
                m_isInternalUndoRedo = true;

                if (terrain.terrainData != null)
                {
                    terrain.SetHeights(0, 0, oldHeightmap);
                    terrain.TerrainColliderWithoutHoles();
                }

                if (m_state != null)
                {
                    m_state.Load(oldState);
                }

                UpdateHandlePositions();
                InitAdditiveAndInterpolatedHeights();

                m_isInternalUndoRedo = false;

                return true;
            });
        }

        private void LateUpdate()
        {
            if(!IsActive)
            {
                return;
            }

            Transform terrainTransform = Terrain.transform;
            if (terrainTransform.position != gameObject.transform.position ||
               terrainTransform.rotation != gameObject.transform.rotation ||
               terrainTransform.localScale != gameObject.transform.localScale)
            {
                gameObject.transform.position = terrainTransform.position;
                gameObject.transform.rotation = terrainTransform.rotation;
                gameObject.transform.localScale = terrainTransform.localScale;
            }

            if (m_editor.ActiveWindow != null)
            {
                RuntimeWindow window = m_editor.ActiveWindow;
                if (window.WindowType == RuntimeWindowType.Scene)
                {
                    RaycastHit hit;
                    if (Physics.Raycast(window.Pointer, out hit))
                    {
                        TryHitTerrainHandle(hit);
                    }
                }
            }

            if (m_state.Interpolation != m_prevInterpolation)
            {
                m_prevInterpolation = m_state.Interpolation;
                InitLerpGrid();
            }

            if (m_isDragging)
            {
                if (m_selectedHandles != null && m_selectedHandles.Length == 1)
                {
                    for (int i = 0; i < m_selectedHandles.Length; ++i)
                    {
                        int hid = m_selectedHandles[i];
                        TerrainGridHandle handle = m_keyToHandle[hid];
                        UpdateTerrain(hid, handle.GetLocalPosition(), GetInterpolatedHeights, SetTerrainHeights);
                    }
                }
            }
        }

        private void TryHitTerrainHandle(RaycastHit hit)
        {
            TerrainGridHandle handle = hit.collider.GetComponent<TerrainGridHandle>();
            if (m_pointerOverHandle != handle)
            {
                if (m_pointerOverHandle != null)
                {
                    m_pointerOverHandle.IsPointerOver = false;
                }

                m_pointerOverHandle = handle;

                if (m_pointerOverHandle != null)
                {
                    m_pointerOverHandle.IsPointerOver = true;
                }
            }
        }

        private void UpdateTerrain(int hid, Vector3 position, Func<int, int, int, int, float[,]> getValues, Action<int, int, float[,]> setValues)
        {
            switch (m_state.Interpolation)
            {
                case Interpolation.Bilinear: UpdateTerrainBilinear(hid, position, getValues, setValues); break;
                case Interpolation.Bicubic: UpdateTerrainBicubic(hid, position, getValues, setValues); break;
            }
        }

        private void UpdateTerrainBilinear(int hid, Vector3 position, Func<int, int, int, int, float[,]> getValues, Action<int, int, float[,]> setValues)
        {
            TerrainData data = TerrainData;
            float[] grid = m_state.Grid;
            grid[hid] = position.y / data.heightmapScale.y;

            int zCount = Mathf.FloorToInt(m_zCount);
            
            m_lerpGrid[0, 0] = grid[hid - zCount - 1];
            m_lerpGrid[0, 1] = grid[hid - zCount];
            m_lerpGrid[0, 2] = grid[hid - zCount + 1];
            m_lerpGrid[1, 0] = grid[hid - 1];
            m_lerpGrid[1, 1] = grid[hid];
            m_lerpGrid[1, 2] = grid[hid + 1];
            m_lerpGrid[2, 0] = grid[hid + zCount - 1];
            m_lerpGrid[2, 1] = grid[hid + zCount];
            m_lerpGrid[2, 2] = grid[hid + zCount + 1];

            Vector2Int blockSize = new Vector2Int(
                (int)(m_state.XSpacing / data.heightmapScale.x),
                (int)(m_state.ZSpacing / data.heightmapScale.z));

            Vector2Int hPos = new Vector2Int(
                (int)(position.x / data.heightmapScale.x),
                (int)(position.z / data.heightmapScale.z));

            hPos -= blockSize;

            float[,] heightsvalues = getValues(
                hPos.x, hPos.y,
                blockSize.x * 2 + 1, blockSize.y * 2 + 1);

            for (int gy = 0; gy < 2; gy++)
            {
                int baseY = gy * blockSize.y;

                for (int gx = 0; gx < 2; gx++)
                {
                    int baseX = gx * blockSize.x;

                    for (int y = 0; y < blockSize.y; y++)
                    {
                        float ty = (float)y / blockSize.y;
                        for (int x = 0; x < blockSize.x; x++)
                        {
                            float tx = (float)x / blockSize.x;
                            heightsvalues[baseY + y, baseX + x] =
                                Mathf.Lerp(
                                    Mathf.Lerp(m_lerpGrid[gy, gx], m_lerpGrid[gy, gx + 1], tx),
                                    Mathf.Lerp(m_lerpGrid[gy + 1, gx], m_lerpGrid[gy + 1, gx + 1], tx),
                                    ty);
                        }
                    }
                }
            }

            setValues(hPos.x, hPos.y, heightsvalues);
        }


        private void UpdateTerrainBicubic(int hid, Vector3 position, Func<int, int, int, int, float[,]> getValues, Action<int, int, float[,]> setValues)
        {
            var data = TerrainData;
            if (hid >= 0)
            {
                m_state.Grid[hid] = position.y / data.heightmapScale.y;
            }
            else
            {
                Debug.LogError("Handle is not found!");
            }

            int zCount = Mathf.FloorToInt(m_zCount);
            int xCount = Mathf.FloorToInt(m_xCount);

            int2 iidx = new int2(hid % xCount, hid / xCount);

            for (int y = 0; y < 7; y++)
            {
                int _y = math.clamp(iidx.y - 3 + y, 0, zCount - 1);

                for (int x = 0; x < 7; x++)
                {
                    //int _x = math.clamp(iidx.x - 3 + x, 0, zCount - 1);
                    int _x = math.clamp(iidx.x - 3 + x, 0, xCount - 1);
                    m_lerpGrid[y, x] = m_state.Grid[xCount * _y + _x];
                }
            }

            float2 heightmapScale = ((float3)data.heightmapScale).xz;
            float2 pos = ((float3)position).xz;
            int2 block_size = (int2)(new float2(m_state.XSpacing, m_state.ZSpacing) / heightmapScale);

            int2 hPos = (int2)(pos / heightmapScale);
            hPos -= block_size * 2;

            int2 max_block = new int2(block_size.x * 4, block_size.y * 4);
            int res = data.heightmapResolution;
            RectInt r = new RectInt(hPos.x, hPos.y, max_block.x, max_block.y);
            r.xMin = math.clamp(r.xMin, 0, res);
            r.xMax = math.clamp(r.xMax, 0, res);
            r.yMin = math.clamp(r.yMin, 0, res);
            r.yMax = math.clamp(r.yMax, 0, res);

            float[,] hmap = getValues(r.x, r.y, r.width, r.height);

            for (int gy = 0; gy < 4; gy++)
            {
                int base_y = gy * block_size.y;

                for (int gx = 0; gx < 4; gx++)
                {
                    int base_x = gx * block_size.x;

                    m_interpolator.UpdateCoefficients(new float4x4(
                        m_lerpGrid[gy, gx], m_lerpGrid[gy, gx + 1], m_lerpGrid[gy, gx + 2], m_lerpGrid[gy, gx + 3],
                        m_lerpGrid[gy + 1, gx], m_lerpGrid[gy + 1, gx + 1], m_lerpGrid[gy + 1, gx + 2], m_lerpGrid[gy + 1, gx + 3],
                        m_lerpGrid[gy + 2, gx], m_lerpGrid[gy + 2, gx + 1], m_lerpGrid[gy + 2, gx + 2], m_lerpGrid[gy + 2, gx + 3],
                        m_lerpGrid[gy + 3, gx], m_lerpGrid[gy + 3, gx + 1], m_lerpGrid[gy + 3, gx + 2], m_lerpGrid[gy + 3, gx + 3]
                    ));

                    for (int y = 0; y < block_size.y; y++)
                    {
                        int _y = hPos.y + base_y + y;
                        if (_y >= r.yMin && _y < r.yMax)
                        {
                            float ty = (float)y / block_size.y;

                            for (int x = 0; x < block_size.x; x++)
                            {
                                int _x = hPos.x + base_x + x;
                                if (_x >= r.xMin && _x < r.xMax)
                                {
                                    float tx = (float)x / block_size.x;
                                    float height = m_interpolator.GetValue(tx, ty);
                                    float u = (float)(r.x + (_x - r.xMin)) / data.heightmapResolution;
                                    float v = (float)(r.y + (_y - r.yMin)) / data.heightmapResolution;
                                    if (u >= 0 && u <= 1 && v >= 0 && v <= 1)
                                    {
                                        Color color = m_state.CutoutTexture.GetPixelBilinear(u, v);
                                        if (Mathf.Approximately(color.a, 1))
                                        {
                                            hmap[_y - r.yMin, _x - r.xMin] = 0;
                                        }
                                        else
                                        {
                                            hmap[_y - r.yMin, _x - r.xMin] = height;
                                        }
                                    }
                                    else
                                    {
                                        hmap[_y - r.yMin, _x - r.xMin] = height;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            setValues(r.x, r.y, hmap);

            float[,] terrainHeightMap = data.GetHeights(r.x, r.y, r.width, r.height);
            for(int y = 0; y < r.height; ++y)
            {
                for(int x = 0; x < r.width; ++x)
                {
                    float height = terrainHeightMap[y, x] - m_additiveHeights[r.y + y, r.x + x];
                    m_state.HeightMap[(r.y + y) * data.heightmapResolution + r.x + x] = height;
                    m_interpolatedHeights[r.y + y, r.x + x] = height;
                }
            }
        }

        private void InitLerpGrid()
        {
            if (m_state.Interpolation == Interpolation.Bilinear)
            {
                m_lerpGrid = new float[3, 3];
            }
            else
            {
                m_lerpGrid = new float[7, 7];
            }
        }
    }
}
