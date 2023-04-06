using Battlehub.RTCommon;
using Battlehub.RTEditor;
using Battlehub.RTHandles;
using Battlehub.Spline3;
using System;
using UnityEngine;

namespace Battlehub.MeshDeformer3
{
    public enum MeshDeformerToolMode
    {
        Object = 0,
        ControlPoint = 1,
        Settings = 2,
    }

    public interface IMeshDeformerTool
    {
        event Action SelectionChanged;
        event Action ModeChanged;

        MeshDeformerToolMode Mode
        {
            get;
            set;
        }

        bool ShowTerminalPoints
        {
            get;
            set;
        }

        int PointsPerSegment
        {
            get;
            set;
        }

        bool ShowOriginal
        {
            get;
            set;
        }

        void SelectControlPoint(Camera camera, Vector2 point);
        bool DragControlPoint(bool extend);
        bool CanDeform();
        void DeformAxis(Axis axis);
        bool CanSubdivide();
        void Subdivide();
        bool CanAppend();
        void Append();
        bool CanRemove();
        void Remove();
        bool CanDestroy();
        void Destroy();
    }

    [DefaultExecutionOrder(-90)]
    public class MeshDeformerTool : MonoBehaviour, IMeshDeformerTool
    {
        public event Action SelectionChanged;
        public event Action ModeChanged;

        private ISelectionComponentState m_selectionComponentState;
        private ControlPointPicker m_controlPointPicker;
        private IRTE m_editor;

        private MeshDeformerToolMode m_mode = MeshDeformerToolMode.Object;
        public MeshDeformerToolMode Mode
        {
            get { return m_mode; }
            set
            {
                if (m_mode != value)
                {
                    m_mode = value;
                    UnsubscribeEvents();
                    if (m_mode != MeshDeformerToolMode.Object)
                    {
                        SetupSelectionComponentAndSubscribeEvents();
                        m_editor.ActiveWindowChanged += OnActiveWindowChanged;
                        EnableSplineRenderers(true);
                    }
                    else
                    {
                        SetCanSelect(true);
                        m_editor.ActiveWindowChanged -= OnActiveWindowChanged;
                        EnableSplineRenderers(false);
                    }

                    if (ModeChanged != null)
                    {
                        ModeChanged();
                    }
                }
            }
        }

        private Deformer m_selectedDeformer;
        private Deformer SelectedDeformer
        {
            get { return m_selectedDeformer; }
        }


        public bool ShowTerminalPoints
        {
            get { return SelectedDeformer != null ? SelectedDeformer.ShowTerminalPoints : false; }
            set
            {
                if (SelectedDeformer != null)
                {
                    SelectedDeformer.ShowTerminalPoints = value;
                    SelectedDeformer.Refresh();
                }
            }
        }

        public int PointsPerSegment
        {
            get { return SelectedDeformer != null ? SelectedDeformer.PointsPerSegment : 0; }
            set
            {
                if (SelectedDeformer != null)
                {
                    SelectedDeformer.PointsPerSegment = value;
                    SelectedDeformer.Refresh();
                }
            }
        }

        private bool m_showOriginal;
        public bool ShowOriginal
        {
            get { return m_showOriginal; }
            set
            {
                m_showOriginal = value;
                if(SelectedDeformer != null)
                {
                    SelectedDeformer.IsOriginalMeshVisible = m_showOriginal;
                }
            }
        }

        private void Awake()
        {
            m_editor = IOC.Resolve<IRTE>();
            m_editor.Tools.ToolChanged += OnEditorToolChanged;
            m_editor.Selection.SelectionChanged += OnEditorSelectionChanged;

            IOC.RegisterFallback<IMeshDeformerTool>(this);

            GameObject controlPointPicker = new GameObject("ControlPointPicker");

            controlPointPicker.transform.SetParent(transform, false);
            controlPointPicker.gameObject.SetActive(false);
            controlPointPicker.hideFlags = HideFlags.HideInHierarchy;
            ExposeToEditor exposeToEditor = controlPointPicker.AddComponent<ExposeToEditor>();
            exposeToEditor.CanInspect = false;
            controlPointPicker.gameObject.SetActive(true);

            m_controlPointPicker = controlPointPicker.AddComponent<ControlPointPicker>();
            m_controlPointPicker.SelectionChanged += OnPickerSelectionChanged;

            Deformer.Refreshed += OnDeformerRefreshed;
        }

        private void OnActiveWindowChanged(RuntimeWindow arg)
        {
            SetupSelectionComponentAndSubscribeEvents();
        }

        private void OnDestroy()
        {
            Deformer.Refreshed -= OnDeformerRefreshed;

            if (m_editor != null)
            {
                m_editor.Tools.ToolChanged -= OnEditorToolChanged;
                m_editor.ActiveWindowChanged -= OnActiveWindowChanged;
                m_editor.Selection.SelectionChanged -= OnEditorSelectionChanged;
            }

            if(m_controlPointPicker != null)
            {
                m_controlPointPicker.SelectionChanged -= OnPickerSelectionChanged;
            }

            UnsubscribeEvents();

            if (m_selectionComponentState != null)
            {
                m_selectionComponentState.CanSelect(this, true);
                m_selectionComponentState.IsBoxSelectionEnabled(this, true);
                m_selectionComponentState.CanSelectAll(this, true);
            }

            m_selectionComponentState = null;

            EnableSplineRenderers(false);

            IOC.UnregisterFallback<IMeshDeformerTool>(this);
        }

        private void OnPickerSelectionChanged()
        {
            GetSelectedDeformer();
            if (SelectionChanged != null)
            {
                SelectionChanged();
            }
        }

        private void OnEditorToolChanged()
        {
            if(m_editor.Tools.Current == RuntimeTool.None || m_editor.Tools.Current == RuntimeTool.Custom)
            {
                Mode = MeshDeformerToolMode.Object;
            }
        }

        private void OnEditorSelectionChanged(UnityEngine.Object[] unselectedObjects)
        {
            GetSelectedDeformer();
        }

        private void GetSelectedDeformer()
        {
            m_selectedDeformer = m_controlPointPicker.Selection != null && m_controlPointPicker.Selection.Spline != null ? m_controlPointPicker.Selection.Spline.GetComponentInParent<Deformer>() : null;
            if (m_selectedDeformer == null)
            {
                m_selectedDeformer = m_editor.Selection.activeGameObject != null ? m_editor.Selection.activeGameObject.GetComponentInParent<Deformer>() : null;
            }
        }

        private void SetupSelectionComponentAndSubscribeEvents()
        {
            UnsubscribeEvents();

            if (m_editor.ActiveWindow != null)
            {
                SetCanSelect(true);
                m_selectionComponentState = m_editor.ActiveWindow.IOCContainer.Resolve<ISelectionComponentState>();
                if (Mode != MeshDeformerToolMode.Object)
                {
                    SetCanSelect(false);
                }
                SubscribeEvents();
            }
            else
            {
                SetCanSelect(true);
            }
        }

        private void SetCanSelect(bool value)
        {
            if (m_selectionComponentState != null)
            {
                m_selectionComponentState.CanSelect(this, value);
                m_selectionComponentState.IsBoxSelectionEnabled(this, value);
                m_selectionComponentState.CanSelectAll(this, value);
            }
        }

        private void UnsubscribeEvents()
        {
            if (m_selectionComponentState == null || m_selectionComponentState.Component.PositionHandle == null)
            {
                return;
            }

            m_selectionComponentState.Component.PositionHandle.BeforeDrag.RemoveListener(OnPositionHandleBeforeDrag);
            m_selectionComponentState.Component.PositionHandle.Drop.RemoveListener(OnPositionHandleDrop);
        }

        private void SubscribeEvents()
        {
            if (m_selectionComponentState == null || m_selectionComponentState.Component.PositionHandle == null)
            {
                return;
            }

            m_selectionComponentState.Component.PositionHandle.BeforeDrag.AddListener(OnPositionHandleBeforeDrag);
            m_selectionComponentState.Component.PositionHandle.Drop.AddListener(OnPositionHandleDrop);
        }

        public void EnableSplineRenderers(bool enable)
        {
            SplineRenderer[] splineRenderers = FindObjectsOfType<SplineRenderer>();
            for (int i = 0; i < splineRenderers.Length; ++i)
            {
                SplineRenderer splineRenderer = splineRenderers[i];
                if(splineRenderer.GetComponent<Deformer>() != null)
                {
                    splineRenderer.Layer = m_editor.CameraLayerSettings.AllScenesLayer;
                    splineRenderer.enabled = enable;
                }
            }
        }

        public void SelectControlPoint(Camera camera, Vector2 point)
        {
            if(m_selectionComponentState == null)
            {
                return;
            }

            if (m_selectionComponentState.Component.IsPositionHandleEnabled)
            {
                BaseSpline spline = m_controlPointPicker.Selection != null ? m_controlPointPicker.Selection.GetSpline() : null;
                if (spline != null && !(spline is Deformer))
                {
                    return;
                }

                m_editor.Undo.BeginRecord();
                PickResult oldSelection = m_controlPointPicker.Selection != null ? new PickResult(m_controlPointPicker.Selection) : null;
                PickResult newSelection = m_controlPointPicker.Pick(camera, point);
                if (newSelection != null && newSelection.Spline != null && newSelection.Spline.GetComponent<Deformer>() == null)
                {
                    newSelection = null;
                }

                GameObject deformerGo = null;
                if(newSelection == null)
                {
                    RaycastHit hit;
                    if (Physics.Raycast(camera.ScreenPointToRay(point), out hit))
                    {
                        Segment segment = hit.collider.GetComponent<Segment>();
                        if (segment != null)
                        {
                            Deformer deformer = segment.GetComponentInParent<Deformer>();
                            if(deformer != null)
                            {
                                deformerGo = deformer.gameObject;
                            }
                        }
                    }
                }

                if(deformerGo != null)
                {
                    m_editor.Selection.Select(deformerGo, new[] { deformerGo });
                }

                m_controlPointPicker.ApplySelection(newSelection, true);
                newSelection = newSelection != null ? new PickResult(newSelection) : null;
                m_editor.Undo.CreateRecord(record =>
                {
                    m_controlPointPicker.Selection = newSelection;
                    if (deformerGo != null)
                    {
                        m_editor.Selection.Select(deformerGo, new[] { deformerGo });
                    }
                    return true;
                },
                record =>
                {
                    m_controlPointPicker.Selection = oldSelection;
                    if (deformerGo != null)
                    {
                        m_editor.Selection.Select(deformerGo, new[] { deformerGo });
                    }
                    return true;
                });
                m_editor.Undo.EndRecord();
            }
        }

        public bool DragControlPoint(bool extend)
        {
            PositionHandle positionHandle = m_editor.Tools.ActiveTool as PositionHandle;
            if (m_controlPointPicker.IsPointSelected && positionHandle != null && positionHandle.IsDragging)
            {
                if (extend)
                {
                    ControlPointPicker picker = m_editor.Selection.activeGameObject.GetComponent<ControlPointPicker>();
                    BaseSpline spline = picker.Selection.GetSpline();
                    if(!(spline is Deformer))
                    {
                        return false;
                    }

                    BaseSplineState oldState = spline.GetState();
                    PickResult oldSelection = picker.Selection != null ? new PickResult(picker.Selection) : null;
                    m_controlPointPicker.Drag(true);
                    spline = picker.Selection.GetSpline();
                    BaseSplineState newState = spline.GetState();
                    PickResult newSelection = picker.Selection != null ? new PickResult(picker.Selection) : null;
                    RecordState(spline.gameObject, oldState, newState, picker, oldSelection, newSelection);
                }
                else
                {
                    m_controlPointPicker.Drag(false);
                }
                return true;
            }
            return false;
        }

        public bool CanDeform()
        {
            return
                m_editor.Selection.activeGameObject != null &&
                m_editor.Selection.activeGameObject.GetComponent<MeshFilter>() &&
                /*m_editor.Selection.activeGameObject.GetComponent<MeshCollider>() &&*/
                m_editor.Selection.activeGameObject.GetComponentInParent<Deformer>() == null;
        }

        public void DeformAxis(Axis axis)
        {
            Deformer meshDeformer = m_editor.Selection.activeGameObject.GetComponent<Deformer>();
            if (meshDeformer == null)
            {
                m_editor.Undo.BeginRecord();
                GameObject go = m_editor.Selection.activeGameObject;
                m_editor.Undo.AddComponent(go.GetComponent<ExposeToEditor>(), typeof(Deformer));
                meshDeformer = go.GetComponent<Deformer>();
                meshDeformer.Axis = axis;
                m_editor.Undo.CreateRecord(redo =>
                {
                    Deformer deformer = go.GetComponent<Deformer>();
                    deformer.Axis = axis;
                    EnableSplineRenderers(Mode == MeshDeformerToolMode.ControlPoint);
                    GetSelectedDeformer();
                    return false;
                }, undo =>
                {
                    return false;
                }
                );
                m_editor.Undo.EndRecord();
                GetSelectedDeformer();
            }

            EnableSplineRenderers(false);
        }

        private void OnDeformerRefreshed(Deformer obj)
        {
            if(obj.gameObject == m_editor.Selection.activeObject)
            {
                UpdateSelection();
            }
        }

        private void UpdateSelection()
        {
            var activeObject = m_editor.Selection.activeObject;
            var objects = m_editor.Selection.objects;

            m_editor.Selection.Select(null, null);
            m_editor.Selection.Select(activeObject, objects);
        }

        public bool CanSubdivide()
        {
            return
                m_editor.Selection.activeGameObject != null &&
                m_editor.Selection.activeGameObject.GetComponentInParent<Deformer>();
        }

        public void Subdivide()
        {
            Deformer meshDeformer = m_editor.Selection.activeGameObject.GetComponentInParent<Deformer>();

            MeshFilter meshFilter = meshDeformer.GetComponent<MeshFilter>();

            MeshCollider meshCollider = meshDeformer.GetComponent<MeshCollider>();

            if(meshFilter.sharedMesh != null)
            {
                Mesh newMesh = MeshSubdivider.Subdivide(meshFilter.sharedMesh, 2);
                Mesh oldMesh = Instantiate(meshFilter.sharedMesh);
                oldMesh.name = meshFilter.sharedMesh.name;
                newMesh.name = meshFilter.sharedMesh.name;
                meshFilter.sharedMesh = null;
                meshFilter.sharedMesh = Instantiate(newMesh);
                if(meshCollider != null)
                {
                    meshCollider.sharedMesh = null;
                    meshCollider.sharedMesh = meshFilter.sharedMesh;
                }
                meshDeformer.Refresh();

                GameObject go = meshDeformer.gameObject;
                m_editor.Undo.CreateRecord(redo =>
                {
                    Mesh newMeshInstance = Instantiate(newMesh);
                    newMeshInstance.name = newMesh.name;

                    MeshFilter filter = go.GetComponent<MeshFilter>();
                    Destroy(filter.sharedMesh);
                    filter.sharedMesh = newMeshInstance;

                    MeshCollider collider = go.GetComponent<MeshCollider>();
                    if (collider != null)
                    {
                        Destroy(collider.sharedMesh);
                        collider.sharedMesh = newMeshInstance;
                    }

                    Deformer deformer = go.GetComponent<Deformer>();
                    deformer.Refresh();

                    return true;
                }, undo =>
                {
                    Mesh oldMeshInstance = Instantiate(oldMesh);
                    oldMeshInstance.name = oldMesh.name;

                    MeshFilter filter = go.GetComponent<MeshFilter>();
                    Destroy(filter.sharedMesh);
                    filter.sharedMesh = oldMeshInstance;
                    
                    MeshCollider collider = go.GetComponent<MeshCollider>();
                    if (collider != null)
                    {
                        Destroy(collider.sharedMesh);
                        collider.sharedMesh = oldMeshInstance;
                    }

                    Deformer deformer = go.GetComponent<Deformer>();
                    deformer.Refresh();

                    return true;
                }, 
                purge =>
                {
                    Destroy(newMesh);
                    Destroy(oldMesh);
                });
            }
        }

        public bool CanAppend()
        {
            ControlPointPicker picker = m_editor.Selection.activeGameObject.GetComponent<ControlPointPicker>();
            if(picker == null)
            {
                return false;
            }
            BaseSpline spline = picker.Selection.GetSpline();
            return picker.Selection.Index == spline.SegmentsCount + 1 ||
                   picker.Selection.Index == spline.SegmentsCount + 2 ||
                   picker.Selection.Index == 0 || picker.Selection.Index == 1;
        }

        public void Append()
        {
            ControlPointPicker picker = m_editor.Selection.activeGameObject.GetComponent<ControlPointPicker>();
            BaseSpline spline = picker.Selection.GetSpline();
            BaseSplineState oldState = spline.GetState();
            PickResult oldSelection = picker.Selection != null ? new PickResult(picker.Selection) : null;
            if (picker.Selection.Index == 0 || picker.Selection.Index == 1)
            {
                picker.Prepend();
            }
            else
            {
                picker.Append();
            }

            spline = picker.Selection.GetSpline();
            BaseSplineState newState = spline.GetState();
            PickResult newSelection = picker.Selection != null ? new PickResult(picker.Selection) : null;
            RecordState(spline.gameObject, oldState, newState, picker, oldSelection, newSelection);
        }

        public bool CanRemove()
        {
            ControlPointPicker picker = m_editor.Selection.activeGameObject.GetComponent<ControlPointPicker>();
            if (picker == null)
            {
                return false;
            }
            return picker != null && picker.Selection != null && picker.Selection.GetSpline().SegmentsCount > 1 && picker.Selection.Index >= 0;
        }

        public void Remove()
        {
            ControlPointPicker picker = m_editor.Selection.activeGameObject.GetComponent<ControlPointPicker>();
            BaseSpline spline = picker.Selection.GetSpline();
            BaseSplineState oldState = spline.GetState();
            PickResult oldSelection = picker.Selection != null ? new PickResult(picker.Selection) : null;
            picker.Remove();
            PickResult newSelection = picker.Selection != null ? new PickResult(picker.Selection) : null;

            spline = picker.Selection.GetSpline();
            BaseSplineState newState = spline.GetState();
            RecordState(spline.gameObject, oldState, newState, picker, oldSelection, newSelection);
        }

        public bool CanDestroy()
        {
            return
                m_editor.Selection.activeGameObject != null &&
                m_editor.Selection.activeGameObject.GetComponent<MeshFilter>() &&
                m_editor.Selection.activeGameObject.GetComponent<MeshCollider>() &&
                m_editor.Selection.activeGameObject.GetComponentInParent<Deformer>() != null;
        }

        public void Destroy()
        {
            GameObject go = m_editor.Selection.activeGameObject;
            
            GameObject copy = Instantiate(go);
            copy.name = go.name;

            Deformer deformer = copy.GetComponent<Deformer>();
            Destroy(deformer);

            m_editor.Undo.BeginRecord();
            m_editor.RegisterCreatedObjects(new[] { copy }, true);
            m_editor.Delete(new[] { go });
            m_editor.Undo.EndRecord();
        }

        private void RecordState(GameObject spline,
            BaseSplineState oldState,
            BaseSplineState newState,
            ControlPointPicker picker,
            PickResult oldSelection,
            PickResult newSelection)
        {
            m_editor.Undo.CreateRecord(record =>
            {
                Deformer deformer = spline.GetComponent<Deformer>();
                deformer.SetState(newState);
                picker.Selection = newSelection;
                return true;
            },
            record =>
            {
                Deformer deformer = spline.GetComponent<Deformer>();
                deformer.SetState(oldState);
                picker.Selection = oldSelection;
                return true;
            });
        }

        private void OnPositionHandleBeforeDrag(BaseHandle handle)
        {
           // handle.EnableUndo = false;
        }

        private void OnPositionHandleDrop(BaseHandle handle)
        {
            //handle.EnableUndo = true;
        }
    }
}
