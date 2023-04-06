using Battlehub.RTCommon;
using UnityEngine;
using UnityEngine.Rendering;

namespace Battlehub.RTEditor
{
    public class TimelineGridParameters
    {
        public int VertLines;
        public int VertLinesSecondary;
        
        public int HorLines;
        public int HorLinesSecondary;
        public Color LineColor;

        public float FixedHeight;
    }

    public class TimelineGrid : MonoBehaviour
    {
        //private CommandBuffer m_commandBuffer;
        private RTECamera m_rteCamera;
        private Camera m_camera;

        private Mesh m_vGridMesh0;
        private Mesh m_vGridMesh1;
        private Mesh m_vGridMesh2;

        private Mesh m_hGridMesh0;
        private Mesh m_hGridMesh1;
        private Mesh m_hGridMesh2;

        private Material m_vGridMaterial0;
        private Material m_vGridMaterial1;
        private Material m_vGridMaterial2;
        
        private Material m_hGridMaterial0;
        private Material m_hGridMaterial1;
        private Material m_hGridMaterial2;
        
        public const int k_Lines = 5;
        private const float k_FadeOutPixels = 2;

        private TimelineGridParameters m_parameters = null;

        private void OnDestroy()
        {
            if (m_rteCamera != null)
            {
                m_rteCamera.CommandBufferRefresh -= OnCommandBufferRefresh;
                Destroy(m_rteCamera);
                m_rteCamera = null;
            }
        }

        public void Init(Camera camera)
        {
            if (m_rteCamera != null)
            {
                m_rteCamera.CommandBufferRefresh -= OnCommandBufferRefresh;
                Destroy(m_rteCamera);
                m_rteCamera = null;
            }

            m_camera = camera;
            
            m_rteCamera = m_camera.gameObject.AddComponent<RTECamera>();
            m_rteCamera.Event = CameraEvent.BeforeImageEffects;
            m_rteCamera.CommandBufferRefresh += OnCommandBufferRefresh;

            m_vGridMaterial1 = CreateGridMaterial();
            m_vGridMaterial2 = CreateGridMaterial();
            m_vGridMaterial0 = CreateGridMaterial();
            m_hGridMaterial1 = CreateGridMaterial();
            m_hGridMaterial2 = CreateGridMaterial();
            m_hGridMaterial0 = CreateGridMaterial();
        }

        public void SetGridParameters(TimelineGridParameters parameters)
        {
            Vector2 maxSupportedViewportSize = new Vector2(4096, 4096);
            SetGridParameters(parameters, maxSupportedViewportSize);
        }

        public void SetGridParameters(TimelineGridParameters parameters, Vector2 viewportSize)
        {
            //m_commandBuffer.Clear();

            m_parameters = parameters;

            if (m_vGridMesh0 != null)
            {
                Destroy(m_vGridMesh0);
            }

            if (m_vGridMesh1 != null)
            {
                Destroy(m_vGridMesh1);
            }

            if (m_vGridMesh2 != null)
            {
                Destroy(m_vGridMesh2);
            }

            if (m_hGridMesh0 != null)
            {
                Destroy(m_hGridMesh0);
            }

            if (m_hGridMesh1 != null)
            {
                Destroy(m_hGridMesh1);
            }

            if(m_hGridMesh2 != null)
            {
                Destroy(m_hGridMesh2);
            }

            int vLinesCount = m_parameters.VertLines * m_parameters.VertLinesSecondary;
            int hLinesCount = m_parameters.HorLines * m_parameters.HorLinesSecondary;

            int repeatX = Mathf.Max(1, Mathf.CeilToInt((viewportSize.x / (vLinesCount * m_parameters.VertLinesSecondary)) / k_FadeOutPixels));
            int repeatY = Mathf.Max(1, Mathf.CeilToInt((viewportSize.y / (hLinesCount * m_parameters.HorLinesSecondary)) / k_FadeOutPixels));

            m_vGridMesh0 = CreateGridMesh(vLinesCount, true, repeatX, repeatY);
            if (m_parameters.FixedHeight < 0)
            {
                m_hGridMesh0 = CreateGridMesh(hLinesCount, false, repeatY, repeatX);
            }
            else
            {
                m_hGridMesh0 = CreateGridMesh(m_parameters.HorLines, false, 1, repeatX, int.MaxValue, false);
            }

            m_vGridMesh1 = CreateGridMesh(vLinesCount, true, repeatX, repeatY);
            if(m_parameters.FixedHeight < 0)
            {
                m_hGridMesh1 = CreateGridMesh(hLinesCount, false, repeatY, repeatX);
            }

            m_vGridMesh2 = CreateGridMesh(vLinesCount, true, repeatX, repeatY);
            if(m_parameters.FixedHeight < 0)
            {
                m_hGridMesh2 = CreateGridMesh(hLinesCount, false, repeatY, repeatX);
            }
        }

        public static float EaseOutQuad(float start, float end, float value)
        {
            end -= start;
            return -end * value * (value - 2) + start;
        }

        private void SetAlpha(Material material, float fadeOutOffset, float p)
        {
            Color color = m_parameters.LineColor;

            p = p % 3.0f / 3.0f;
            
            p = (p > 1 - fadeOutOffset) ? 1 : p / (1 - fadeOutOffset);
            p = (p <= 0.5) ? 0 : (p - 0.5f) * 2.0f;

            color.a *= EaseOutQuad(1, 0, p);
            material.color = color;
        }

        private class UpdateGraphicsArgs
        {
            public Vector2 ViewportSize;
            public Vector2 ContentSize;
            public Vector2 NormalizedOffset;
            public Vector2 NormalizedSize;
            public Vector2 Interval;
        }
        private UpdateGraphicsArgs m_updateGraphicsArgs = new UpdateGraphicsArgs();
        public void UpdateGraphics(Vector2 viewportSize, Vector2 contentSize, Vector2 normalizedOffset, Vector2 normalizedSize, Vector2 interval)
        {
            m_updateGraphicsArgs.ViewportSize = viewportSize;
            m_updateGraphicsArgs.ContentSize = contentSize;
            m_updateGraphicsArgs.NormalizedOffset = normalizedOffset;
            m_updateGraphicsArgs.NormalizedSize = normalizedSize;
            m_updateGraphicsArgs.Interval = interval;

            m_rteCamera.RefreshCommandBuffer();
        }

        private Material CreateGridMaterial()
        {
            Shader shader = Shader.Find("Battlehub/RTEditor/TimelineGrid");
            Material material = new Material(shader);
            return material;
        }

        private Mesh CreateGridMesh(float count, bool isVertical, int repeat = 2, int lineLength = 2, int skipLine = int.MaxValue, bool drawListLine = true)
        {
            if(lineLength < 1000)
            {
                lineLength *= 100;
            }

            Mesh mesh = new Mesh();
            mesh.name = "TimelineGrid";

            float space = 1.0f / count;
            int totalCount = Mathf.CeilToInt(count * repeat);
            int index = 0;
            int[] indices = new int[(totalCount - totalCount / skipLine) * 2];
            Vector3[] vertices = new Vector3[indices.Length];
            Color[] colors = new Color[indices.Length];

            if(!drawListLine)
            {
                totalCount--;
            }

            if(isVertical)
            {
                for (int i = 0; i < totalCount; ++i)
                {
                    if (i % skipLine == 0)
                    {
                        continue;
                    }

                    vertices[index] = new Vector3(i * space, 0, 0);
                    vertices[index + 1] = new Vector3(i * space, -lineLength, 0);

                    indices[index] = index;
                    indices[index + 1] = index + 1;

                    Color color = Color.white;
                    colors[index] = colors[index + 1] = color;

                    index += 2;
                }
            }
            else
            {
                for (int i = 0; i < totalCount; ++i)
                {
                    if (i % skipLine == 0)
                    {
                        continue;
                    }

                    vertices[index] = new Vector3(0, -i * space, 0);
                    vertices[index + 1] = new Vector3(lineLength, -i * space, 0);

                    indices[index] = index;
                    indices[index + 1] = index + 1;

                    Color color = Color.white;
                    colors[index] = colors[index + 1] = color;

                    index += 2;
                }
            }


            mesh.vertices = vertices;
            mesh.SetIndices(indices, MeshTopology.Lines, 0);
            mesh.colors = colors;

            return mesh;
        }

        private void OnCommandBufferRefresh(IRTECamera camera)
        {
            Vector2 viewportSize = m_updateGraphicsArgs.ViewportSize;
            //Vector2 contentSize = m_updateGraphicsArgs.ContentSize;
            Vector2 normalizedOffset = m_updateGraphicsArgs.NormalizedOffset;
            Vector2 normalizedSize = m_updateGraphicsArgs.NormalizedSize;
            Vector2 interval = m_updateGraphicsArgs.Interval;

            if (m_parameters == null)
            {
                throw new System.InvalidOperationException("Call SetGridParameters method first");
            }

            bool fixedHeight = m_parameters.FixedHeight > 0;
            if (fixedHeight)
            {
                m_parameters.HorLinesSecondary = 2;
            }

            
            int vLinesCount = m_parameters.VertLines;
            int hLinesCount = m_parameters.HorLines;
            //Color lineColor = m_parameters.LineColor;

            int vLinesSq = m_parameters.VertLinesSecondary * m_parameters.VertLinesSecondary;
            int hLinesSq = m_parameters.HorLinesSecondary * m_parameters.HorLinesSecondary;

            Vector2 contentScale = new Vector2(
                1.0f / normalizedSize.x,
                1.0f / normalizedSize.y);

            Vector3 offset = new Vector3(-0.5f, 0.5f, 2.0f);
            offset.x -= ((1 - normalizedSize.x) * normalizedOffset.x / normalizedSize.x) % (contentScale.x * vLinesSq / Mathf.Max(1, vLinesCount));
            if (!fixedHeight)
            {
                offset.y += ((1 - normalizedSize.y) * (1 - normalizedOffset.y) / normalizedSize.y) % (contentScale.y * hLinesSq / Mathf.Max(1, hLinesCount));
            }

            Vector3 scale = Vector3.one;

            float aspect = viewportSize.x / viewportSize.y;
            offset.x *= aspect;
            // scale.x = aspect;

            float px = interval.x * normalizedSize.x;
            float py = interval.y * normalizedSize.y;

            //required to match grid scale to scroll viewer viewport size & offset
            px = Mathf.Log(px * m_parameters.VertLinesSecondary, m_parameters.VertLinesSecondary);
            py = Mathf.Log(py * m_parameters.HorLinesSecondary, m_parameters.HorLinesSecondary);

            float fadeOutOffset = Mathf.Min(0.4f, 1 - Mathf.Clamp01(viewportSize.x / 600.0f));

            SetAlpha(m_vGridMaterial0, fadeOutOffset, px - 1);
            SetAlpha(m_vGridMaterial1, fadeOutOffset, px);
            SetAlpha(m_vGridMaterial2, fadeOutOffset, px + 1);

            SetAlpha(m_hGridMaterial0, fadeOutOffset, py - 1);
            SetAlpha(m_hGridMaterial1, fadeOutOffset, py);
            SetAlpha(m_hGridMaterial2, fadeOutOffset, py + 1);

            scale.x = aspect * vLinesSq / Mathf.Pow(m_parameters.VertLinesSecondary, (px - 1) % 3.0f);
            scale.y = hLinesSq / Mathf.Pow(m_parameters.HorLinesSecondary, (py - 1) % 3.0f);
            if (fixedHeight)
            {
                offset.y += ((1 - normalizedSize.y) * (1 - normalizedOffset.y) / normalizedSize.y);
                scale.y = hLinesCount * m_parameters.FixedHeight / viewportSize.y;
                m_hGridMaterial0.color = m_parameters.LineColor;
            }

            Matrix4x4 vMatrix = Matrix4x4.TRS(offset, Quaternion.identity, scale);
            if (m_vGridMesh0 != null)
            {
                m_rteCamera.CommandBuffer.DrawMesh(m_vGridMesh0, vMatrix, m_vGridMaterial0);
            }

            Matrix4x4 hMatrix = Matrix4x4.TRS(offset, Quaternion.identity, scale);
            if (m_hGridMesh0 != null)
            {
                m_rteCamera.CommandBuffer.DrawMesh(m_hGridMesh0, hMatrix, m_hGridMaterial0);
            }

            scale.x = aspect * vLinesSq / Mathf.Pow(m_parameters.VertLinesSecondary, px % 3.0f);
            scale.y = hLinesSq / Mathf.Pow(m_parameters.HorLinesSecondary, py % 3.0f);

            vMatrix = Matrix4x4.TRS(offset, Quaternion.identity, scale);
            if (m_vGridMesh1 != null)
            {
                m_rteCamera.CommandBuffer.DrawMesh(m_vGridMesh1, vMatrix, m_vGridMaterial1);
            }

            if (!fixedHeight)
            {
                hMatrix = Matrix4x4.TRS(offset, Quaternion.identity, scale);
                if (m_hGridMesh1 != null)
                {
                    m_rteCamera.CommandBuffer.DrawMesh(m_hGridMesh1, hMatrix, m_hGridMaterial1);
                }
            }

            scale.x = aspect * vLinesSq / Mathf.Pow(m_parameters.VertLinesSecondary, (px + 1) % 3.0f);
            scale.y = hLinesSq / Mathf.Pow(m_parameters.HorLinesSecondary, (py + 1) % 3.0f);

            vMatrix = Matrix4x4.TRS(offset, Quaternion.identity, scale);
            if (m_vGridMesh2 != null)
            {
                if (interval.x > m_parameters.VertLinesSecondary)
                {
                    m_rteCamera.CommandBuffer.DrawMesh(m_vGridMesh2, vMatrix, m_vGridMaterial2);
                }
            }

            if (!fixedHeight)
            {
                hMatrix = Matrix4x4.TRS(offset, Quaternion.identity, scale);
                if (m_hGridMesh2 != null)
                {
                    if (interval.y > m_parameters.HorLinesSecondary)
                    {
                        m_rteCamera.CommandBuffer.DrawMesh(m_hGridMesh2, hMatrix, m_hGridMaterial2);
                    }
                }
            }
        }

    }
}
