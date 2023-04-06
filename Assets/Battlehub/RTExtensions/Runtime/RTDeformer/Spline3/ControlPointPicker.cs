using Battlehub.RTCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Battlehub.Spline3
{
    [DefaultExecutionOrder(-1)]
    public class ControlPointPicker : PointPicker
    {
        public override void Drag(bool extend)
        {
            BaseSpline spline = m_pickResult.GetSpline();
            if (extend)
            {
                if (m_pickResult.Index == 1 || m_pickResult.Index == 0)
                {
                    spline.Prepend();
                    SetPoint(spline, m_pickResult.Index, transform.position);
                }
                else if (m_pickResult.Index == spline.SegmentsCount + 1 ||
                        m_pickResult.Index == spline.SegmentsCount + 2)
                {
                    spline.Append();
                    m_pickResult.Index++;
                    SetPoint(spline, m_pickResult.Index, transform.position);
                }
                else
                {
                    spline.Insert(m_pickResult.Index, 0);
                    m_pickResult.Index++;
                     SetPoint(spline, m_pickResult.Index, transform.position);
                }
            }
            else
            {
                base.Drag(false);
            }
        }

        public void Append()
        {
            BaseSpline spline = m_pickResult.GetSpline();
            spline.Append(2.0f);
            if(spline.ShowTerminalPoints)
            {
                m_pickResult.Index = spline.LocalControlPoints.Count() - 1;
            }
            else
            {
                m_pickResult.Index = spline.LocalControlPoints.Count() - 2;
            }
            
            transform.position = spline.GetControlPoint(m_pickResult.Index);
        }

        public void Prepend()
        {
            BaseSpline spline = m_pickResult.GetSpline();
            spline.Prepend(2.0f);
            if (spline.ShowTerminalPoints)
            {
                m_pickResult.Index = 0;
            }
            else
            {
                m_pickResult.Index = 1;
            }
            transform.position = spline.GetControlPoint(m_pickResult.Index);
        }

        public void Insert()
        {
            BaseSpline spline = m_pickResult.GetSpline();

            int segmentIndex = m_pickResult.Index;
            m_pickResult.Index = spline.Insert(segmentIndex);

            transform.position = spline.GetControlPoint(m_pickResult.Index);
        }

        public void Remove()
        {
            BaseSpline spline = m_pickResult.GetSpline();
            spline.Remove(m_pickResult.Index);
            if (spline.SegmentsCount <= m_pickResult.Index - 1)
            {
                m_pickResult.Index--;
            }

            if(0 <= m_pickResult.Index && m_pickResult.Index < spline.ControlPointCount)
            {
                transform.position = spline.GetControlPoint(m_pickResult.Index);
            }
        }

        public override Vector3 GetPoint(BaseSpline spline, int index)
        {
            return spline.GetControlPoint(index);
        }

        public override void SetPoint(BaseSpline spline, int index, Vector3 position)
        {
            spline.SetControlPoint(index, position);
        }

        public override IEnumerable<Vector3> GetLocalPoints(BaseSpline spline)
        {
            return spline.LocalControlPoints;
        }

        


    }
}


