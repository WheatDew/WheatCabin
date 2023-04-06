using Battlehub.RTCommon;
using Battlehub.RTEditor;
using Battlehub.RTSL.Interface;
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Battlehub.RTTerrain
{
    public interface ITerrainSettings
    {
        Vector3 Position
        {
            get;
            set;
        }
        float Width
        {
            get;
            set;
        }

        float Length
        {
            get;
            set;
        }

        float Resolution
        {
            get;
            set;
        }

        Texture2D DefaultTexture
        {
            get;
            set;
        }

        void ApplyDefaultTexture();
        void Refresh();
        void InitEditor(PropertyEditor editor, PropertyInfo property, string label);
        void InitTexturePicker(TexturePicker editor, PropertyInfo property);
        
    }

    public class TerrainSettings : EditorExtension, ITerrainSettings
    {
        private Terrain m_terrain;

        public Vector3 Position
        {
            get { return GetValueSafe(() => m_terrain.transform.localPosition); }
            set
            {
                SetValueSafe(value, v =>
                {
                    m_terrain.transform.localPosition = v;
                });
            }
        }

        public float Width
        {
            get;
            set;
        }

        public float Length
        {
            get;
            set;
        }

        public float Resolution
        {
            get;
            set;
        }

        private Texture2D m_defaultTexture;
        public Texture2D DefaultTexture
        {
            get
            {
                return m_defaultTexture;
            }
            set
            {
                if(m_defaultTexture != value)
                {
                    m_defaultTexture = value;
                    m_editor.IsBusy = true;
                    m_playerPrefs.SetValue("Battlehub.RTTerrain.TerrainSettings.Texture", value, (error) =>
                    {
                        m_editor.IsBusy = false;
                    });
                }   
            }
        }

        private T GetValueSafe<T>(Func<T> func)
        {
            if (m_terrain == null || m_terrain.terrainData == null)
            {
                return default;
            }

            return func();
        }

        private void SetValueSafe<T>(T value, Action<T> action)
        {
            if (m_terrain == null || m_terrain.terrainData == null)
            {
                return;
            }

            action(value);
        }

        private float[,] GetHeightmap()
        {
            int w = m_terrain.terrainData.heightmapResolution;
            int h = m_terrain.terrainData.heightmapResolution;
            return m_terrain.terrainData.GetHeights(0, 0, w, h);
        }

        private IRTE m_editor;
        private IPlayerPrefsStorage m_playerPrefs;

        protected override void OnEditorExist()
        {
            base.OnEditorExist();
            m_editor = IOC.Resolve<IRTE>();
            m_playerPrefs = IOC.Resolve<IPlayerPrefsStorage>();

            OnSelectionChanged(null);
            m_editor.Selection.SelectionChanged += OnSelectionChanged;
            IOC.RegisterFallback<ITerrainSettings>(this);

            m_playerPrefs.GetValue<Texture2D>("Battlehub.RTTerrain.TerrainSettings.Texture", (error, texture) =>
            {
                if (texture == null)
                {
                    m_defaultTexture = (Texture2D)Resources.Load("Textures/RTT_DefaultGrass");
                }
                else
                {
                    m_defaultTexture = texture;
                }

                if(m_defaultTexture != null)
                {
                    m_defaultTexture.hideFlags = HideFlags.None;
                }
            });
        }

        protected override void OnEditorClosed()
        {
            base.OnEditorClosed();
            if (m_editor != null)
            {
                m_editor.Selection.SelectionChanged -= OnSelectionChanged;
            }
            IOC.UnregisterFallback<ITerrainSettings>(this);
        }

        private void OnSelectionChanged(UnityEngine.Object[] unselectedObjects)
        {
            if (m_editor.Selection.activeGameObject == null)
            {
                m_terrain = null;
            }
            else
            {
                m_terrain = m_editor.Selection.activeGameObject.GetComponent<Terrain>();
            }

            if(m_terrain != null)
            {
                Width = GetValueSafe(() => m_terrain.terrainData.size.x);
                Length = GetValueSafe(() => m_terrain.terrainData.size.z);
                Resolution = GetValueSafe(() => m_terrain.terrainData.heightmapResolution);
            }
        }

        public void ApplyDefaultTexture()
        {
            Terrain[] terrainObjects = m_editor.Object.Get(false).Where(o => o != null).Select(o => o.GetComponent<Terrain>()).Where(t => t != null).ToArray();
            m_editor.Undo.BeginRecord();

            for(int i = 0; i < terrainObjects.Length; ++i)
            {
                Terrain terrain = terrainObjects[i];
                TerrainLayer[] oldLayers = terrain.terrainData.terrainLayers.ToArray();
                TerrainLayer[] newLayers = terrain.terrainData.terrainLayers.ToArray();
                if(newLayers.Length > 0 && newLayers[0] != null)
                {
                    TerrainLayer layer = Instantiate(newLayers[0]);
                    layer.diffuseTexture = m_defaultTexture;
                    newLayers[0] = layer;
                    terrain.terrainData.terrainLayers = newLayers;

                    m_editor.Undo.CreateRecord(record =>
                    {
                        if (terrain != null && terrain.terrainData != null)
                        {
                            terrain.terrainData.terrainLayers = newLayers;
                            TerrainLayerEditor.UpdateLayersList(terrain);
                        }

                        return true;
                    },
                    record =>
                    {
                        if (terrain != null && terrain.terrainData != null)
                        {
                            terrain.terrainData.terrainLayers = oldLayers;
                            TerrainLayerEditor.UpdateLayersList(terrain);
                        }

                        return true;
                    });
                }
            }

            m_editor.Undo.EndRecord();

        }

        public void Refresh()
        {
            if (m_terrain == null || m_terrain.terrainData == null)
            {
                return;
            }

            Vector3 oldSize = m_terrain.terrainData.size;
            Vector3 newSize = m_terrain.terrainData.size;
            newSize.x = Mathf.Clamp(Width, 10, 500);
            newSize.z = Mathf.Clamp(Length, 10, 500);

            int oldHeightmapResolution = m_terrain.terrainData.heightmapResolution;
            int newHeightmapResolution = Mathf.Clamp(Mathf.RoundToInt(Resolution), 17, 2049);

            float[,] oldHeightMap = null;
            float[,] newHeightMap = null;
            if (oldHeightmapResolution != newHeightmapResolution)
            {
                oldHeightMap = GetHeightmap();
                m_terrain.terrainData.heightmapResolution = newHeightmapResolution;
                newHeightMap = GetHeightmap();
            }

            Terrain terrain = m_terrain;
            Action undo = () =>
            {
                if (terrain == null || terrain.terrainData == null)
                {
                    return;
                }

                if (terrain.terrainData.heightmapResolution != oldHeightmapResolution)
                {
                    terrain.terrainData.heightmapResolution = oldHeightmapResolution;
                    terrain.SetHeights(0, 0, oldHeightMap);
                }
                terrain.SetSize(oldSize);

                Width = terrain.terrainData.size.x;
                Length = terrain.terrainData.size.z;
                Resolution = terrain.terrainData.heightmapResolution;
            };

            Action redo = () =>
            {
                if (terrain == null || terrain.terrainData == null)
                {
                    return;
                }

                if (terrain.terrainData.heightmapResolution != newHeightmapResolution)
                {
                    terrain.terrainData.heightmapResolution = newHeightmapResolution;
                    terrain.SetHeights(0, 0, newHeightMap);
                }
                terrain.SetSize(newSize);

                Width = terrain.terrainData.size.x;
                Length = terrain.terrainData.size.z;
                Resolution = terrain.terrainData.heightmapResolution;
            };

            redo();

            ITerrainGridTool tool = IOC.Resolve<ITerrainGridTool>();
            if (tool == null || !tool.Refresh(redo, undo))
            {
                IRTE editor = IOC.Resolve<IRTE>();
                editor.Undo.CreateRecord(record => { redo(); return true; }, record => { undo(); return true; });
            }
        }

        public void InitEditor(PropertyEditor editor, PropertyInfo property, string label)
        {
            editor.Init(m_terrain, this, property, null, label, null, null, Refresh, false);
        }

        public void InitTexturePicker(TexturePicker picker, PropertyInfo property)
        {
            picker.Init(this, property);
        }
    }
}

