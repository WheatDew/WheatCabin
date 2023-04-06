using Battlehub.RTCommon;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Battlehub.Spline3
{
    public class PickResult
    {
        public GameObject Spline;
        public BaseSpline GetSpline()
        {
            BaseSpline[] splines = Spline.GetComponents<BaseSpline>();
            return splines.Where(s => s.IsSelectable).FirstOrDefault();
        }
        public float ScreenDistance;
        public Vector3 WorldPosition;
        public int Index;

        public PickResult()
        {

        }

        public PickResult(PickResult other)
        {
            Spline = other.Spline;
            ScreenDistance = other.ScreenDistance;
            WorldPosition = other.WorldPosition;
            Index = other.Index;
        }
    }

    public abstract class PointPicker : MonoBehaviour
    {
        public event Action SelectionChanged;

        protected IRTE m_editor;
        protected bool m_isPointSelected;
        protected PickResult m_pickResult;
        protected Vector3 m_prevPosition;

        public virtual bool IsPointSelected
        {
            get { return m_isPointSelected; }
        }

        public virtual PickResult Selection
        {
            get { return m_pickResult; }
            set
            {
                m_pickResult = value;
                if (m_pickResult != null)
                {
                    GetLocalPoints(m_pickResult.GetSpline()); //instead of refresh
                    transform.position = GetPoint(m_pickResult.GetSpline(), m_pickResult.Index);
                    m_prevPosition = transform.position;
                }

                if (SelectionChanged != null)
                {
                    SelectionChanged();
                }
            }
        }

        protected virtual void Awake()
        {
            m_editor = IOC.Resolve<IRTE>();
            m_editor.Selection.SelectionChanged += OnEditorSelectionChanged;
            m_prevPosition = transform.position;
        }

        protected virtual void OnDestroy()
        {
            if (m_editor != null)
            {
                m_editor.Selection.SelectionChanged -= OnEditorSelectionChanged;
            }
        }

        protected virtual void LateUpdate()
        {
            if (m_pickResult == null)
            {
                return;
            }

            if (m_prevPosition != transform.position)
            {
                m_prevPosition = transform.position;
                BaseSpline spline = m_pickResult.GetSpline();
                SetPoint(spline, m_pickResult.Index, transform.position);
            }
        }

        public virtual void Refresh()
        {
            if (m_pickResult != null)
            {
                transform.position = GetPoint(m_pickResult.GetSpline(), m_pickResult.Index);
                m_prevPosition = transform.position;
            }
        }

        public virtual void Drag(bool extend)
        {
            BaseSpline spline = m_pickResult.GetSpline();
            if(m_prevPosition != transform.position)
            {
                SetPoint(spline, m_pickResult.Index, transform.position);
                m_prevPosition = transform.position;
            }
        }

        public virtual PickResult Pick(Camera camera, Vector2 position)
        {
            return PickControlPoint(camera, position, 20);
        }

        public virtual void ApplySelection(PickResult pickResult, bool canClearSelection)
        {
            PickResult oldPickResult = m_pickResult;
            m_pickResult = pickResult;
            if (m_pickResult != null)
            {
                BaseSpline spline = m_pickResult.GetSpline();
                transform.position = GetPoint(spline, m_pickResult.Index);
                m_prevPosition = transform.position;
                m_editor.Selection.activeGameObject = gameObject;
            }
            else if (oldPickResult != null && canClearSelection)
            {
                m_editor.Selection.activeGameObject = null;
            }

            Selection = m_pickResult;
        }

        protected List<PickResult> m_nearestControlPoints = new List<PickResult>();
        protected virtual PickResult PickControlPoint(Camera camera, Vector3 mousePosition, float maxDistance)
        {
            BaseSpline[] splines = FindObjectsOfType<BaseSpline>();

            m_nearestControlPoints.Clear();
            maxDistance = maxDistance * maxDistance;

            foreach (BaseSpline spline in splines)
            {
                if (!spline.IsSelectable)
                {
                    continue;
                }
                GetNearestVertices(camera, spline, mousePosition, m_nearestControlPoints, maxDistance, 1.0f);
            }

            if (m_nearestControlPoints.Count == 0)
            {
                return null;
            }

            PickResult result = m_nearestControlPoints[0];
            for (int i = 1; i < m_nearestControlPoints.Count; i++)
            {
                if (m_nearestControlPoints[i].ScreenDistance < result.ScreenDistance) result = m_nearestControlPoints[i];
            }

            m_nearestControlPoints.Clear();
            return result;
        }

        protected int GetNearestVertices(Camera camera, BaseSpline spline, Vector3 mousePosition, List<PickResult> list, float maxDistance, float distModifier)
        {
            IEnumerable<Vector3> points = GetLocalPoints(spline);

            int index = 0;
            if (!spline.ShowTerminalPoints && !spline.IsLooping)
            {
                index++;
                points = points.Take(spline.ControlPointCount - 1).Skip(1);
            }
            
            int matches = 0;
            foreach (Vector3 point in points)
            {
                Vector3 v = spline.transform.TransformPoint(point);
                Vector3 p = camera.WorldToScreenPoint(v);
                p.z = mousePosition.z;

                float dist = (p - mousePosition).sqrMagnitude * distModifier;

                if (dist < maxDistance)
                {
                    list.Add(new PickResult
                    {
                        Spline = spline.gameObject,
                        ScreenDistance = dist,
                        WorldPosition = v,
                        Index = index
                    });

                    matches++;
                }

                index++;
            }

            return matches;
        }

        protected virtual void OnEditorSelectionChanged(UnityEngine.Object[] unselectedObjects)
        {
            m_isPointSelected = m_editor.Selection.activeGameObject == gameObject;
            if (!m_isPointSelected)
            {
                m_pickResult = null;
            }
        }

  
        public abstract void SetPoint(BaseSpline spline, int index, Vector3 position);

        public abstract Vector3 GetPoint(BaseSpline spline, int index);

        public abstract IEnumerable<Vector3> GetLocalPoints(BaseSpline spline);
    }

}
