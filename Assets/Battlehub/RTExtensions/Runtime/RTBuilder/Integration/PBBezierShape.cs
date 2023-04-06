using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace Battlehub.ProBuilderIntegration
{
    public class PBBezierShape : PBComplexShape
    {
        [SerializeField]
        private List<BezierPoint> m_bezierPoints = new List<BezierPoint>();
        public List<BezierPoint> BezierPoints
        {
            get { return m_bezierPoints; }
            set { m_bezierPoints = value; }
        }

        [SerializeField]
        private bool m_closeLoop = false;
        public bool CloseLoop
        {
            get { return m_closeLoop; }
            set { m_closeLoop = value; }
        }

        [SerializeField]
        private float m_radius = .5f;
        public float Radius
        {
            get { return m_radius; }
            set { m_radius = value; }
        }

        [SerializeField]
        private int m_rows = 8;
        public int Rows
        {
            get { return m_rows; }
            set { m_rows = value; }
        }

        [SerializeField]
        private int m_columns = 16;
        public int Columns
        {
            get { return m_columns; }
            set { m_columns = value; }
        }

        [SerializeField]
        private bool m_smooth = true;
        public bool Smooth
        {
            get { return m_smooth; }
            set { m_smooth = value; }
        }

        protected override void Awake()
        {
            base.Awake();
            Init();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        /// <summary>
        /// Initialize the points list with a default set.
        /// </summary>
        private void Init()
        {
            Vector3 tan = new Vector3(0f, 0f, 2f);
            Vector3 p1 = new Vector3(3f, 0f, 0f);
            m_bezierPoints.Add(new BezierPoint(Vector3.zero, -tan, tan, Quaternion.identity));
            m_bezierPoints.Add(new BezierPoint(p1, p1 + tan, p1 + -tan, Quaternion.identity));
        }

        protected override void CreateShape()
        {
            ProBuilderMesh m = TargetMesh;
            PBSpline.Extrude(BezierPoints, Radius, Columns, Rows, CloseLoop, Smooth, ref m);
        }

        protected override PBComplexShapeSelection CreateSelectionObject()
        {
            return gameObject.AddComponent<PBBezierShapeSelection>();
        }
    }
}

