using UnityEngine;
using System.Collections;
using Battlehub.RTCommon;
using Battlehub.Utils;

namespace Battlehub.RTGizmos
{
    public class SphereColliderGizmo : SphereGizmo
    {
        [SerializeField]
        private SphereCollider m_collider;

        protected override Vector3 Center
        {
            get
            {
                if (m_collider == null)
                {
                    return Vector3.zero;
                }
                return m_collider.center;
            }
            set
            {
                if (m_collider != null)
                {
                    m_collider.center = value;
                }
            }
        }

        protected override float Radius
        {
            get
            {
                if (m_collider == null)
                {
                    return 0;
                }

                return m_collider.radius;
            }
            set
            {
                if (m_collider != null)
                {
                    m_collider.radius = value;
                }
            }
        }


        protected override void Awake()
        {
            if (m_collider == null)
            {
                m_collider = GetComponent<SphereCollider>();
            }

            if (m_collider == null)
            {
                Debug.LogError("Set Collider");
            }

            base.Awake();
        }

        protected override void BeginRecord()
        {
            base.BeginRecord();
            Window.Editor.Undo.BeginRecordValue(m_collider, Strong.PropertyInfo((SphereCollider x) => x.center, "center"));
            Window.Editor.Undo.BeginRecordValue(m_collider, Strong.PropertyInfo((SphereCollider x) => x.radius, "radius"));
        }

        protected override void EndRecord()
        {
            base.EndRecord();
            Window.Editor.Undo.EndRecordValue(m_collider, Strong.PropertyInfo((SphereCollider x) => x.center, "center"));
            Window.Editor.Undo.EndRecordValue(m_collider, Strong.PropertyInfo((SphereCollider x) => x.radius, "radius"));
        }
    }
}

