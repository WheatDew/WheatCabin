using Battlehub.RTCommon;
using UnityEngine;

namespace Battlehub.RTTerrain
{
    [DefaultExecutionOrder(1)]
    public abstract class  TerrainProjectorBase : MonoBehaviour
    {
        private IRTE m_editor;
        private Brush m_terrainBrush;
        private Vector2 m_prevPos;
        private Vector3 m_prevCamPos;
        private Quaternion m_prevCamRot;

        private Vector3 m_position;
        private Terrain m_terrain;

        public virtual Brush TerrainBrush
        {
            get { return m_terrainBrush; }
            set { m_terrainBrush = value; }
        }

        public abstract Texture2D Brush
        {
            get;
            set;
        }

        public virtual float Size
        {
            get { return transform.localScale.x; }
            set
            {
                Vector3 localScale = transform.localScale;
                localScale.x = value;
                localScale.y = value;
                transform.localScale = localScale;

                if(m_terrainBrush != null)
                {
                    m_terrainBrush.Radius = value * 0.5f;
                }
            }
        }

        public virtual float Opacity
        {
            get;
            set;
        }

        private bool m_handleInput = true;
        public bool HandleInput
        {
            get { return m_handleInput; }
            set 
            {
                m_handleInput = value; 
            }
        }

        protected virtual void Awake()
        {
            m_editor = IOC.Resolve<IRTE>();
            Enable(false);
        }

        public abstract void Enable(bool value);

        protected virtual void Update()
        {
            if (!m_handleInput)
            {
                return;
            }

            if (m_terrainBrush != null && m_terrainBrush.IsPainting)
            {
                if (m_editor.Input.GetPointerUp(0))
                {
                    m_terrainBrush.EndPaint();
                }
            }
       
            if (m_editor.ActiveWindow == null || m_editor.ActiveWindow.WindowType != RuntimeWindowType.Scene || !m_editor.ActiveWindow.IsPointerOver)
            {
                Enable(false);
                return;
            }

            Camera cam = m_editor.ActiveWindow.Camera;
            if (cam == null)
            {
                return;
            }

            Enable(true);

            if (m_prevPos != m_editor.ActiveWindow.Pointer.ScreenPoint || m_prevCamPos != cam.transform.position || m_prevCamRot != cam.transform.rotation)
            {
                m_prevPos = m_editor.ActiveWindow.Pointer.ScreenPoint;
                m_prevCamPos = cam.transform.position;
                m_prevCamRot = cam.transform.rotation;

                m_terrain = null;

                Ray ray = m_editor.ActiveWindow.Pointer;
                RaycastHit[] hits = Physics.RaycastAll(ray);
                foreach (RaycastHit hit in hits)
                {
                    if (!(hit.collider is TerrainCollider))
                    {
                        continue;
                    }

                    m_terrain = hit.collider.GetComponent<Terrain>();
                    m_position = hit.point;
                    if (m_terrain != null)
                    {
                        break;
                    }
                }

                if (m_terrain == null)
                {
                    Vector3 position = m_editor.ActiveWindow.Camera.transform.position;
                    transform.position = new Vector3(position.x - 10000, position.y - 10000, position.z - 10000);
                    return;
                }

                m_position.y += 200;
                transform.position = m_position;
            }

            if (m_terrain != null && !m_editor.Tools.IsViewing && m_terrainBrush != null)
            {
                if (m_editor.Input.GetPointerDown(0))
                {
                    IRenderPipelineCameraUtility cameraUtility = IOC.Resolve<IRenderPipelineCameraUtility>();
                    if (cameraUtility != null)
                    {
                        cameraUtility.RequiresDepthTexture(m_editor.ActiveWindow.Camera, true);
                    }

                    m_terrainBrush.Terrain = m_terrain;
                    m_terrainBrush.BeginPaint();
                }
                else if (m_editor.Input.GetPointer(0))
                {
                    m_terrainBrush.Terrain = m_terrain;
                    m_terrainBrush.Paint(m_terrain.transform.InverseTransformPoint(m_position), (m_editor.Input.GetKey(KeyCode.LeftShift) ? -Time.deltaTime : Time.deltaTime) * 100.0f, Opacity / 100.0f);
                }
            }
        }
    }


    [DefaultExecutionOrder(1)]
    public class TerrainProjector : TerrainProjectorBase
    {
        private Renderer m_decal;
       
        public override Texture2D Brush
        {
            get { return (Texture2D)m_decal.material.GetTexture("_MainTex"); }
            set
            {
                m_decal.material.SetTexture("_MainTex", value);
                TerrainBrush.Texture = value;
            }
        }

        protected override void Awake()
        {
            m_decal = GetComponent<Renderer>();
            base.Awake();
        }

        public override void Enable(bool value)
        {
            if(m_decal.enabled != value)
            {
                m_decal.enabled = value;
            }
        }
    }

}
