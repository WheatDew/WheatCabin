using Battlehub.RTCommon;
using Battlehub.RTHandles;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace Battlehub.RTTerrain
{
    public class TerrainAreaHandle : BaseHandle
    {
        private Terrain m_terrain;
        private float[,] m_oldHeights;
        private Vector3[] m_oldResizerPositions;
        private float[,] m_beginDragHeights;
        private float[,] m_heights;
        private int[] m_paddings;
        private Vector3 m_beginDragPoint;
        private Vector3 m_prevPoint;
        private ITerrainAreaProjector m_projector;
        private Mesh m_areaResizerMesh;
        private Material m_areaResizerMaterial;
        private int m_areaResizerIndex;        
        private bool m_initialized = false;

        public Vector3[] AreaResizerPositions
        {
            get { return m_areaResizerMesh.vertices; }
            set 
            {
                m_areaResizerMesh.vertices = value;
                m_areaResizerMesh.RecalculateBounds();
            }
        }

        public override Transform[] Targets 
        {
            get { return base.Targets; }
            set
            {
                if(value != null)
                {
                    foreach(Transform transform in value)
                    {
                        m_terrain = transform.GetComponent<Terrain>();
                        if(m_terrain != null)
                        {
                            base.Targets = new[] { m_terrain.transform };
                            break;
                        }
                    }
                }
                else
                {
                    base.Targets = value;
                }

                if(m_areaResizerMesh != null)
                {
                    Destroy(m_areaResizerMesh);
                }

                m_areaResizerMesh = new Mesh();
                BuildPointsMesh(m_areaResizerMesh, 5);
            }
        }

        private float m_handleScale = 4.5f;
        public float HandleScale
        {
            get { return m_handleScale; }
            set
            {
                if(m_handleScale != value)
                {
                    SetHandleScale(value);
                }
            }
        }

        private void SetHandleScale(float value)
        {
            m_handleScale = value;
            if(m_areaResizerMaterial != null)
            {
                m_areaResizerMaterial.SetFloat("_Scale", value);
            }
        }

        protected override void Awake()
        {
            base.Awake();
            
            m_areaResizerMaterial = new Material(Shader.Find("Hidden/RTHandles/PointBillboard"));
            SetHandleScale(m_handleScale);
            m_areaResizerMaterial.SetInt("_HandleZTest", (int)CompareFunction.Always);

            if (m_areaResizerMesh != null)
            {
                Destroy(m_areaResizerMesh);
            }
            m_areaResizerMesh = new Mesh();
            BuildPointsMesh(m_areaResizerMesh, 5);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Destroy(m_areaResizerMaterial);
            Destroy(m_areaResizerMesh);
        }

        protected override void OnEnable()
        {
            BaseHandleInput input = GetComponent<BaseHandleInput>();
            if (input == null || input.Handle != this)
            {
                input = gameObject.AddComponent<TerrainAreaHandleInput>();
                input.Handle = this;
            }
            m_projector = IOC.Resolve<ITerrainAreaProjector>();
            m_projector.Scale = new Vector3(0, 1, 0);
            base.OnEnable();
        }


        protected override void OnDisable()
        {
            base.OnDisable();
            m_initialized = false;
            m_projector = null;
        }

        protected override bool OnBeginDrag()
        {
            if(!base.OnBeginDrag())
            {
                return false;
            }
            
            if(SelectedAxis == RuntimeHandleAxis.Y)
            {
                Vector3 scale = WorldScaleToHeightMap(m_projector.Scale);

                Vector3 p = WorldToHeightMapPoint(Position);
                int res = m_terrain.terrainData.heightmapResolution;
                int rows = Mathf.FloorToInt(scale.z) + 1;
                int cols = Mathf.FloorToInt(scale.x) + 1;

                m_paddings = new[]
                {
                    -Mathf.Clamp(Mathf.RoundToInt(p.z - rows / 2), int.MinValue, 0),
                    -Mathf.Clamp(Mathf.RoundToInt(p.x - cols / 2), int.MinValue, 0),
                    -Mathf.Clamp(-(Mathf.RoundToInt(p.z + rows / 2) - res + 1), int.MinValue, 0),
                    -Mathf.Clamp(-(Mathf.RoundToInt(p.x + cols / 2) - res + 1), int.MinValue, 0)
                };

                rows -= m_paddings[0];
                cols -= m_paddings[1]; 
                rows -= m_paddings[2];
                cols -= m_paddings[3];

                if (rows < 0 || cols < 0)
                {
                    return false;
                }

                m_beginDragHeights = m_terrain.GetHeights(
                       Mathf.Clamp(Mathf.RoundToInt(p.x - cols / 2), 0, res - (cols + 1)),
                       Mathf.Clamp(Mathf.RoundToInt(p.z - rows / 2), 0, res - (rows + 1)), cols, rows);
                m_heights = new float[rows, cols];

                m_beginDragPoint = WorldToHeightMapPoint(Position);
                DragPlane = GetDragPlane(Vector3.up);
            }
            else if(SelectedAxis == RuntimeHandleAxis.Custom)
            {
                DragPlane = GetDragPlane(Matrix4x4.identity, Vector3.up);
            }
            else
            {
                return false;
            }

            if(!GetPointOnDragPlane(Window.Pointer, out m_prevPoint))
            {
                return false;
            }

            if(EnableUndo && SelectedAxis == RuntimeHandleAxis.Y)
            {
                BeginRecord();
            }
            
            return true;
        }

        protected override void OnDrag()
        {
            if (!Window.IsPointerOver)
            {
                return;
            }

            if(SelectedAxis == RuntimeHandleAxis.Y)
            {
                Vector3 pointOnDragPlane;
                if (!GetPointOnDragPlane(Window.Pointer, out pointOnDragPlane))
                {
                    return;
                }

                if (pointOnDragPlane != m_prevPoint)
                {
                    pointOnDragPlane.z = m_prevPoint.z;
                    pointOnDragPlane.x = m_prevPoint.x;

                    SetPosition(Position + pointOnDragPlane - m_prevPoint);
                    m_prevPoint = pointOnDragPlane;

                    int rows = m_heights.GetLength(0);
                    int cols = m_heights.GetLength(1);
                    Vector3 point = WorldToHeightMapPoint(Position);

                    float delta = (point - m_beginDragPoint).y;
                    float opacity = m_projector.BrushOpacity;
                    
                    for (int i = 0; i < rows; ++i)
                    {
                        float v = (m_paddings[0] + i) / (float)(m_paddings[0] + m_paddings[2] + rows - 1);
                        for (int j = 0; j < cols; ++j)
                        {
                            float u = (m_paddings[1] + j) / (float)(m_paddings[1] + m_paddings[3] + cols - 1);
                            Color c;
                            if (m_projector.Brush != null)
                            {
                                 c = m_projector.Brush.GetPixelBilinear(u, v);
                            }
                            else
                            {
                                c = Color.white;
                            }
                            
                            if(c.a < (1 - opacity))
                            {
                                c.a = 0;
                            }
                            m_heights[i, j] = m_beginDragHeights[i, j] + delta * c.a;
                        }
                    }
                    
                    float res = m_terrain.terrainData.heightmapResolution;
                    m_terrain.SetHeights(
                        (int)Mathf.Clamp(Mathf.RoundToInt(point.x - cols / 2), 0, res - (cols + 1)),
                        (int)Mathf.Clamp(Mathf.RoundToInt(point.z - rows / 2), 0, res - (rows + 1)), m_heights);

                    Vector3[] areaResizerPositions = AreaResizerPositions;
                    areaResizerPositions[areaResizerPositions.Length - 1] = Position;
                    SetAreaResizerPositions(areaResizerPositions);
                }
            }
            else if (SelectedAxis == RuntimeHandleAxis.Custom) 
            {
                Vector3 point = m_prevPoint;
                RaycastHit hit;
                if (Physics.Raycast(Window.Pointer, out hit))
                {
                    if (Editor.Selection.IsSelected(hit.collider.gameObject) && hit.collider is TerrainCollider)
                    {
                        point = hit.point;
                    }
                }

                if(m_prevPoint != point)
                {
                    if (m_areaResizerIndex >= 0 && m_areaResizerIndex < 4)
                    {
                        Vector3[] areaResizerPositions = AreaResizerPositions;
                        areaResizerPositions[m_areaResizerIndex] = point;
                        SetAreaResizerPositions(areaResizerPositions);
                        m_prevPoint = point;

                        areaResizerPositions = AreaResizerPositions;
                        Vector3 areaSize, areaCenter;
                        GetArea(areaResizerPositions, out areaSize, out areaCenter);
                        Position = PointToGrid(areaCenter);
                        SetProjector(areaSize, areaCenter);

                        areaResizerPositions[areaResizerPositions.Length - 1] = Position;
                        AreaResizerPositions = areaResizerPositions;

                        TryRefreshCommandBuffer();
                    }
                    else
                    {
                        Vector3[] areaResizerPositions = AreaResizerPositions;
                        areaResizerPositions[m_areaResizerIndex] = point;
                        SetAreaResizerPositions(areaResizerPositions);
                        SetPosition(point);
                        m_projector.Position = point;
                        m_prevPoint = point;
                        
                        TryRefreshCommandBuffer();
                    } 
                }
            }
        }

        protected override void OnDrop()
        {
            base.OnDrop();
           
            if (m_terrain != null)
            {
                Vector3 position = Position - m_terrain.GetPosition();
                position.y = Mathf.Clamp(position.y, 0, m_terrain.terrainData.size.y);
                position = PointToGrid(position + m_terrain.GetPosition());
                SetPosition(position);

                Vector3 projectorScale = m_projector.Scale;
                projectorScale.y = 0;

                Vector3[] areaResizerPositions = AreaResizerPositions;
                areaResizerPositions[0] = Position - projectorScale * 0.5f;
                areaResizerPositions[1] = Position + projectorScale * 0.5f;
                projectorScale.z *= -1;
                areaResizerPositions[2] = Position - projectorScale * 0.5f;
                areaResizerPositions[3] = Position + projectorScale * 0.5f;
                areaResizerPositions[4] = Position;

                SetAreaResizerPositions(areaResizerPositions);
                SetAreaResizersY();

                if(EnableUndo && SelectedAxis == RuntimeHandleAxis.Y)
                {
                    EndRecord();
                }
            }
        }

        public override RuntimeHandleAxis HitTest(out float distance)
        {
            if(IsDragging)
            {
                distance = 0;
                return SelectedAxis;
            }

            RuntimeHandleAxis axis = Appearance.HitTestPositionHandle(Window.Camera, Window.Pointer, m_drawingSettings, out distance);

            Vector3[] areaResizerPositions = AreaResizerPositions;

            Vector2 screenPoint = Window.Pointer.ScreenPoint;
            bool forceSelectAreaResizer = (screenPoint - (Vector2)Window.Camera.WorldToScreenPoint(Position)).magnitude <= Appearance.SelectionMargin * Appearance.SelectionMarginPixels;

            m_areaResizerIndex = -1;
            for(int i = areaResizerPositions.Length - 1; i >= 0; --i)
            {
                Vector2 areaResizer = Window.Camera.WorldToScreenPoint(areaResizerPositions[i]);
                
                float toAreaResizer = (screenPoint - areaResizer).magnitude;

                if ((forceSelectAreaResizer || toAreaResizer < distance) && toAreaResizer <= Appearance.SelectionMargin * Appearance.SelectionMarginPixels)
                {
                    distance = toAreaResizer;
                    axis = RuntimeHandleAxis.Custom;
                    m_areaResizerIndex = i;
                }
            }

            return axis;
        }

        protected override void UpdateOverride()
        {
            if (TrySelectAxis())
            {
                TryRefreshCommandBuffer();
            }
        }

        private RTHDrawingSettings m_drawingSettings = new RTHDrawingSettings
        {
            LockObject = new LockObject { PositionX = true, PositionZ = true },
            DrawLocked = false
        };
        protected override void RefreshCommandBuffer(IRTECamera camera)
        {
            base.RefreshCommandBuffer(camera);

            if(!m_initialized)
            {
                return;
            }

            Color[] colors = m_areaResizerMesh.colors;
            for(int i = 0; i < colors.Length; ++i)
            {
                colors[i] = Appearance.Colors.YColor;
            }

            if(SelectedAxis == RuntimeHandleAxis.Custom && m_areaResizerIndex >= 0)
            {
                colors[m_areaResizerIndex] = Appearance.Colors.SelectionColor;
            }

            m_areaResizerMesh.colors = colors;
            camera.CommandBuffer.DrawMesh(m_areaResizerMesh, Matrix4x4.identity, m_areaResizerMaterial, 0);

            m_drawingSettings.Position = transform.position;
            m_drawingSettings.Rotation = transform.rotation;
            m_drawingSettings.SelectedAxis = SelectedAxis;

            Appearance.DoPositionHandle(camera.CommandBuffer, camera.Camera, m_drawingSettings);

        }

        public virtual void ChangePosition()
        {
            Vector3 position = Position;
        
            RaycastHit hit;
            if (Physics.Raycast(Window.Pointer, out hit))
            {
                if (Editor.Selection.IsSelected(hit.collider.gameObject) && hit.collider is TerrainCollider)
                {
                    Vector3[] areaResizerPositions;
                    if (!m_initialized)
                    {
                        areaResizerPositions = new Vector3[5];
                        areaResizerPositions[0] = HeightMapPointToWorld(WorldScaleToHeightMap(hit.point) + new Vector3(1, 0, -1));
                        areaResizerPositions[1] = HeightMapPointToWorld(WorldScaleToHeightMap(hit.point) + new Vector3(-1, 0, 1));
                        areaResizerPositions[2] = HeightMapPointToWorld(WorldScaleToHeightMap(hit.point) + new Vector3(-1, 0, -1));
                        areaResizerPositions[3] = HeightMapPointToWorld(WorldScaleToHeightMap(hit.point) + new Vector3(1, 0, 1));
                        areaResizerPositions[4] = HeightMapPointToWorld(WorldScaleToHeightMap(hit.point));

                        AreaResizerPositions = areaResizerPositions;

                        Vector3 areaSize, areaCenter;
                        GetArea(areaResizerPositions, out areaSize, out areaCenter);
                        SetProjector(areaSize, areaCenter);
                    }

                    SetPosition(PointToGrid(hit.point));

                    areaResizerPositions = AreaResizerPositions;
                    Vector3 delta = Position - m_projector.Position;
                    for (int i = 0; i < areaResizerPositions.Length; ++i)
                    {
                        areaResizerPositions[i] += delta;
                    }
                    AreaResizerPositions = areaResizerPositions;
                    m_projector.Position += delta;

                    SetAreaResizersY();
                }
            }

            if (position != Position)
            {
                TryRefreshCommandBuffer();
            }
        }

        private Vector3 WorldScaleToHeightMap(Vector3 scale)
        {
            TerrainData data = m_terrain.terrainData;
            Vector3 size = data.size; 
            int resolution = data.heightmapResolution - 1;
            Vector3 result = new Vector3(Mathf.RoundToInt(scale.x * resolution / size.x), 1, Mathf.RoundToInt(scale.z * resolution / size.z));
            return result;
        }

        private Vector3 HeightMapToWorldScale(Vector3 scale)
        {
            TerrainData data = m_terrain.terrainData;
            Vector3 size = data.size;
            int resolution = data.heightmapResolution - 1;

            return new Vector3(scale.x * size.x / resolution, 1, scale.z * size.z / resolution);
        }

        private Vector3 WorldToHeightMapPoint(Vector3 point)
        {
            point = point - m_terrain.GetPosition();

            TerrainData data = m_terrain.terrainData;

            Vector3 size = data.size;
            Vector3 scale = data.heightmapScale;
            int resolution = data.heightmapResolution - 1;

            float x = Mathf.RoundToInt(resolution * point.x / size.x);
            float y = point.y / scale.y;
            float z = Mathf.RoundToInt(resolution * point.z / size.z);

            return new Vector3(x, y, z);
        }

        private Vector3 HeightMapPointToWorld(Vector3 point)
        {
            TerrainData terrainData = m_terrain.terrainData;

            Vector3 size = terrainData.size;
            int resolution = terrainData.heightmapResolution - 1;

            float x = point.x * size.x / resolution;
            float y = terrainData.GetHeight(Mathf.RoundToInt(point.x), Mathf.RoundToInt(point.z));
            float z = point.z * size.z / resolution;

            return m_terrain.GetPosition() + new Vector3(x, y, z);
        }

        private Vector3 PointToGrid(Vector3 point)
        {
            return HeightMapPointToWorld(WorldToHeightMapPoint(point));
        }

        private void SetPosition(Vector3 value)
        {
            foreach (BaseHandle handle in AllHandles)
            {
                if(handle is TerrainAreaHandle)
                {
                    TerrainAreaHandle terrainAreaHandle = (TerrainAreaHandle)handle;
                    terrainAreaHandle.m_initialized = true;
                    terrainAreaHandle.Position = value;
                    terrainAreaHandle.TryRefreshCommandBuffer();
                }   
            }
        }

        private void SetAreaResizerPositions(Vector3[] positions)
        {
            foreach (BaseHandle handle in AllHandles)
            {
                if (handle is TerrainAreaHandle)
                {
                    TerrainAreaHandle terrainAreaHandle = (TerrainAreaHandle)handle;
                    terrainAreaHandle.AreaResizerPositions = positions;
                    terrainAreaHandle.TryRefreshCommandBuffer();
                }
            }
        }

        private void SetAreaResizersY()
        {
            Vector3[] resizerPositions = AreaResizerPositions;
            for(int i = 0; i < resizerPositions.Length; ++i)
            {
                resizerPositions[i] = HeightMapPointToWorld(WorldToHeightMapPoint(resizerPositions[i]));
            }

            SetAreaResizerPositions(resizerPositions);
        }

        private void GetArea(Vector3[] areaResizerPositions, out Vector3 areaSize, out Vector3 areaCenter)
        {
            if (m_areaResizerIndex < 2)
            {
                areaSize = HeightMapToWorldScale(WorldScaleToHeightMap((areaResizerPositions[1] - areaResizerPositions[0]) * 0.5f));
                areaCenter = m_areaResizerIndex == 1 ? areaResizerPositions[0] + areaSize : areaResizerPositions[1] - areaSize;
            }
            else
            {
                areaSize = HeightMapToWorldScale(WorldScaleToHeightMap((areaResizerPositions[3] - areaResizerPositions[2]) * 0.5f));
                areaCenter = m_areaResizerIndex == 3 ? areaResizerPositions[2] + areaSize : areaResizerPositions[3] - areaSize;
            }
        }

        private void SetProjector(Vector3 areaSize, Vector3 areaCenter)
        {
            if(m_projector == null)
            {
                return;
            }

            m_projector.Position = PointToGrid(areaCenter);

            float scaleX = Mathf.Abs(areaSize.x * 2);
            float scaleZ = Mathf.Abs(areaSize.z * 2);

            Vector3 projectorScale = new Vector3(scaleX, 1, scaleZ);
            projectorScale.x = Mathf.Max(projectorScale.x, 0.1f);
            projectorScale.z = Mathf.Max(projectorScale.z, 0.1f);
            m_projector.Scale = projectorScale;
        }


        private void BuildPointsMesh(Mesh target, int pointsCount)
        {
            Vector3[] vertices = new Vector3[pointsCount];
            int[] indices = new int[pointsCount];
            Color[] colors = new Color[pointsCount];
            for(int i = 0; i < pointsCount; i++)
            {
                indices[i] = i;
                colors[i] = Color.white;
            }

            target.Clear();
            target.subMeshCount = 1;
            target.name = "TerrainAreaHandleVertices";
            target.vertices = vertices;
            target.SetIndices(indices, MeshTopology.Points, 0);
            target.colors = colors;
            target.RecalculateBounds();
        }

        private void BeginRecord()
        {
            m_oldHeights = GetHeightmap();
            m_oldResizerPositions = AreaResizerPositions.ToArray();
        }

        private void EndRecord()
        {
            Terrain terrain = m_terrain;
            terrain.TerrainColliderWithoutHoles();

            float[,] oldHeightmap = m_oldHeights;
            float[,] newHeightmap = GetHeightmap();

            Vector3[] oldResizerPositions = m_oldResizerPositions;
            Vector3[] newResizerPositions = AreaResizerPositions.ToArray();

            m_oldHeights = null;
            m_oldResizerPositions = null;

            Editor.Undo.CreateRecord(record =>
            {
                if(terrain.terrainData != null)
                {
                    terrain.SetHeights(0, 0, newHeightmap);
                    terrain.TerrainColliderWithoutHoles();
                }

                SetAreaResizerPositions(newResizerPositions);
                SetPosition(newResizerPositions.Last());
                Vector3 areaSize, areaCenter;
                GetArea(oldResizerPositions, out areaSize, out areaCenter);
                SetProjector(areaSize, areaCenter);

                return true;
            },
            record =>
            {
                if(terrain.terrainData != null)
                {
                    terrain.SetHeights(0, 0, oldHeightmap);
                    terrain.TerrainColliderWithoutHoles();
                }

                SetAreaResizerPositions(oldResizerPositions);
                SetPosition(oldResizerPositions.Last());
                Vector3 areaSize, areaCenter;
                GetArea(oldResizerPositions, out areaSize, out areaCenter);
                SetProjector(areaSize, areaCenter);
             
                return true;
            });
        }

        protected override void BeginRecordTransform()
        {
        }

        protected override void EndRecordTransform()
        {
        }

        protected override void OnUndoCompleted()
        {
        }

        protected override void OnRedoCompleted()
        {
        }

        private float[,] GetHeightmap()
        {
            int w = m_terrain.terrainData.heightmapResolution;
            int h = m_terrain.terrainData.heightmapResolution;
            return m_terrain.terrainData.GetHeights(0, 0, w, h);
        }
    }
}
