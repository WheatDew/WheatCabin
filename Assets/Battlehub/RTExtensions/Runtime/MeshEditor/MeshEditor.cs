using Battlehub.RTCommon;
using Battlehub.RTEditor;
using Battlehub.RTHandles;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace Battlehub.MeshTools
{
    public interface IMeshEditor
    {
        bool enabled
        {
            get;
            set;
        }
    }

    public class MeshEditor : SceneComponentExtension, IMeshEditor
    {
        private IRuntimeEditor m_editor;
        private IRuntimeSceneComponent m_scene;
        private ISelectionComponentState m_selectionComponentState;
        private IInput m_input;
        private List<EditableMesh> m_editableMeshes = new List<EditableMesh>();
        private Dictionary<EditableMesh, Tuple<Face, Face>[]> m_faces;

        private GameObject m_pivotPoint;
        private Quaternion m_initialRotation;
        private TBNBasis m_tbn;
        private Matrix4x4 m_objToTBN;
        private Vector3 m_uvPosition;
        private Vector3 m_uvScale;
        private Face m_lastFace;
        private EditableMesh LastMesh
        {
            get { return m_editableMeshes.Where(m => m != null).LastOrDefault(); }
        }

        protected override void Awake()
        {
            base.Awake();
            m_editor = IOC.Resolve<IRuntimeEditor>();
            
            m_pivotPoint = new GameObject("MeshEditorPivotPoint");
            m_pivotPoint.hideFlags = HideFlags.HideInHierarchy;
            m_pivotPoint.transform.SetParent(transform);
            ExposeToEditor exposeToEditor = m_pivotPoint.AddComponent<ExposeToEditor>();
            exposeToEditor.CanInspect = false;
            exposeToEditor.CanDelete = false;
            exposeToEditor.CanDuplicate = false;
            exposeToEditor.CanRename = false;
            exposeToEditor.CanCreatePrefab = false;

            LockAxes la = m_pivotPoint.AddComponent<LockAxes>();
            la.PositionZ = true;
            la.RotationX = la.RotationY = la.RotationScreen = la.RotationFree = true;
            la.ScaleZ = true;
            la.PivotRotation = true;
            la.PivotRotationValue = RuntimePivotRotation.Local;

            IOC.RegisterFallback<IMeshEditor>(this);

            if (m_editor != null)
            {
                m_editor.Selection.SelectionChanged += OnEditorSelectionChanged;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Destroy(m_pivotPoint.gameObject);

            if (m_editor != null)
            {
                m_editor.Selection.SelectionChanged -= OnEditorSelectionChanged;
            }

            IOC.UnregisterFallback<IMeshEditor>(this);
        }

        protected override void OnSceneActivated(IRuntimeSceneComponent sceneComponent)
        {
            base.OnSceneActivated(sceneComponent);
            if (sceneComponent.Window.WindowType != RuntimeWindowType.Scene)
            {
                return;
            }
            if (enabled)
            {
                if (m_selectionComponentState != null)
                {
                    Enable(false);
                }

                m_scene = sceneComponent;
                m_selectionComponentState = sceneComponent.Window.IOCContainer.Resolve<ISelectionComponentState>();
                m_input = m_scene.Window.Editor.Input;

                Enable(true);
            }
        }

        //protected override void OnSceneDeactivated(IRuntimeSceneComponent sceneComponent)
        //{
        //    base.OnSceneDeactivated(sceneComponent);

        //    if (m_selectionComponentState != null)
        //    {
        //        Enable(false);
        //        m_selectionComponentState = null;
        //    }

        //    m_scene = null;
        //    m_input = null;
        //}

        protected virtual void OnEnable()
        {
            m_scene = m_editor.GetWindow(RuntimeWindowType.Scene).IOCContainer.Resolve<IRuntimeSceneComponent>();
            m_selectionComponentState = m_editor.GetWindow(RuntimeWindowType.Scene).IOCContainer.Resolve<ISelectionComponentState>();
            m_input = m_scene.Window.Editor.Input;


            LockAxes la = m_pivotPoint.GetComponent<LockAxes>();
            la.PositionY = la.PositionX = false;
            la.ScaleX = la.ScaleY = false;
            la.RotationZ = false;


            if (m_selectionComponentState != null)
            {
                Enable(true);
            }

        }

        protected virtual void OnDisable()
        {
            if (m_pivotPoint != null)
            {
                LockAxes la = m_pivotPoint.GetComponent<LockAxes>();
                la.PositionY = la.PositionX = true;
                la.ScaleX = la.ScaleY = true;
                la.RotationZ = true;
            }

            if (m_selectionComponentState != null)
            {
                Enable(false);
            }

            m_scene = null;
            m_selectionComponentState = null;
            m_input = null;
        }

        protected virtual void Enable(bool value)
        {
            //m_selectionComponentState.IsBoxSelectionEnabled(this, !value);
            //m_selectionComponentState.CanSelect(this, !value);
            //m_selectionComponentState.CanSelectAll(this, !value);

            if (m_scene != null)
            {
                if (value)
                {
                    Subscribe();
                }
                else
                {
                    Unsubscribe();
                    if (m_scene.PositionHandle.IsDragging)
                    {
                        OnEndDrag(m_scene.PositionHandle);
                    }
                    else if (m_scene.RotationHandle.IsDragging)
                    {
                        OnEndDrag(m_scene.RotationHandle);
                    }
                    else if (m_scene.ScaleHandle.IsDragging)
                    {
                        OnEndDrag(m_scene.ScaleHandle);
                    }
                }
            }
        }

        private void Subscribe()
        {
            m_scene.PositionHandle.BeforeDrag.AddListener(OnBeginDrag);
            m_scene.PositionHandle.Drop.AddListener(OnEndDrag);

            m_scene.RotationHandle.BeforeDrag.AddListener(OnBeginDrag);
            m_scene.RotationHandle.Drop.AddListener(OnEndDrag);

            m_scene.ScaleHandle.BeforeDrag.AddListener(OnBeginDrag);
            m_scene.ScaleHandle.Drop.AddListener(OnEndDrag);
        }

        private void Unsubscribe()
        {
            m_scene.PositionHandle.BeforeDrag.RemoveListener(OnBeginDrag);
            m_scene.PositionHandle.Drop.RemoveListener(OnEndDrag);

            m_scene.RotationHandle.BeforeDrag.RemoveListener(OnBeginDrag);
            m_scene.RotationHandle.Drop.RemoveListener(OnEndDrag);

            m_scene.ScaleHandle.BeforeDrag.RemoveListener(OnBeginDrag);
            m_scene.ScaleHandle.Drop.RemoveListener(OnEndDrag);
        }

        protected virtual void LateUpdate()
        {
            if(m_scene == null)
            {
                return;
            }

            if(m_scene.PositionHandle.IsDragging)
            {
                EditableMesh lastMesh = LastMesh;
                if (lastMesh == null)
                {
                    return;
                }
                Vector3 uv = m_objToTBN.MultiplyPoint3x4(lastMesh.transform.InverseTransformPoint(m_pivotPoint.transform.position));
                Vector2 deltaUv = m_uvPosition - uv;

                foreach(var kvp in m_faces)
                {
                    EditableMesh editableMesh = kvp.Key;
                    if(editableMesh == null)
                    {
                        continue;
                    }

                    foreach (var tuple in kvp.Value)
                    {
                        Face face = tuple.Item1;
                        Face original = tuple.Item2;

                        face.UV0 = original.UV0 + deltaUv;
                        face.UV1 = original.UV1 + deltaUv;
                        face.UV2 = original.UV2 + deltaUv;
                    }

                    editableMesh.RefreshUVs(editableMesh.SelectedFaces);
                }
            }
            else if(m_scene.RotationHandle.IsDragging)
            {
                EditableMesh lastMesh = LastMesh;
                if (lastMesh == null)
                {
                    return;
                }

                foreach (var kvp in m_faces)
                {
                    EditableMesh editableMesh = kvp.Key;
                    if (editableMesh == null)
                    {
                        continue;
                    }

                    Quaternion tbnRotation = Quaternion.Inverse(QuaternionFromMatrix(editableMesh.transform.worldToLocalMatrix * m_objToTBN));
                    Quaternion initialRotation = tbnRotation * m_initialRotation;
                    Quaternion rotation = tbnRotation * m_pivotPoint.transform.rotation;
                    Quaternion delta = Quaternion.Inverse(initialRotation) * rotation;

                    foreach (var tuple in kvp.Value)
                    {
                        Face face = tuple.Item1;
                        Face original = tuple.Item2;
                        Vector2 uvPos = (original.UV0 + original.UV1 + original.UV2) / 3;
 
                        float mag0 = (original.UV0 - uvPos).magnitude;
                        float mag1 = (original.UV1 - uvPos).magnitude;
                        float mag2 = (original.UV2 - uvPos).magnitude;
                        
                        face.UV0 = uvPos + (Vector2)(delta * (original.UV0 - uvPos)).normalized * mag0;
                        face.UV1 = uvPos + (Vector2)(delta * (original.UV1 - uvPos)).normalized * mag1;
                        face.UV2 = uvPos + (Vector2)(delta * (original.UV2 - uvPos)).normalized * mag2;
                    }

                    editableMesh.RefreshUVs(editableMesh.SelectedFaces);
                }
            }
            else if(m_scene.ScaleHandle.IsDragging)
            {
                EditableMesh lastMesh = LastMesh;
                if(lastMesh == null)
                {
                    return;
                }

                Vector3 s = lastMesh.transform.InverseTransformVector(m_pivotPoint.transform.TransformVector(m_pivotPoint.transform.localScale));
                Vector3 uvScale = m_objToTBN.MultiplyVector(s);
                Vector2 scale = new Vector2(m_uvScale.x / uvScale.x, m_uvScale.y / uvScale.y);
                                
                foreach (var kvp in m_faces)
                {
                    EditableMesh editableMesh = kvp.Key;
                    if (editableMesh == null)
                    {
                        continue;
                    }

                    foreach (var tuple in kvp.Value)
                    {
                        Face face = tuple.Item1;
                        Face original = tuple.Item2;
                        Vector2 uvPos = (original.UV0 + original.UV1 + original.UV2) / 3;

                        face.UV0 = uvPos + Vector2.Scale((original.UV0 - uvPos), scale);
                        face.UV1 = uvPos + Vector2.Scale((original.UV1 - uvPos), scale);
                        face.UV2 = uvPos + Vector2.Scale((original.UV2 - uvPos), scale);
                    }

                    editableMesh.RefreshUVs(editableMesh.SelectedFaces);
                }
            }
            else
            {
                bool select = m_input.GetPointerDown(0);
                bool multi = m_input.GetKey(KeyCode.LeftShift);
                if (select)
                {
                    SelectionState prevState = new SelectionState(this);
                    if (!multi)
                    {
                        for(int i = 0; i < m_editableMeshes.Count; ++i)
                        {
                            EditableMesh editableMesh = m_editableMeshes[i];
                            editableMesh.UnselectFaces();
                        }
                        m_editableMeshes.Clear();
                    }

                    Ray ray = m_scene.Window.Pointer;
                    RaycastHit[] hits = Physics.RaycastAll(ray.origin, ray.direction).OrderBy(d => d.distance).ToArray();
                    for(int i = 0; i < hits.Length; ++i)
                    {
                        RaycastHit hit = hits[i];
                        EditableMesh editableMesh = hit.collider.GetComponent<EditableMesh>();
                        if (editableMesh != null)
                        {
                            if (editableMesh.SelectFace(m_scene.Window.Pointer, out m_lastFace))
                            {
                                m_pivotPoint.transform.position = editableMesh.transform.TransformPoint((m_lastFace.Position));
                                GetTNBBasis();

                                m_initialRotation = Quaternion.LookRotation(
                                    editableMesh.transform.TransformDirection(m_tbn.Normal),
                                    editableMesh.transform.TransformDirection(m_tbn.Tangent));

                                m_pivotPoint.transform.rotation = m_initialRotation;

                                m_scene.Selection.Select(m_pivotPoint, new[] { m_pivotPoint });

                                if (!m_editableMeshes.Contains(editableMesh))
                                {
                                    m_editableMeshes.Add(editableMesh);
                                }
                                else
                                {
                                    m_editableMeshes.Remove(editableMesh);
                                    m_editableMeshes.Add(editableMesh);
                                }

                                RecordSelection(prevState);
                                break;
                            }
                        }
                    }

                    if(m_editableMeshes.Count == 0 && m_editor.Selection.activeGameObject == m_pivotPoint)
                    {
                        m_editor.Selection.Select(null, null);
                        RecordSelection(prevState);
                    }
                }
            }
        }

        private void RecordSelection(SelectionState prevState)
        {
            SelectionState newState = new SelectionState(this);
            m_editor.Undo.CreateRecord(redoRecord =>
            {
                newState.Apply();
                return true;
            },
            undoRecord =>
            {
                prevState.Apply();
                return true;
            });
        }

        private MeshesState m_prevState;
        private bool m_wasUndoEnabled;

        private void OnBeginDrag(BaseHandle handle)
        {
            m_wasUndoEnabled = handle.EnableUndo;
            handle.EnableUndo = false;

            m_prevState = new MeshesState(m_pivotPoint.transform, m_editableMeshes.ToArray());

            SetFaceSelectionVisibility(false);
            if(m_lastFace != null)
            {
                GetTNBBasis();
                m_uvPosition = m_objToTBN.MultiplyPoint3x4(m_lastFace.Position); 
                
                EditableMesh lastMesh = LastMesh;
                if (lastMesh != null)
                {
                    Vector3 s = lastMesh.transform.InverseTransformVector(m_pivotPoint.transform.TransformVector(m_pivotPoint.transform.localScale));
                    m_uvScale = m_objToTBN.MultiplyVector(s);
                }

                for (int i = 0; i < m_editableMeshes.Count; ++i)
                {
                    EditableMesh editableMesh = m_editableMeshes[i];
                    editableMesh.Separate(editableMesh.SelectedFaces);
                }

                m_faces = m_editableMeshes.ToDictionary(k => k, v => v.SelectedFaces.Select(f => new Tuple<Face, Face>(f, new Face(f))).ToArray());
            }
        }

        private void OnEndDrag(BaseHandle handle)
        {
            handle.EnableUndo = m_wasUndoEnabled;

            SetFaceSelectionVisibility(true);
            EditableMesh editableMesh = m_editableMeshes.Where(m => m != null).LastOrDefault();
            if(editableMesh != null)
            {
                m_pivotPoint.transform.position = editableMesh.transform.TransformPoint((m_lastFace.Position));
                m_pivotPoint.transform.localScale = Vector3.one;

                GetTNBBasis();
                m_initialRotation = Quaternion.LookRotation(
                    editableMesh.transform.TransformDirection(m_tbn.Normal),
                    editableMesh.transform.TransformDirection(m_tbn.Tangent));
                m_pivotPoint.transform.rotation = m_initialRotation;

                MeshesState prevState = m_prevState;
                MeshesState newState = new MeshesState(m_pivotPoint.transform, m_editableMeshes.ToArray());

                m_editor.Undo.CreateRecord(redoRecord =>
                {
                    newState.Apply();
                    return true;
                },
                undoRecord =>
                {
                    prevState.Apply();
                    return true;
                });
            }

            m_faces = null;
        }

        public static Quaternion QuaternionFromMatrix(Matrix4x4 m)
        {
            Quaternion q = new Quaternion();
            q.w = Mathf.Sqrt(Mathf.Max(0, 1 + m[0, 0] + m[1, 1] + m[2, 2])) / 2;
            q.x = Mathf.Sqrt(Mathf.Max(0, 1 + m[0, 0] - m[1, 1] - m[2, 2])) / 2;
            q.y = Mathf.Sqrt(Mathf.Max(0, 1 - m[0, 0] + m[1, 1] - m[2, 2])) / 2;
            q.z = Mathf.Sqrt(Mathf.Max(0, 1 - m[0, 0] - m[1, 1] + m[2, 2])) / 2;
            q.x *= Mathf.Sign(q.x * (m[2, 1] - m[1, 2]));
            q.y *= Mathf.Sign(q.y * (m[0, 2] - m[2, 0]));
            q.z *= Mathf.Sign(q.z * (m[1, 0] - m[0, 1]));
            return q;
        }

        private void GetTNBBasis()
        {
            m_lastFace.GetFaceTBNBasis(out m_tbn);
            m_objToTBN = Matrix4x4.identity;
            m_objToTBN.SetColumn(1, m_tbn.Tangent);
            m_objToTBN.SetColumn(0, m_tbn.Binormal);
            m_objToTBN.SetColumn(2, m_tbn.Normal);
            m_objToTBN = m_objToTBN.inverse;
            m_objToTBN.SetColumn(3, new Vector4(0.5f, 0.5f, 0, 0));
        }

        private void SetFaceSelectionVisibility(bool isSelectionVisible)
        {
            for (int i = 0; i < m_editableMeshes.Count; ++i)
            {
                EditableMesh editableMesh = m_editableMeshes[i];
                if (editableMesh != null)
                {
                    editableMesh.IsSelectionVisible = isSelectionVisible;
                }
            }
        }

        private void OnEditorSelectionChanged(UnityEngine.Object[] unselectedObjects)
        {
            if (!m_editor.Selection.IsSelected(m_pivotPoint))
            {
                for (int i = 0; i < m_editableMeshes.Count; ++i)
                {
                    EditableMesh editableMesh = m_editableMeshes[i];
                    editableMesh.SelectedFaces = null;
                }
            }
        }

        private class SelectionState
        {
            private MeshEditor Editor;
            private EditableMesh[] EditableMeshes;
            private Face LastFace;
            private Face[][] SelectedFaces;
            private Vector3 PivotPointPosition;
            private Quaternion PivotPointRotation;
            private Vector3 PivotPointScale;
            private UnityEngine.Object[] Selection;
            private UnityEngine.Object ActiveObject;

            public SelectionState(MeshEditor editor)
            {
                Editor = editor;
                EditableMeshes = editor.m_editableMeshes.ToArray();
                LastFace = editor.m_lastFace;

                IRTE rte = IOC.Resolve<IRTE>();
                ActiveObject = rte.Selection.activeObject;
                Selection = rte.Selection.objects;
                
                SelectedFaces = new Face[EditableMeshes.Length][];
                for (int i = 0; i < EditableMeshes.Length; ++i)
                {
                    SelectedFaces[i] = EditableMeshes[i].SelectedFaces.ToArray();
                }
                Transform pivotPoint = editor.m_pivotPoint.transform;
                PivotPointPosition = pivotPoint.position;
                PivotPointRotation = pivotPoint.rotation;
                PivotPointScale = pivotPoint.localScale;
            }

            public void Apply()
            {
                Transform pivotPoint = Editor.m_pivotPoint.transform;
                pivotPoint.position = PivotPointPosition;
                pivotPoint.rotation = PivotPointRotation;
                pivotPoint.localScale = PivotPointScale;

                for (int i = 0; i < Editor.m_editableMeshes.Count; ++i)
                {
                    Editor.m_editableMeshes[i].SelectedFaces = null;
                }

                for (int i = 0; i < EditableMeshes.Length; ++i)
                {
                    EditableMeshes[i].SelectedFaces = SelectedFaces[i];
                }
                Editor.m_editableMeshes = EditableMeshes.ToList();
                Editor.m_lastFace = LastFace;

                IRTE rte = IOC.Resolve<IRTE>();
                if (rte != null)
                {
                    rte.Selection.Select(ActiveObject, Selection);
                }
            }
        }

        private class MeshesState
        {
            private EditableMesh[] EditableMeshes;
            private Vector3[][] Vertices;
            private Vector3[][] Normals;
            private Vector2[][] UV;
            private int[][] Triangles;
            private Tuple<uint, uint>[][] Descriptors;

            private Vector3 PivotPointPosition;
            private Quaternion PivotPointRotation;
            private Vector3 PivotPointScale;

            private Transform PivotPoint;

            public MeshesState(Transform PivotPoint, EditableMesh[] meshes)
            {
                EditableMeshes = meshes;
                Vertices = new Vector3[meshes.Length][];
                UV = new Vector2[meshes.Length][];
                Normals = new Vector3[meshes.Length][];
                Triangles = new int[meshes.Length][];
                Descriptors = new Tuple<uint, uint>[meshes.Length][];
                for (int i = 0; i < EditableMeshes.Length; ++i)
                {
                    EditableMesh editableMesh = EditableMeshes[i];
                    Vertices[i] = editableMesh.Mesh.vertices;
                    UV[i] = editableMesh.Mesh.uv;
                    Normals[i] = editableMesh.Mesh.normals;
                    Triangles[i] = editableMesh.Mesh.triangles;

                    Mesh mesh = editableMesh.Mesh;
                    Descriptors[i] = new Tuple<uint, uint>[mesh.subMeshCount];
                    for(int s = 0; s < mesh.subMeshCount; ++s)
                    {
                        Descriptors[i][s] = new Tuple<uint, uint>(mesh.GetIndexStart(s), mesh.GetIndexCount(s));
                    }
                }

                this.PivotPoint = PivotPoint;
                PivotPointPosition = PivotPoint.position;
                PivotPointRotation = PivotPoint.rotation;
                PivotPointScale = PivotPoint.localScale;
            }

            public void Apply()
            {
                for (int i = 0; i < EditableMeshes.Length; ++i)
                {
                    EditableMesh editableMesh = EditableMeshes[i];
                    Mesh mesh = editableMesh.Mesh;
                    int submeshCount = mesh.subMeshCount;
                    mesh.Clear();
                    mesh.subMeshCount = submeshCount;

                    mesh.vertices = Vertices[i];
                    mesh.uv = UV[i];
                    mesh.normals = Normals[i];

                    mesh = editableMesh.Mesh;
                    for (int s = 0; s < mesh.subMeshCount; ++s)
                    {
                        var desc = mesh.GetSubMesh(s);
                        mesh.SetTriangles(Triangles[i], (int)Descriptors[i][s].Item1, (int)Descriptors[i][s].Item2, s);
                    }
                }

                PivotPoint.position = PivotPointPosition;
                PivotPoint.rotation = PivotPointRotation;
                PivotPoint.localScale = PivotPointScale;
            }
        }
    }
}

