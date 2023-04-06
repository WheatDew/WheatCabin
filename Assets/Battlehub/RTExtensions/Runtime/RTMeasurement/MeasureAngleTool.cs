using Battlehub.RTCommon;
using System.Collections.Generic;
using UnityEngine;

namespace Battlehub.RTMeasurement
{
    public class MeasureAngleTool : MeasureTool
    {
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
                    if (m_points.Count == 3)
                    {
                        m_points.Clear();
                    }
                    else if(m_points.Count == 2)
                    {
                        m_points.Insert(0, point);
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

                    if (m_points.Count >= 2)
                    {
                        m_points[3 - m_points.Count] = point;
                        Renderer.Vertices = m_points.ToArray();
                        Renderer.Refresh(true);
                    }

                    if(Output != null)
                    {
                        Output.transform.position = point;
                        if (m_points.Count == 3)
                        {
                            Vector3 v1 = (m_points[0] - m_points[1]).normalized;
                            Vector3 v2 = (m_points[2] - m_points[1]).normalized;

                            float angle = Mathf.Acos(Vector3.Dot(v1, v2)) * Mathf.Rad2Deg;
                            if(float.IsNaN(angle))
                            {
                                Output.text = "";
                            }
                            else
                            {
                                Output.text = angle.ToString("F2");
                                Output.text += System.Environment.NewLine + System.Environment.NewLine;
                            }
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