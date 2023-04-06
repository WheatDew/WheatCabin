using UnityEngine;
using Battlehub.Utils;

namespace Battlehub.RTGizmos
{
    public class AudioSourceGizmo : SphereGizmo
    {
        [SerializeField]
        private AudioSource m_source;

        [SerializeField]
        [HideInInspector]
        private bool m_max = true;

        private AudioSourceGizmo m_gizmo;

        protected override Vector3 Center
        {
            get { return Vector3.zero; }
            set { }
        }

        protected override float Radius
        {
            get
            {
                if (m_source == null)
                {
                    return 0;
                }

                if(m_max)
                {
                    return m_source.maxDistance;
                }
                else
                {
                    return m_source.minDistance;
                }
            }
            set
            {
                if (m_source != null)
                {
                    if (m_max)
                    {
                        m_source.maxDistance = value;
                    }
                    else
                    {
                        m_source.minDistance = value;
                    }
                }
            }
        }

        protected override void Start()
        {
            base.Start();

            if (m_source == null)
            {
                m_source = GetComponent<AudioSource>();
            }

            if (m_source == null)
            {
                Debug.LogError("Set AudioSource");
            }

            if(m_max)
            {
                m_gizmo = gameObject.AddComponent<AudioSourceGizmo>();
                m_gizmo.LineColor = LineColor;
                m_gizmo.HandlesColor = HandlesColor;
                m_gizmo.SelectionColor = SelectionColor;
                m_gizmo.SelectionMargin = SelectionMargin;
                m_gizmo.EnableUndo = EnableUndo;
                m_gizmo.m_max = !m_max;
                m_gizmo.Window = Window;
            }

            if(RTECamera != null)
            {
                RTECamera.RefreshCommandBuffer();
            }
            
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Destroy(m_gizmo);
        }

        protected override void BeginRecord()
        {
            base.BeginRecord();
            Window.Editor.Undo.BeginRecordValue(m_source, Strong.PropertyInfo((AudioSource x) => x.minDistance, "minDistance"));
            Window.Editor.Undo.BeginRecordValue(m_source, Strong.PropertyInfo((AudioSource x) => x.maxDistance, "maxDistance"));
        }

        protected override void EndRecord()
        {
            base.EndRecord();
            Window.Editor.Undo.EndRecordValue(m_source, Strong.PropertyInfo((AudioSource x) => x.minDistance, "minDistance"));
            Window.Editor.Undo.EndRecordValue(m_source, Strong.PropertyInfo((AudioSource x) => x.maxDistance, "maxDistance"));
        }

        public override void Reset()
        {
            base.Reset();
            LineColor = new Color(0.375f, 0.75f, 1, 0.5f);
            HandlesColor = new Color(0.375f, 0.75f, 1, 0.5f);
            SelectionColor = new Color(1.0f, 1.0f, 0, 1.0f);
        }
    }
}

