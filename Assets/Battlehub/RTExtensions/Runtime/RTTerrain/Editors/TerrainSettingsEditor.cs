using Battlehub.RTCommon;
using Battlehub.RTEditor;
using Battlehub.Utils;
using System;
using System.Reflection;
using UnityEngine;

namespace Battlehub.RTTerrain
{
    public class TerrainSettingsEditor : MonoBehaviour
    {
        [SerializeField]
        private FloatEditor m_widthEditor = null;
        [SerializeField]
        private FloatEditor m_lengthEditor = null;
        [SerializeField]
        private FloatEditor m_heightmapResolutionEditor = null;
        [SerializeField]
        private Vector3Editor m_positionEditor = null;
        [SerializeField]
        private TexturePicker m_texturePicker = null;

        private TerrainEditor m_terrainEditor;
        protected TerrainEditor TerrainEditor
        {
            get { return m_terrainEditor; }
        }

        private ITerrainSettings m_terrainSettings;
        private ILocalization m_localization;

        protected virtual void Awake()
        {
            m_localization = IOC.Resolve<ILocalization>();
            m_terrainSettings = IOC.Resolve<ITerrainSettings>();

            m_terrainEditor = GetComponentInParent<TerrainEditor>();
            m_terrainEditor.TerrainChanged += OnTerrainChanged;

            if (m_texturePicker != null)
            {
                m_terrainSettings.InitTexturePicker(m_texturePicker, Strong.PropertyInfo((ITerrainSettings x) => x.DefaultTexture));
                m_texturePicker.TextureChanged += OnDefaultTextureChanged;
            }
        }

        protected virtual void OnDestroy()
        {
            if(m_terrainEditor != null)
            {
                m_terrainEditor.TerrainChanged -= OnTerrainChanged;
            }

            if(m_texturePicker != null)
            {
                m_texturePicker.TextureChanged -= OnDefaultTextureChanged;
            }
        }

        protected virtual void OnEnable()
        {
            InitEditors();
        }

        protected virtual void OnDisable()
        {
            
        }

        private void OnDefaultTextureChanged(object sender, EventArgs e)
        {
            IWindowManager wm = IOC.Resolve<IWindowManager>();
            wm.Confirmation(
                m_localization.GetString("ID_RTTerrain_Settings_DefaultTextureChanged", "Default terrain texture changed"), 
                m_localization.GetString("ID_RTTerrain_Settings_WouldYouLikeToApplyTexture", "Would you like to apply this texture to the terrain in the scene?"),
                (dialog, okArgs) =>
                {
                    m_terrainSettings.ApplyDefaultTexture();
                },
                (dialog, cancelArgs) => { },
                m_localization.GetString("ID_RTTerrain_Settings_Yes", "Yes"),
                m_localization.GetString("ID_RTTerrain_Settings_No", "No"));

        }

        private void OnTerrainChanged()
        {
            InitEditors();
        }

        private void InitEditors()
        {
            Terrain terrain = TerrainEditor.Terrain;
            if (terrain != null)
            {
                if (m_widthEditor != null)
                {
                    m_terrainSettings.InitEditor(m_widthEditor, Strong.PropertyInfo((ITerrainSettings x) => x.Width), m_localization.GetString("ID_RTTerrain_Settings_Width", "Width"));
                }

                if (m_lengthEditor != null)
                {
                    m_terrainSettings.InitEditor(m_lengthEditor, Strong.PropertyInfo((ITerrainSettings x) => x.Length), m_localization.GetString("ID_RTTerrain_Settings_Length", "Length"));
                }

                if(m_heightmapResolutionEditor != null)
                {
                    m_terrainSettings.InitEditor(m_heightmapResolutionEditor, Strong.PropertyInfo((ITerrainSettings x) => x.Resolution), m_localization.GetString("ID_RTTerrain_Settings_Resolution", "Resolution"));
                }

                if(m_positionEditor != null)
                {
                    m_terrainSettings.InitEditor(m_positionEditor, Strong.PropertyInfo((ITerrainSettings x) => x.Position), m_localization.GetString("ID_RTTerrain_Settings_Position", "Position"));
                }
            }
        }

        

    }
}

