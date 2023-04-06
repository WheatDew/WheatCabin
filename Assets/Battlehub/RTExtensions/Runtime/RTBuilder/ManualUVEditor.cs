using Battlehub.ProBuilderIntegration;
using Battlehub.RTCommon;
using Battlehub.RTEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Battlehub.RTBuilder
{
    public interface IManualUVEditor
    {
        event Action UVChanged;
        event Action<ManualUVSelection> SelectionChanged;
        event Action<ManualUVSelection> MeshSelected;
     
        IRuntimeSelection PivotPointSelection
        {
            get;
        }

        IEnumerable<ManualUVSelection> Selection
        {
            get;
        }

        void BeginMove();
        void Move();
        void EndMove();

        void BeginRotate();
        void Rotate();
        void EndRotate();

        void BeginScale();
        void Scale();
        void EndScale();

        void SelectVertices(Func<Vector2, float> canSelect, bool selectClosest, bool clearSelection);
        void SelectEdges(Func<Vector2, Vector2, float> canSelect, bool selectClosest, bool clearSelection);
        void SelectFaces(Func<Vector2, float> canSelect, bool selectClosest, bool clearSelection);
        void RefreshPivotPoint();
    }

    public class ManualUVSelection
    {
        public PBMesh Mesh
        {
            get { return GameObject.GetComponent<PBMesh>(); }
        }

        public GameObject GameObject;
        public readonly HashSet<int> Vertices = new HashSet<int>();
        public readonly HashSet<int> Faces = new HashSet<int>();
        public readonly HashSet<int> Edges = new HashSet<int>();

        public ManualUVSelection(GameObject gameObject)
        {
            GameObject = gameObject;
        }

        public ManualUVSelection(ManualUVSelection selection)
        {
            GameObject = selection.GameObject;
            Vertices = new HashSet<int>(selection.Vertices);
            Faces = new HashSet<int>(selection.Faces);
            Edges = new HashSet<int>(selection.Edges);
        }

        public void Clear()
        {
            Vertices.Clear();
            Faces.Clear();
            Edges.Clear();
        }
    }

    public class ManualUVEditor : MonoBehaviour, IManualUVEditor
    {
        public event Action UVChanged;
        public event Action<ManualUVSelection> SelectionChanged;
        public event Action<ManualUVSelection> MeshSelected;
        
        private readonly List<ManualUVSelection> m_selection = new List<ManualUVSelection>();
        public IEnumerable<ManualUVSelection> Selection
        {
            get { return m_selection; }
        }

        private IRuntimeSelection m_pivotPointSelection;
        public IRuntimeSelection PivotPointSelection
        {
            get { return m_pivotPointSelection; }
        }

        private IRuntimeEditor m_editor;
        private IProBuilderTool m_tool;
        private Vector3 m_prevPivotPoint;
        private Transform m_pivotPoint;
        private Vector2 m_initialUV;
        private Vector2[][] m_initialUVs;
        private int[][] m_indexes;

        private void Awake()
        {
            IOC.RegisterFallback<IManualUVEditor>(this);
            m_editor = IOC.Resolve<IRuntimeEditor>();
            m_tool = IOC.Resolve<IProBuilderTool>();
            m_tool.SelectionChanged += OnSelectionChanged;
            m_tool.ModeChanged += OnModeChanged;
            
            m_pivotPointSelection = new RuntimeSelection(IOC.Resolve<IRTE>());

            m_pivotPoint = new GameObject("ManualUVEditorPivotPoint").transform;
            m_pivotPoint.SetParent(transform, false);
            LockAxes la = m_pivotPoint.gameObject.AddComponent<LockAxes>();
            la.PositionZ = true;
            la.RotationX = la.RotationY = la.RotationFree = la.RotationScreen = true;
            la.ScaleZ = true;
            la.PivotRotationValue = RuntimePivotRotation.Global;
            la.PivotRotation = true;
        }

        private void OnDestroy()
        {
            IOC.UnregisterFallback<IManualUVEditor>(this);
            if(m_tool != null)
            {
                m_tool.SelectionChanged -= OnSelectionChanged;
                m_tool.ModeChanged -= OnModeChanged;
            }
        }

        private void Update()
        {
            if(m_prevPivotPoint != m_pivotPoint.position)
            {
                BeginMove();
                Vector2 uv = ManualUVRenderer.WorldToUV(m_pivotPoint.transform.position);
                Vector2 delta = uv - ManualUVRenderer.WorldToUV(m_prevPivotPoint); 
                Move(delta);
                m_prevPivotPoint = m_pivotPoint.transform.position;
            }
        }

        private void GetIndexes()
        {
            if (m_tool.Mode == ProBuilderToolMode.Vertex)
            {
                m_indexes = m_selection.Select(s => s.Vertices.ToArray()).ToArray();
            }
            else if (m_tool.Mode == ProBuilderToolMode.Face)
            {
                m_indexes = m_selection.Select(s =>
                {
                    HashSet<int> indexes = new HashSet<int>();
                    PBFace[] faces = s.Mesh.Faces;
                    foreach (int faceIndex in s.Faces)
                    {
                        PBFace face = faces[faceIndex];
                        for (int i = 0; i < face.Indexes.Length; ++i)
                        {
                            if (!indexes.Contains(face.Indexes[i]))
                            {
                                indexes.Add(face.Indexes[i]);
                            }
                        }
                    }
                    return indexes.ToArray();
                }).ToArray();
            }
            else
            {
                m_indexes = m_selection.Select(s =>
                {
                    HashSet<int> indexes = new HashSet<int>();
                    PBEdge[] edges = s.Mesh.Edges;
                    foreach (int edgeIndex in s.Edges)
                    {
                        PBEdge edge = edges[edgeIndex];
                        if (!indexes.Contains(edge.A))
                        {
                            indexes.Add(edge.A);
                        }
                        if (!indexes.Contains(edge.B))
                        {
                            indexes.Add(edge.B);
                        }
                    }
                    return indexes.ToArray();
                }).ToArray();
            }
        }

        public void BeginMove()
        {
            m_initialUV = ManualUVRenderer.WorldToUV(m_pivotPoint.transform.position);
            m_initialUVs = m_selection.Select(s => s.Mesh.Textures.ToArray()).ToArray();
            GetIndexes();
        }

        public void Move()
        {
            Vector2 uv = ManualUVRenderer.WorldToUV(m_pivotPoint.transform.position);
            Vector2 delta = uv - m_initialUV;
            Move(delta);
            m_prevPivotPoint = m_pivotPoint.transform.position;

        }

        private void Move(Vector2 delta)
        {
            for (int i = 0; i < m_selection.Count; ++i)
            {
                ManualUVSelection selection = m_selection[i];
                PBMesh mesh = selection.Mesh;
                Vector2[] textures = mesh.Textures;
                foreach (int index in m_indexes[i])
                {
                    textures[index] = m_initialUVs[i][index] + delta;
                }
                mesh.Textures = textures;
                mesh.RefreshUV();
            }

            if (UVChanged != null)
            {
                UVChanged();
            }
        }

        public void EndMove()
        {    
            RecordUV(m_selection.Select(s => s.GameObject).ToArray(), m_initialUVs, m_selection.Select(s => s.Mesh.Textures.ToArray()).ToArray());
            RefreshPivotPoint();
        }

        public void BeginRotate()
        {
            m_initialUVs = m_selection.Select(s => s.Mesh.Textures.ToArray()).ToArray();
            GetIndexes();
        }

        public void Rotate()
        {
            Vector2 uv = ManualUVRenderer.WorldToUV(m_pivotPoint.transform.position);
            float angle = -m_pivotPoint.transform.eulerAngles.z;
            
            for (int i = 0; i < m_selection.Count; ++i)
            {
                ManualUVSelection selection = m_selection[i];
                PBMesh mesh = selection.Mesh;
                Vector2[] textures = mesh.Textures;
                foreach (int index in m_indexes[i])
                {
                    textures[index] = (m_initialUVs[i][index]).RotateAroundPoint(uv, angle);
                }
                mesh.Textures = textures;
                mesh.RefreshUV();
            }

            if (UVChanged != null)
            {
                UVChanged();
            }
        }

        public void EndRotate()
        {
            m_pivotPoint.rotation = Quaternion.identity;
            RecordUV(m_selection.Select(s => s.GameObject).ToArray(), m_initialUVs, m_selection.Select(s => s.Mesh.Textures.ToArray()).ToArray());
        }

        public void BeginScale()
        {
            m_initialUV = ManualUVRenderer.WorldToUV(m_pivotPoint.transform.position);
            m_initialUVs = m_selection.Select(s => s.Mesh.Textures.ToArray()).ToArray();
            GetIndexes();
        }

        public void Scale()
        {
            Vector2 scale = m_pivotPoint.transform.localScale;

            for (int i = 0; i < m_selection.Count; ++i)
            {
                ManualUVSelection selection = m_selection[i];
                PBMesh mesh = selection.Mesh;
                Vector2[] textures = mesh.Textures;
                foreach (int index in m_indexes[i])
                {
                    textures[index] = m_initialUV + Vector2.Scale((m_initialUVs[i][index] - m_initialUV), scale);
                }
                mesh.Textures = textures;
                mesh.RefreshUV();
            }

            if (UVChanged != null)
            {
                UVChanged();
            }
        }

        public void EndScale()
        {
            m_pivotPoint.localScale = Vector3.one;
            RecordUV(m_selection.Select(s => s.GameObject).ToArray(), m_initialUVs, m_selection.Select(s => s.Mesh.Textures.ToArray()).ToArray());
        }

        private void OnSelectionChanged()
        {
            RefreshSelectionAndPivotPoint();
        }

        private void OnModeChanged(ProBuilderToolMode obj)
        {
            RefreshSelectionAndPivotPoint();
        }

        private void RefreshSelectionAndPivotPoint()
        {
            m_selection.Clear();

            IMeshEditor editor = m_tool.GetEditor();
            if(editor == null)
            {
                return;
            }

            MeshSelection meshSelection = editor.GetSelection();
            if (meshSelection != null)
            {
                foreach (PBMesh mesh in meshSelection.GetSelectedMeshes())
                {
                    ManualUVSelection uvSelection = new ManualUVSelection(mesh.gameObject);
                    m_selection.Add(uvSelection);

                    if (MeshSelected != null)
                    {
                        MeshSelected(uvSelection);
                    }
                }

                Select(meshSelection);
            }

            RefreshPivotPoint();
        }

        private void Select(MeshSelection meshSelection)
        {
            for (int index = 0; index < m_selection.Count; ++index)
            {
                ManualUVSelection selection = m_selection[index];
                ManualUVSelection oldSelection = new ManualUVSelection(selection);

                selection.Clear();

                if(meshSelection.HasVertices)
                {
                    foreach(int i in meshSelection.GetIndices(selection.Mesh))
                    {
                        selection.Vertices.Add(i);
                    }
                }
                else if(meshSelection.HasEdges)
                {
                    foreach (int i in meshSelection.GetEdges(selection.Mesh))
                    {
                        selection.Edges.Add(i);
                    }
                }
                else if (meshSelection.HasFaces)
                {
                    foreach (int i in meshSelection.GetFaces(selection.Mesh))
                    {
                        selection.Faces.Add(i);
                    }
                }

                if (SelectionChanged != null)
                {
                    SelectionChanged(selection);
                }
            }
        }

        public void SelectVertices(Func<Vector2, float> canSelect, bool selectClosest, bool clearSelection)
        {
            m_editor.Undo.BeginRecord();
            for (int index = 0; index < m_selection.Count; ++index)
            {                
                ManualUVSelection selection = m_selection[index];
                ManualUVSelection oldSelection = new ManualUVSelection(selection);

                if (clearSelection)
                {
                    selection.Clear();
                }

                PBMesh mesh = selection.Mesh;
                Vector2[] uvs = mesh.Textures;
                float minDistance = float.PositiveInfinity;
                int closestIndex = -1;
                for (int i = 0; i < uvs.Length; ++i)
                {
                    float distance = canSelect(uvs[i]);
                    if (selectClosest)
                    {
                        if (distance < minDistance)
                        {
                            closestIndex = i;
                            minDistance = distance;
                        }
                    }
                    else
                    {
                        if (distance <= 0)
                        {
                            if (!selection.Vertices.Contains(i))
                            {
                                selection.Vertices.Add(i);
                            }
                        }
                    }
                }

                if (closestIndex >= 0)
                {
                    if (!selection.Vertices.Contains(closestIndex))
                    {
                        selection.Vertices.Add(closestIndex);
                    }
                }

                RecordSelection(index, oldSelection, new ManualUVSelection(selection));

                if(SelectionChanged != null)
                {
                    SelectionChanged(selection);
                }
            }

            m_editor.Undo.EndRecord();
        }

        public void SelectEdges(Func<Vector2, Vector2, float> canSelect, bool selectClosest, bool clearSelection)
        {
            m_editor.Undo.BeginRecord();
            for (int index = 0; index < m_selection.Count; ++index)
            {
                ManualUVSelection selection = m_selection[index];
                ManualUVSelection oldSelection = new ManualUVSelection(selection);

                if (clearSelection)
                {
                    selection.Clear();
                }

                PBMesh mesh = selection.Mesh;
                Vector2[] uvs = mesh.Textures;
                PBEdge[] edges = mesh.Edges;
                float minDistance = float.PositiveInfinity;
                int closestEdgeIndex = -1;
                for (int i = 0; i < edges.Length; ++i)
                {
                    PBEdge edge = edges[i];

                    float distance = canSelect(uvs[edge.A], uvs[edge.B]);
                    if (selectClosest)
                    {
                        if (distance < minDistance)
                        {
                            closestEdgeIndex = i;
                            minDistance = distance;
                        }
                    }
                    else
                    {
                        if (distance <= 0)
                        {
                            if (!selection.Edges.Contains(i))
                            {
                                selection.Edges.Add(i);
                            }
                        }
                    }
                }

                if (closestEdgeIndex >= 0)
                {
                    if (!selection.Edges.Contains(closestEdgeIndex))
                    {
                        selection.Edges.Add(closestEdgeIndex);
                    }
                }

                RecordSelection(index, oldSelection, new ManualUVSelection(selection));

                if (SelectionChanged != null)
                {
                    SelectionChanged(selection);
                }
            }

            m_editor.Undo.EndRecord();
        }

        public void SelectFaces(Func<Vector2, float> canSelect, bool selectClosest, bool clearSelection)
        {
            m_editor.Undo.BeginRecord();
            for (int index = 0; index < m_selection.Count; ++index)
            {
                ManualUVSelection selection = m_selection[index];
                ManualUVSelection oldSelection = new ManualUVSelection(selection);

                if (clearSelection)
                {
                    selection.Clear();
                }

                PBMesh mesh = selection.Mesh;
                Vector2[] meshUV = mesh.Textures;
                PBFace[] meshFaces = mesh.Faces;
                float minDistance = float.PositiveInfinity;
                int closestIndex = -1;
                for (int i = 0; i < meshFaces.Length; ++i)
                {
                    PBFace face = meshFaces[i];
                    Vector2 uv = meshUV[face.Indexes[0]];
                    for (int j = 1; j < face.Indexes.Length; ++j)
                    {
                        uv += meshUV[face.Indexes[j]];
                    }
                    uv /= face.Indexes.Length;

                    float distance = canSelect(uv);
                    if (selectClosest)
                    {
                        if (distance < minDistance)
                        {
                            closestIndex = i;
                            minDistance = distance;
                        }
                    }
                    else
                    {
                        if (distance <= 0)
                        {
                            if (!selection.Faces.Contains(i))
                            {
                                selection.Faces.Add(i);
                            }
                        }
                    }
                }

                if (closestIndex >= 0)
                {
                    if (!selection.Faces.Contains(closestIndex))
                    {
                        selection.Faces.Add(closestIndex);
                    }
                }

                RecordSelection(index, oldSelection, new ManualUVSelection(selection));

                if (SelectionChanged != null)
                {
                    SelectionChanged(selection);
                }
            }

            m_editor.Undo.EndRecord();
        }

        public void RefreshPivotPoint()
        {
            Vector2 pivotPosition = Vector2.zero;
            int count = 0;
            switch (m_tool.Mode)
            {
                case ProBuilderToolMode.Vertex:
                    foreach (ManualUVSelection selection in m_selection)
                    {
                        PBMesh mesh = selection.Mesh;
                        
                        Vector2[] uvs = mesh.Textures;
                        foreach (int index in selection.Vertices)
                        {
                            count++;
                            if (count == 1)
                            {
                                pivotPosition = uvs[index];
                            }
                            else
                            {
                                pivotPosition *= (count - 1) / (float)count;
                                pivotPosition += uvs[index] / count;
                            }
                        }
                    }
                    break;
                case ProBuilderToolMode.Face:
                    foreach (ManualUVSelection selection in m_selection)
                    {
                        PBMesh mesh = selection.Mesh;
                        Vector2[] uvs = mesh.Textures;
                        PBFace[] faces = mesh.Faces;
                        foreach (int index in selection.Faces)
                        {
                            PBFace face = faces[index];
                            int[] faceIndexes = face.Indexes;
                            Vector2 faceUV = uvs[faceIndexes[0]];
                            for (int i = 1; i < faceIndexes.Length; ++i)
                            {
                                faceUV += uvs[faceIndexes[i]];
                            }
                            faceUV /= faceIndexes.Length;

                            count++;
                            if (count == 1)
                            {
                                pivotPosition = faceUV;
                            }
                            else
                            {
                                pivotPosition *= (count - 1) / (float)count;
                                pivotPosition += faceUV / count;
                            }
                        }
                    }
                    break;
                default:
                    foreach (ManualUVSelection selection in m_selection)
                    {
                        PBMesh mesh = selection.Mesh;
                        Vector2[] uvs = mesh.Textures;
                        PBEdge[] edges = mesh.Edges;
                        foreach (int index in selection.Edges)
                        {
                            PBEdge edge = edges[index];
                            Vector2 edgeUV = (uvs[edge.A] + uvs[edge.B]) * 0.5f;

                            count++;
                            if (count == 1)
                            {
                                pivotPosition = edgeUV;
                            }
                            else
                            {
                                pivotPosition *= (count - 1) / (float)count;
                                pivotPosition += edgeUV / count;
                            }
                        }
                    }
                    break;
            }

            pivotPosition *= ManualUVRenderer.Scale;
            m_pivotPoint.position = new Vector3(pivotPosition.x, pivotPosition.y, -1);
            m_pivotPoint.rotation = Quaternion.identity;
            m_pivotPoint.localScale = Vector3.one;
            m_prevPivotPoint = m_pivotPoint.position;

            if (count == 0)
            {
                m_pivotPointSelection.Select(null, null);
            }
            else
            {
                m_pivotPointSelection.Select(m_pivotPoint.gameObject, new[] { m_pivotPoint.gameObject });
            }
        }

        private void RecordSelection(int index, ManualUVSelection oldSelection, ManualUVSelection newSelection)
        {
            m_editor.Undo.CreateRecord(record =>
            {
                m_selection[index] = newSelection;
                if(SelectionChanged != null)
                {
                    SelectionChanged(newSelection);
                }
                RefreshPivotPoint();
                return true;
            },
            record =>
            {
                m_selection[index] = oldSelection;
                if (SelectionChanged != null)
                {
                    SelectionChanged(oldSelection);
                }
                RefreshPivotPoint();
                return true;
            });
        }

        private void RecordUV(GameObject[] gameObjects, Vector2[][] oldUv, Vector2[][] newUv)
        {
            m_editor.Undo.CreateRecord(record =>
            {
                for(int i = 0; i < gameObjects.Length; ++i)
                {
                    GameObject go = gameObjects[i];
                    PBMesh mesh = go.GetComponent<PBMesh>();
                    mesh.Textures = newUv[i];
                    mesh.RefreshUV();
                }

                RefreshPivotPoint();

                if (UVChanged != null)
                {
                    UVChanged();
                }
                return true;
            },
            record =>
            {

                for (int i = 0; i < gameObjects.Length; ++i)
                {
                    GameObject go = gameObjects[i];
                    PBMesh mesh = go.GetComponent<PBMesh>();
                    mesh.Textures = oldUv[i];
                    mesh.RefreshUV();
                }

                RefreshPivotPoint();

                if (UVChanged != null)
                {
                    UVChanged();
                }

                return true;
            });
        }
    }
}
