using Battlehub.RTCommon;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTEditor
{
    public class TexturePicker : MonoBehaviour
    {
        public event EventHandler TextureChanged;

        [SerializeField]
        private ObjectEditor m_objectEditor = null;

        [SerializeField]
        private TexturePickerDropArea m_dropArea = null;

        [SerializeField]
        private RawImage m_texturePreview = null;

        private bool m_initialized;

        private Texture2D m_texture;
        public Texture2D Texture
        {
            get { return m_texture; }
            set
            {
                if (m_texture != value)
                {
                    m_texture = value;
                    UpdatePreview();

                    if (TextureChanged != null)
                    {
                        TextureChanged(this, EventArgs.Empty);
                    }

                }
            }
        }

        private void UpdatePreview()
        {
            if (m_texturePreview != null)
            {
                m_texturePreview.gameObject.SetActive(m_texture != null);
                m_texturePreview.texture = m_texture;
            }
        }

        private object m_target;
        private PropertyInfo m_property;
        private bool m_enableUndo = true;

        public void Init(object target, PropertyInfo property, bool enableUndo = true)
        {
            m_enableUndo = enableUndo;
            m_target = target;
            m_property = property;

            object value = m_property.GetValue(m_target);
            m_texture = value as Texture2D;
            UpdatePreview();

            enabled = true;

            if (m_objectEditor != null)
            {
                m_objectEditor.Init(target, target, property, null, null, null, null, null, enableUndo);
            }

            if (m_dropArea != null)
            {
                m_dropArea.Drop += OnDrop;
            }
        }

        private void OnDestroy()
        {
            if (m_dropArea != null)
            {
                m_dropArea.Drop -= OnDrop;
            }
        }

        public void Reload()
        {
            m_objectEditor.Reload();
            ReloadTexture();
        }

        private void OnDrop(object sender, System.EventArgs e)
        {
            IObjectEditorLoader loader = IOC.Resolve<IObjectEditorLoader>();
            loader.Load(m_dropArea.Editor.DragDrop.DragObjects[0], typeof(Texture2D), obj =>
            {
                if(m_property != null && m_target != null)
                {
                    IRTE rte = IOC.Resolve<IRTE>();
                    if(m_enableUndo)
                    {
                        rte.Undo.BeginRecordValue(m_target, m_property);
                    }
                    m_property.SetValue(m_target, obj);
                    if(m_enableUndo)
                    {
                        rte.Undo.EndRecordValue(m_target, m_property);
                    }
                    Texture = obj as Texture2D;
                }
            });
        }

        private float m_nextUpdate;
        private void Update()
        {
            if (m_nextUpdate > Time.time)
            {
                return;
            }
            m_nextUpdate = Time.time + 0.2f;

            if (m_property == null || m_target == null)
            {
                enabled = false;
                return;
            }

            ReloadTexture();
        }

        private void ReloadTexture()
        {
            object value = m_property.GetValue(m_target);
            if (!ReferenceEquals(value, Texture))
            {
                Texture = value as Texture2D;
            }
        }
    }
}


