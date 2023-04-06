using Battlehub.ProBuilderIntegration;
using Battlehub.RTCommon;
using Battlehub.RTHandles;
using Battlehub.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Battlehub.RTBuilder
{
    public class ManualUVSceneComponent : RTEComponent
    {
        [SerializeField]
        private Color m_selectionColor = Color.yellow;

        private readonly List<ManualUVRenderer> m_uvRenderers = new List<ManualUVRenderer>();
        private GameObject m_uvRenderersRoot;
        private IRTECamera m_graphicsCamera;

        private Plane m_plane;

        private readonly Dictionary<PBMesh, ManualUVRenderer> m_meshToRenderer = new Dictionary<PBMesh, ManualUVRenderer>();
        private IRuntimeSelectionComponent m_selectionComponent;

        private IProBuilderTool m_tool;
        private IManualUVEditor m_uvEditor;

        protected override void Awake()
        {
            base.Awake();
        
            m_tool = IOC.Resolve<IProBuilderTool>();
            m_uvEditor = IOC.Resolve<IManualUVEditor>();

            m_uvRenderersRoot = new GameObject("UVRenderers");
            m_uvRenderersRoot.transform.SetParent(transform, false);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        
            for (int i = 0; i < m_uvRenderers.Count; ++i)
            {
                Destroy(m_uvRenderers[i]);
            }
            m_uvRenderers.Clear();

            if (m_selectionComponent != null)
            {
                if(m_selectionComponent.BoxSelection != null)
                {
                    m_selectionComponent.BoxSelection.Selection -= OnBoxSelection;
                }
                
                if(m_selectionComponent.PositionHandle != null)
                {
                    m_selectionComponent.PositionHandle.BeforeDrag.RemoveListener(OnBeginMove);
                    m_selectionComponent.PositionHandle.Drag.RemoveListener(OnMove);
                    m_selectionComponent.PositionHandle.Drop.RemoveListener(OnEndMove);
                }

                if(m_selectionComponent.RotationHandle != null)
                {
                    m_selectionComponent.RotationHandle.BeforeDrag.RemoveListener(OnBeginRotate);
                    m_selectionComponent.RotationHandle.Drag.RemoveListener(OnRotate);
                    m_selectionComponent.RotationHandle.Drop.RemoveListener(OnEndRotate);
                }

                if(m_selectionComponent.ScaleHandle != null)
                {
                    m_selectionComponent.ScaleHandle.BeforeDrag.RemoveListener(OnBeginScale);
                    m_selectionComponent.ScaleHandle.Drag.RemoveListener(OnScale);
                    m_selectionComponent.ScaleHandle.Drop.RemoveListener(OnEndScale);
                }
            }
        }

        private void OnEnable()
        {
            m_tool.SelectionChanging += OnSelectionChanging;

            m_tool.ModeChanged += OnModeChanged;

            m_uvEditor.SelectionChanged += OnUVSelectionChanged;
            m_uvEditor.MeshSelected += OnMeshSelected;
            m_uvEditor.UVChanged += OnUVChanged;
            m_uvEditor.RefreshPivotPoint();

            if (m_graphicsCamera != null)
            {
                CreateUVRenderers();
                RefreshUVRenderers();
                m_graphicsCamera.RenderersCache.Refresh();
            }
        }

        private void OnDisable()
        {
            if (m_tool != null)
            {
                m_tool.SelectionChanging -= OnSelectionChanging;
                m_tool.ModeChanged -= OnModeChanged;
            }

            if(m_uvEditor != null)
            {
                m_uvEditor.SelectionChanged -= OnUVSelectionChanged;
                m_uvEditor.MeshSelected -= OnMeshSelected;
                m_uvEditor.UVChanged -= OnUVChanged;
            }
        }

        protected override void Start()
        {
            base.Start();
        
            m_plane = new Plane(Vector3.forward, 0);

            RuntimeCameraWindow cameraWindow = (RuntimeCameraWindow)Window;

            m_graphicsCamera = cameraWindow.GetGraphicsCamera();
            CreateUVRenderers();
            RefreshUVRenderers();
            m_graphicsCamera.RenderersCache.Refresh();
            m_uvEditor.RefreshPivotPoint();

            m_selectionComponent = Window.IOCContainer.Resolve<IRuntimeSelectionComponent>();
            m_selectionComponent.BoxSelection.Selection += OnBoxSelection;


            m_selectionComponent.PositionHandle.EnableUndo = false;
            m_selectionComponent.PositionHandle.BeforeDrag.AddListener(OnBeginMove);
            m_selectionComponent.PositionHandle.Drag.AddListener(OnMove);
            m_selectionComponent.PositionHandle.Drop.AddListener(OnEndMove);

            m_selectionComponent.RotationHandle.EnableUndo = false;
            m_selectionComponent.RotationHandle.BeforeDrag.AddListener(OnBeginRotate);
            m_selectionComponent.RotationHandle.Drag.AddListener(OnRotate);
            m_selectionComponent.RotationHandle.Drop.AddListener(OnEndRotate);

            m_selectionComponent.ScaleHandle.EnableUndo = false;
            m_selectionComponent.ScaleHandle.BeforeDrag.AddListener(OnBeginScale);
            m_selectionComponent.ScaleHandle.Drag.AddListener(OnScale);
            m_selectionComponent.ScaleHandle.Drop.AddListener(OnEndScale);
   
        }

        private void OnBeginMove(BaseHandle handle)
        {
            m_uvEditor.BeginMove();
        }

        private void OnMove(BaseHandle handle)
        {
            m_uvEditor.Move();
        }

        private void OnEndMove(BaseHandle handle)
        {
            m_uvEditor.EndMove();
        }

        private void OnBeginRotate(BaseHandle handle)
        {
            m_uvEditor.BeginRotate();
        }

        private void OnRotate(BaseHandle handle)
        {
            m_uvEditor.Rotate();
        }

        private void OnEndRotate(BaseHandle handle)
        {
            m_uvEditor.EndRotate();
        }

        private void OnBeginScale(BaseHandle handle)
        {
            m_uvEditor.BeginScale();
        }

        private void OnScale(BaseHandle handle)
        {
            m_uvEditor.Scale();
          
        }

        private void OnEndScale(BaseHandle handle)
        {
            m_uvEditor.EndScale();
        }

        private bool SelectAction()
        {
            return Editor.Input.GetPointerDown(0);
        }

        private bool SelectMultipleAction()
        {
            return Editor.Input.GetKey(KeyCode.LeftShift);
        }

        private void Update()
        {
            if(!IsWindowActive)
            {
                return;
            }

            RuntimeTools tools = Editor.Tools;
            if (tools.ActiveTool != null && tools.ActiveTool != m_selectionComponent.BoxSelection)
            {
                return;
            }

            if (tools.IsViewing)
            {
                return;
            }

            if (!m_selectionComponent.Selection.Enabled)
            {
                return;
            }

            Vector3 v1 = Window.Camera.ScreenToWorldPoint(Window.Pointer.ScreenPoint);
            Vector3 v2 = Window.Camera.ScreenToWorldPoint(Window.Pointer.ScreenPoint + Vector2.up * 20);
            float uvMargin = (v1 - v2).sqrMagnitude / (ManualUVRenderer.Scale * ManualUVRenderer.Scale);

            if(SelectAction())
            {
                Vector2 hitPoint;
                if(Raycast(out hitPoint))
                {
                    switch (m_tool.Mode)
                    {
                        case ProBuilderToolMode.Vertex:
                            m_uvEditor.SelectVertices(uv =>
                            {
                                float sqDistance = (uv - hitPoint).sqrMagnitude;
                                if (sqDistance > uvMargin)
                                {
                                    return float.PositiveInfinity;
                                }
                                return sqDistance;
                            }, true, !SelectMultipleAction());
                            break;
                        case ProBuilderToolMode.Face:
                            m_uvEditor.SelectFaces(uv =>
                            {
                                float sqDistance = (uv - hitPoint).sqrMagnitude;
                                if (sqDistance > uvMargin)
                                {
                                    return float.PositiveInfinity;
                                }
                                return sqDistance;
                            }, true, !SelectMultipleAction());
                            break;
                        default:
                            m_uvEditor.SelectEdges((uv0, uv1) =>
                            {
                                float sqDistance = PBMath.SqDistanceTo(uv0, uv1, hitPoint);
                                if (sqDistance > uvMargin)
                                {
                                    return float.PositiveInfinity;
                                }
                                return sqDistance;
                            }, true, !SelectMultipleAction());
                            break;
                    }  
                }

                m_uvEditor.RefreshPivotPoint();
            }
        }

        private void OnBoxSelection(object sender, BoxSelectionArgs e)
        {
            Bounds selectionBounds = m_selectionComponent.BoxSelection.SelectionBounds;
            Vector2 min = ManualUVRenderer.WorldToUV(Window.Camera.ScreenToWorldPoint(selectionBounds.min));
            Vector2 max = ManualUVRenderer.WorldToUV(Window.Camera.ScreenToWorldPoint(selectionBounds.max));

            switch (m_tool.Mode)
            {
                case ProBuilderToolMode.Vertex:
                    m_uvEditor.SelectVertices(uv => (min.x <= uv.x && min.y <= uv.y && uv.x <= max.x && uv.y <= max.y) ? 0 : 1, false, !SelectMultipleAction());
                    break;
                case ProBuilderToolMode.Face:
                    m_uvEditor.SelectFaces(uv => (min.x <= uv.x && min.y <= uv.y && uv.x <= max.x && uv.y <= max.y) ? 0 : 1, false, !SelectMultipleAction());
                    break;
                default:
                    m_uvEditor.SelectEdges((uv0, uv1) => PBMath.Intersects(min, max, uv0, uv1) ? 0 : 1, false, !SelectMultipleAction());
                    break;
            }

            m_uvEditor.RefreshPivotPoint();
        }

        private bool Raycast(out Vector2 uv)
        {
            float dinstance;
            Ray ray = Window.Pointer;
            if(!m_plane.Raycast(ray, out dinstance))
            {
                uv = Vector2.zero;
                return false;
            }

            uv = ManualUVRenderer.WorldToUV(ray.GetPoint(dinstance));
            return true;
        }

        private void OnSelectionChanging()
        {
            if (m_graphicsCamera != null)
            {
                CreateUVRenderers();
                m_graphicsCamera.RenderersCache.Refresh();
            }
            else
            {
                CreateUVRenderers();
            }
        }

        private void CreateUVRenderers()
        {
            HashSet<PBMesh> selectedMeshes = new HashSet<PBMesh>();
            IMeshEditor meshEditor = m_tool.GetEditor();
            if (meshEditor != null)
            {
                MeshSelection selection = meshEditor.GetSelection();
                if (selection != null)
                {
                    selectedMeshes = new HashSet<PBMesh>(selection.GetSelectedMeshes());
                }
            }

            int delta = selectedMeshes.Count() - m_uvRenderers.Count;
            if (delta > 0)
            {
                m_uvRenderersRoot.SetActive(false);
                for (int i = 0; i < delta; ++i)
                {
                    ManualUVRenderer renderer = m_uvRenderersRoot.AddComponent<ManualUVRenderer>();
                    renderer.Window = Window;
                    m_uvRenderers.Add(renderer);
                }
                m_uvRenderersRoot.SetActive(true);
            }
            else
            {
                delta = -delta;
                for (int i = 0; i < delta; ++i)
                {
                    Destroy(m_uvRenderers[m_uvRenderers.Count - 1]);
                    m_uvRenderers.RemoveAt(m_uvRenderers.Count - 1);
                }
            }

            foreach(PBMesh mesh in m_meshToRenderer.Keys.ToArray())
            {
                if(!selectedMeshes.Contains(mesh))
                {
                    m_meshToRenderer.Remove(mesh);
                }
            }
          
            int index = 0;
            foreach (PBMesh mesh in selectedMeshes)
            {
                ManualUVRenderer uvRenderer = m_uvRenderers[index];
                switch (m_tool.Mode)
                {
                    case ProBuilderToolMode.Vertex:
                        uvRenderer.Mode = ManualUVRenderer.RenderMode.Vertex;
                        break;
                    case ProBuilderToolMode.Edge:
                        uvRenderer.Mode = ManualUVRenderer.RenderMode.Edge;
                        break;
                    case ProBuilderToolMode.Face:
                        uvRenderer.Mode = ManualUVRenderer.RenderMode.Face;
                        break;
                }

                index++;

                m_meshToRenderer[mesh] = uvRenderer;
            }
        }


        private void OnModeChanged(ProBuilderToolMode oldMode)
        {
            switch (m_tool.Mode)
            {
                case ProBuilderToolMode.Vertex:
                    foreach (ManualUVSelection selection in m_uvEditor.Selection)
                    {
                        ManualUVRenderer uvRenderer = m_meshToRenderer[selection.Mesh];

                        uvRenderer.Mode = ManualUVRenderer.RenderMode.Vertex;
                        uvRenderer.Refresh(false, false);

                        OnUVSelectionChanged(selection);
                    }
                    break;
                case ProBuilderToolMode.Face:
                    foreach (ManualUVSelection selection in m_uvEditor.Selection)
                    {
                        ManualUVRenderer uvRenderer = m_meshToRenderer[selection.Mesh];

                        uvRenderer.Mode = ManualUVRenderer.RenderMode.Face;
                        uvRenderer.Refresh(false, false);

                        OnUVSelectionChanged(selection);
                    }
                    break;
                default:
                    foreach (ManualUVSelection selection in m_uvEditor.Selection)
                    {
                        ManualUVRenderer uvRenderer = m_meshToRenderer[selection.Mesh];

                        uvRenderer.Mode = ManualUVRenderer.RenderMode.Edge;
                        uvRenderer.Refresh(false, false);

                        OnUVSelectionChanged(selection);
                    }
                    break;
            }

            m_uvEditor.RefreshPivotPoint();
        }

        private void OnMeshSelected(ManualUVSelection uvSelection)
        {
            MeshSelectionToRenderer(uvSelection);
        }

        private void OnUVSelectionChanged(ManualUVSelection selection)
        {
            UVSelectionToRenderer(selection);
        }

        private void RefreshUVRenderers()
        {
            if (m_uvEditor.Selection == null)
            {
                return;
            }

            foreach (ManualUVSelection selection in m_uvEditor.Selection)
            {
                MeshSelectionToRenderer(selection);
                UVSelectionToRenderer(selection);
            }
        }

        private void MeshSelectionToRenderer(ManualUVSelection uvSelection)
        {
            PBMesh mesh = uvSelection.Mesh;
            PBEdge[] meshEdges = mesh.Edges;
            Dictionary<int, List<int>> faceToEdge = new Dictionary<int, List<int>>();
            for (int i = 0; i < meshEdges.Length; ++i)
            {
                PBEdge edge = meshEdges[i];
                List<int> edgeIndices;
                if (!faceToEdge.TryGetValue(edge.FaceIndex, out edgeIndices))
                {
                    edgeIndices = new List<int>();
                    faceToEdge.Add(edge.FaceIndex, edgeIndices);
                }

                edgeIndices.Add(i);

            }

            ManualUVRenderer uvRenderer = m_meshToRenderer[mesh];
            uvRenderer.UV = mesh.Textures;
            uvRenderer.IsSelected = new bool[uvRenderer.UV.Length];
            uvRenderer.Edges = meshEdges.ToArray(); 
            uvRenderer.FaceToEdges = faceToEdge;
            uvRenderer.Faces = mesh.Faces.ToArray();
            uvRenderer.Refresh(false, false);
        }

        private void UVSelectionToRenderer(ManualUVSelection selection)
        {
            ManualUVRenderer renderer = m_meshToRenderer[selection.Mesh];

            if (m_tool.Mode == ProBuilderToolMode.Vertex)
            {
                Color[] vertexColors = renderer.VertexColors;
                bool[] isSelected = renderer.IsSelected;
                for (int i = 0; i < vertexColors.Length; ++i)
                {
                    bool selected = selection.Vertices.Contains(i);
                    vertexColors[i] = selected ? m_selectionColor : Color.white;
                    isSelected[i] = selected;
                }
            }
            else if (m_tool.Mode == ProBuilderToolMode.Edge)
            {
                PBEdge[] edges = selection.Mesh.Edges;
                Color[] edgeColors = renderer.EdgeColors;
                bool[] isSelected = renderer.IsSelected;
                for (int i = 0; i < isSelected.Length; ++i)
                {
                    isSelected[i] = false;
                }

                for (int i = 0; i < edgeColors.Length; ++i)
                {
                    int edgeIndex = i / 2;
                    bool selected = selection.Edges.Contains(edgeIndex);

                    PBEdge edge = edges[edgeIndex];
                    if(renderer.Faces[edge.FaceIndex].IsManualUV)
                    {
                        edgeColors[i] = selected ? m_selectionColor : Color.white;
                    }
                    
                    isSelected[edge.A] = selected;
                    isSelected[edge.B] = selected;
                }
            }
            else
            {
                Color[] vertexColors = renderer.VertexColors;
                PBFace[] faces = renderer.Faces;
                bool[] isSelected = renderer.IsSelected;
                for (int i = 0; i < isSelected.Length; ++i)
                {
                    isSelected[i] = false;
                }
                for (int i = 0; i < vertexColors.Length; ++i)
                {
                    bool selected = selection.Faces.Contains(i);
                    
                    if(faces[i].IsManualUV)
                    {
                        vertexColors[i] = selected ? m_selectionColor : Color.white;
                    }
                    
                    if (selected)
                    {
                        int[] faceIdexes = faces[i].Indexes;
                        for (int j = 0; j < faceIdexes.Length; ++j)
                        {
                            int faceIndex = faceIdexes[j];
                            isSelected[faceIndex] = true;
                        }
                    }
                }

                Color[] edgeColors = renderer.EdgeColors;
                for (int i = 0; i < edgeColors.Length; ++i)
                {
                    if(edgeColors[i] == m_selectionColor)
                    {
                        edgeColors[i] = Color.white;
                    }
                }

                Dictionary<int, List<int>> faceToEdges = renderer.FaceToEdges;
                foreach (int faceIndex in selection.Faces)
                {
                    List<int> edgeIndices = faceToEdges[faceIndex];
                    for (int i = 0; i < edgeIndices.Count; ++i)
                    {
                        int edgeIndex = edgeIndices[i];
                        if(edgeColors[edgeIndex * 2 + 0] == Color.white)
                        {
                            edgeColors[edgeIndex * 2 + 0] = m_selectionColor;
                        }

                        if(edgeColors[edgeIndex * 2 + 1] == Color.white)
                        {
                            edgeColors[edgeIndex * 2 + 1] = m_selectionColor;
                        }
                    }
                }
            }

            renderer.Refresh(true, true);
        }

        private void OnUVChanged()
        {
            foreach (KeyValuePair<PBMesh, ManualUVRenderer> kvp in m_meshToRenderer)
            {
                PBMesh mesh = kvp.Key;
                ManualUVRenderer renderer = kvp.Value;
                renderer.UV = mesh.Textures;
                renderer.Refresh(true, false);
            }

            m_graphicsCamera.RenderersCache.Refresh();
        }
    }

}

