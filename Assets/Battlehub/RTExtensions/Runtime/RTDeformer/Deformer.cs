using UnityEngine;
using Battlehub.Spline3;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Battlehub.MeshDeformer3
{
    public class DeformerState : BaseSplineState
    {
        public Axis Axis;

        public DeformerState(Vector3[] controlPoints, ControlPointSettings[] settings, bool isSelectable, bool isLooping, Axis axis)
            : base(controlPoints, settings, isSelectable, isLooping)
        {
            Axis = axis;
        }
    }

    public class Deformer : BaseSpline
    {
        public static event Action<Deformer> Created;
        public static event Action<Deformer> Destroyed;
        public static event Action<Deformer> Refreshed;
        public static event Action<Deformer> OriginalMeshVisibilityChanged;

        [SerializeField, HideInInspector]
        private BaseSpline m_spline;

        [SerializeField, HideInInspector]
        private MeshFilter m_filter;

        [SerializeField, HideInInspector]
        private MeshCollider m_collider;

        [SerializeField, HideInInspector]
        private MeshRenderer m_meshRenderer;

        [SerializeField, HideInInspector]
        private Contact[] m_contacts;

        [SerializeField, HideInInspector]
        private Contact[] m_colliderContacts;

        [SerializeField, HideInInspector]
        private List<Segment> m_segments = new List<Segment>();
           
        private Mesh Mesh
        {
            get { return m_filter != null ? m_filter.sharedMesh : null; }
        }

        private Mesh ColliderMesh
        {
            get { return m_collider != null ? m_collider.sharedMesh : null; }
            set
            {
                if (m_collider != null)
                {
                    m_collider.sharedMesh = value;
                }
            }
        }

        public Contact[] Contacts
        {
            get { return m_contacts; }
        }

        public Contact[] ColliderContacts
        {
            get { return m_colliderContacts; }
        }

        private Axis m_axis = Axis.Z;
        public Axis Axis
        {
            get { return m_axis; }
            set
            {
                if(m_axis != value)
                {
                    m_axis = value;
                }
            }
        }

        private int m_approximation = 50;
        public int Approximation
        {
            get { return m_approximation; }
            set { m_approximation = value; }
        }

        public override Vector3[] LocalControlPoints
        {
            get { return m_spline.LocalControlPoints; }
            set { m_spline.LocalControlPoints = value; }
        }

        public override int ControlPointCount
        {
            get { return m_spline.ControlPointCount; }
        }

        public override int SegmentsCount
        {
            get { return m_spline.SegmentsCount; }
        }

        public override bool IsLooping
        {
            get { return m_spline.IsLooping; }
            set { m_spline.IsLooping = value; }
        }

        [SerializeField]
        private bool m_isSelectable = true;
        public override bool IsSelectable
        {
            get { return m_isSelectable; }
            set { m_isSelectable = value; }
        }

        public override bool ShowTerminalPoints
        {
            get { return m_spline.ShowTerminalPoints; }
            set { m_spline.ShowTerminalPoints = value; }
        }

        public override ControlPointSettings[] Settings
        {
            get { return m_spline.Settings; }
            set { m_spline.Settings = value; }
        }

        [SerializeField]
        private int m_pointsPerSegment = 0;
        public int PointsPerSegment
        {
            get { return m_pointsPerSegment; }
            set { m_pointsPerSegment = Mathf.Max(0, value); }
        }

        private bool m_isOriginalVisible;
        public bool IsOriginalMeshVisible
        {
            get { return m_isOriginalVisible; }
            set
            {
                if(m_isOriginalVisible != value)
                {
                    m_isOriginalVisible = value;
                    OnOriginalVisibilityChanged(m_isOriginalVisible);

                    if(OriginalMeshVisibilityChanged != null)
                    {
                        OriginalMeshVisibilityChanged(this);
                    }
                }
            }
        }

        private void OnOriginalVisibilityChanged(bool visible)
        {
            if (m_meshRenderer != null)
            {
                m_meshRenderer.enabled = visible;
            }

            if (m_collider != null)
            {
                m_collider.enabled = visible;
            }

            if (visible)
            {
                for (int i = 0; i < m_segments.Count; ++i)
                {
                    if (m_segments[i] != null)
                    {
                        Destroy(m_segments[i].gameObject);
                    }
                }
                m_segments.Clear();
            }
            else
            {
                Refresh();
            }
        }

        private bool m_initialize;
        protected override void Awake()
        {
            m_spline = GetComponents<BaseSpline>().Where(c => c != this).FirstOrDefault();
            if (m_spline == null)
            {
                m_spline = gameObject.AddComponent<CatmullRomSpline>();
                m_spline.IsLooping = false;
                m_initialize = true;
            }
            else
            {
                m_initialize = m_spline.SegmentsCount == 0;
            }
            m_spline.IsSelectable = false;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Destroy(m_spline);

            OnOriginalVisibilityChanged(true);
            
            if (Destroyed != null)
            {
                Destroyed(this);
            }
        }

        private void Start()
        {
            m_filter = GetComponent<MeshFilter>();
            m_collider = GetComponent<MeshCollider>();
            m_meshRenderer = GetComponent<MeshRenderer>();

            OnOriginalVisibilityChanged(IsOriginalMeshVisible);

            if (m_initialize)
            {
                Initialize();
            }

            if (Created != null)
            {
                Created(this);
            }

            Refresh();
        }


        private void Initialize()
        {
            Vector3 from;
            Vector3 to;
            Mesh.GetBounds(m_axis, out from, out to);

            m_spline.SetLocalControlPoint(0, from + (from - to));
            m_spline.SetLocalControlPoint(1, from);
            m_spline.SetLocalControlPoint(2, to);
            m_spline.SetLocalControlPoint(3, to + (to - from));
        }

        public void Refresh(bool wrapAndDeform = true, int controlPointIndex = -1)
        {
            if(IsOriginalMeshVisible)
            {
                return;
            }

            if(wrapAndDeform)
            {
                m_spline.Refresh(false);

                for (int i = 0; i < m_segments.Count; ++i)
                {
                    DestroyImmediate(m_segments[i].gameObject);
                }
                m_segments.Clear();

                Vector3 from;
                Vector3 to;
                Mesh.GetBounds(m_axis, out from, out to);

                m_contacts = Mesh.FindContacts(m_axis);
                if(ColliderMesh != null)
                {
                    m_colliderContacts = ColliderMesh.FindContacts(from, to, m_axis);
                }

                int segmentsCount = m_spline.SegmentsCount / (m_pointsPerSegment + 1);
                int[] indices = new int[m_pointsPerSegment + 1];
                for (int i = 0; i < segmentsCount; ++i)
                {
                    GameObject segmentGO = new GameObject();
                    segmentGO.hideFlags = HideFlags.DontSave;
                    segmentGO.SetActive(false);
                    segmentGO.name = "Segment";
                    segmentGO.transform.SetParent(transform, false);

                    Segment segment = segmentGO.AddComponent<Segment>();
                    MeshRenderer segmentRenderer = segmentGO.AddComponent<MeshRenderer>();
                    MeshFilter segmentFilter = segmentGO.AddComponent<MeshFilter>();
                    MeshCollider segmentCollider = segmentGO.AddComponent<MeshCollider>();

                    segmentRenderer.sharedMaterials = m_meshRenderer.sharedMaterials;
                    if(Mesh != null)
                    {
                        segmentFilter.sharedMesh = Instantiate(Mesh);
                    }
                    
                    if(ColliderMesh != null)
                    {
                        segmentCollider.sharedMesh = Instantiate(ColliderMesh);
                    }
                    
                    segmentGO.SetActive(true);

                    for(int j = 0; j < indices.Length; ++j)
                    {
                        indices[j] = i * indices.Length + j;
                    }

                    segment.Wrap(segmentFilter, segmentCollider, Axis, indices, Approximation);
                    segment.Deform(this, Mesh, ColliderMesh, false);
                    m_segments.Add(segment);
                }
            }
            else
            {
                if (controlPointIndex == -1)
                {
                    for (int i = 0; i < m_segments.Count; ++i)
                    {
                        m_segments[i].Deform(this, Mesh, ColliderMesh, false);
                    }
                }
                else
                {
                    int s0 = (controlPointIndex - 2) / (m_pointsPerSegment + 1);
                    int s1 = (controlPointIndex - 1) / (m_pointsPerSegment + 1);
                    for(int i = 0; i < 3; ++i)
                    {
                        if(0 <= s0 && s0 < m_segments.Count)
                        {
                            m_segments[s0].Deform(this, Mesh, ColliderMesh, false);
                        }
                        s0--;
                    }

                    for (int i = 0; i < 3; ++i)
                    {
                        if (0 <= s1 && s1 < m_segments.Count)
                        {
                            m_segments[s1].Deform(this, Mesh, ColliderMesh, false);
                        }
                        s1++;
                    }
                }   
            }

            Segment prev = null;
            for (int i = 0; i < m_segments.Count;  ++i)
            {
                Segment segment = m_segments[i];
                segment.SlerpContacts(this, Mesh, ColliderMesh, prev, null, false);
                prev = segment;
            }

            if(Refreshed != null)
            {
                Refreshed(this);
            }
        }

        public override void Append(float distance = 0)
        {
            m_spline.Append(distance);
            Refresh();
        }

        public override int Insert(int segmentIndex, float offset = 0.5F)
        {
            int ctrlPointIndex = m_spline.Insert(segmentIndex, offset);
            Refresh();
            return ctrlPointIndex;
        }

        public override void Prepend(float distance = 0)
        {
            m_spline.Prepend(distance);
            Refresh();
        }

        public override void Remove(int segmentIndex)
        {
            m_spline.Remove(segmentIndex);
            Refresh();
        }

        public override Vector3 GetPosition(float t)
        {
            return m_spline.GetPosition(t);
        }

        public override Vector3 GetPosition(int segmentIndex, float t)
        {
            return m_spline.GetPosition(segmentIndex, t);
        }

        public override Vector3 GetLocalPosition(float t)
        {
            return m_spline.GetLocalPosition(t);
        }

        public override Vector3 GetLocalPosition(int segmentIndex, float t)
        {
            return m_spline.GetLocalPosition(segmentIndex, t);
        }

        public override Vector3 GetTangent(float t)
        {
            return m_spline.GetTangent(t);
        }

        public override Vector3 GetTangent(int segmentIndex, float t)
        {
            return m_spline.GetTangent(segmentIndex, t);
        }

        public override Vector3 GetLocalTangent(float t)
        {
            return m_spline.GetLocalTangent(t);
        }

        public override Vector3 GetLocalTangent(int segmentIndex, float t)
        {
            return m_spline.GetLocalTangent(segmentIndex, t);
        }

        public override void SetControlPoint(int index, Vector3 position)
        {
            m_spline.SetControlPoint(index, position);
            Refresh(false, index);
        }

        public override void SetLocalControlPoint(int index, Vector3 position)
        {
            m_spline.SetLocalControlPoint(index, position);
            Refresh(false, index);
        }

        public override Vector3 GetControlPoint(int index)
        {
            return m_spline.GetControlPoint(index);
        }

        public override Vector3 GetLocalControlPoint(int index)
        {
            return m_spline.GetLocalControlPoint(index);
        }

        public override Vector3 GetDirection(float t)
        {
            return m_spline.GetDirection(t);
        }

        public override Vector3 GetDirection(int segmentIndex, float t)
        {
            return m_spline.GetDirection(segmentIndex, t);
        }

        public override Vector3 GetLocalDirection(float t)
        {
            return m_spline.GetLocalDirection(t);
        }

        public override Vector3 GetLocalDirection(int segmentIndex, float t)
        {
            return m_spline.GetLocalDirection(segmentIndex, t);
        }

        public override float GetT(int controlPointIndex)
        {
            return m_spline.GetT(controlPointIndex);
        }

        public override int GetSegmentIndex(ref float t)
        {
            return m_spline.GetSegmentIndex(ref t);
        }

        public float GetTwist(int index, float t)
        {
            return 0.0f;
        }

        public Vector3 GetThickness(int index, float t)
        {
            return Vector3.one;
        }

        public float GetWrap(int index, float t)
        {
            return 0.0f;
        }

        public override BaseSplineState GetState()
        {
            Vector3[] controlPoints = LocalControlPoints != null ? LocalControlPoints.ToArray() : new Vector3[0];
            return new DeformerState(controlPoints, Settings, IsSelectable, IsLooping, Axis);
        }


        public override void SetState(BaseSplineState state)
        {
            DeformerState deformerState = (DeformerState)state;
            m_spline.SetState(state);
            m_axis = deformerState.Axis;
            Refresh();
        }

        public override void SetSettings(int index, ControlPointSettings settings)
        {
            m_spline.SetSettings(index, settings);
        }

        public override ControlPointSettings GetSettings(int index, bool useSegmentIndex = false)
        {
            return m_spline.GetSettings(index, useSegmentIndex);
        }
    }
}


