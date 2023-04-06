using Battlehub.RTCommon;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTTerrain
{
    public class TerrainHeightEditor : TerrainStampEditor
    {
        [SerializeField]
        private Button m_flattenButton = null;

        private IRTE m_editor;

        public override float Height
        {
            get { return m_height; }
            set
            {
                if (m_height != value)
                {
                    m_height = value;
                    if (TerrainEditor.Projector.TerrainBrush != null)
                    {
                        TerrainEditor.Projector.TerrainBrush.Max = value;
                    }
                }
            }
        }

        protected override void Awake()
        {
            base.Awake();

            m_editor = IOC.Resolve<IRTE>();

            if (m_flattenButton != null)
            {
                m_flattenButton.onClick.AddListener(OnFlatten);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (m_flattenButton != null)
            {
                m_flattenButton.onClick.RemoveListener(OnFlatten);
            }
        }

        protected virtual void Update()
        {
            if(m_editor.ActiveWindow == null || 
              !m_editor.ActiveWindow.IsPointerOver ||
              m_editor.ActiveWindow.WindowType != RuntimeWindowType.Scene)
            {
                return;
            }

            if(m_editor.Input.GetPointerDown(0) && m_editor.Input.GetKey(KeyCode.LeftShift))
            {
                RaycastHit[] hits = Physics.RaycastAll(m_editor.ActiveWindow.Pointer);
                foreach(RaycastHit hit in hits)
                {
                    if(!(hit.collider is TerrainCollider))
                    {
                        continue;
                    }

                    Vector3 hitPoint = hit.collider.gameObject.transform.InverseTransformPoint(hit.point);
                    Height = hitPoint.y;
                }
            }
        }

        protected override Brush CreateBrush()
        {
            TerrainBrush brush = new TerrainBrush();
            brush.Blend = Brush.BlendFunction.Clamp;
            brush.Max = Height;
            brush.AllowNegativeValue = false;
            return brush;
        }

        private void OnFlatten()
        {
            Terrain terrain = TerrainEditor.Terrain;
            if(terrain == null)
            {
                return;
            }
            float[,] oldHeightmap = GetHeightmap();
            float[,] newHeightmap = GetHeightmap();
            int w = newHeightmap.GetLength(0);
            int h = newHeightmap.GetLength(1);
            float heightmapScale = terrain.terrainData.heightmapScale.y;
            for(int i = 0; i < w; ++i)
            {
                for (int j = 0; j < w; ++j)
                {
                    newHeightmap[i, j] = Height / heightmapScale;
                }
            }
            terrain.SetHeights(0, 0, newHeightmap);

            IRTE editor = IOC.Resolve<IRTE>();
            editor.Undo.CreateRecord(record =>
            {
                terrain.SetHeights(0, 0, newHeightmap);
                terrain.TerrainColliderWithoutHoles();
                return true;
            },
            record =>
            {
                terrain.SetHeights(0, 0, oldHeightmap);
                terrain.TerrainColliderWithoutHoles();
                return true;
            });
        }

        private float[,] GetHeightmap()
        {
            int w = TerrainEditor.Terrain.terrainData.heightmapResolution;
            int h = TerrainEditor.Terrain.terrainData.heightmapResolution;
            return TerrainEditor.Terrain.terrainData.GetHeights(0, 0, w, h);
        }

    }
}


