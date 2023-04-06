using Battlehub.RTCommon;
using System.Collections.Generic;
using UnityEngine;

namespace Battlehub.RTMeasurement
{
    public class MeasureDistanceTool : MeasureTool
    {
        [SerializeField]
        private bool m_metric = true;
        public bool Metric
        {
            get { return m_metric; }
            set { m_metric = value; }
        }

        protected List<Vector3> m_points = new List<Vector3>();
        protected override void UpdateOverride()
        {
            base.UpdateOverride();

            RaycastHit hit;
            if (Physics.Raycast(Window.Pointer, out hit))
            {
                Vector3 point = SnapToVertex(hit.point);

                if (Editor.Input.GetPointerDown(0))
                {
                    if (m_points.Count == 2)
                    {
                        m_points.Clear();
                    }
                    else
                    {
                        m_points.Add(point);
                        m_points.Add(point);
                    }

                    Renderer.Vertices = m_points.ToArray();
                    Renderer.Refresh();
                }
                else
                {
                    PointerRenderer.transform.position = point;

                    if(m_points.Count == 2)
                    {
                        m_points[1] = point;
                        Renderer.Vertices = m_points.ToArray();
                        Renderer.Refresh(true);
                    }

                    if(Output != null)
                    {
                        Output.transform.position = point;
                        if (m_points.Count == 2)
                        {
                            float mag = (m_points[1] - m_points[0]).magnitude;
                            Output.text = m_metric ? mag.ToString("F2") : UnitsConverter.MetersToFeetInches(mag);
                            Output.text += System.Environment.NewLine + System.Environment.NewLine;
                        }
                        else
                        {
                            Output.text = "";
                        }
                    }
                }
            } 
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            m_points.Clear();
        }

        protected override void OnCommandBufferRefresh(IRTECamera camera)
        {
            base.OnCommandBufferRefresh(camera);
            if (Output != null)
            {
                Renderer renderer = Output.GetComponent<Renderer>();
                renderer.enabled = false;
                camera.CommandBuffer.DrawRenderer(renderer, renderer.sharedMaterial);
            }
        }
    }
}

