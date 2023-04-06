using Battlehub.RTCommon;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Battlehub.RTEditor
{
    public class TexturePickerDropArea : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public event EventHandler Drop;

        private IObjectEditorLoader m_loader;
        private IRTE m_editor;
        public IRTE Editor
        {
            get { return m_editor; }
        }
        
        [SerializeField]
        private GameObject m_highlight = null;

        private bool m_isPointerOver;
        private bool IsPointerOver
        {
            get { return m_isPointerOver; }
            set
            {
                if(m_isPointerOver != value)
                {
                    m_isPointerOver = value;
                    m_highlight.SetActive(value);
                }
            }
        }

        private void Awake()
        {
            m_editor = IOC.Resolve<IRTE>();
            m_loader = IOC.Resolve<IObjectEditorLoader>();
            if (m_loader == null)
            {
                m_loader = m_editor.Root.gameObject.AddComponent<ObjectEditorLoader>();
            }
        }

        private void Start()
        {
            m_highlight.SetActive(IsPointerOver);
            m_editor.DragDrop.Drop += OnDrop;
        }

        private void OnDestroy()
        {
            if(m_editor != null && m_editor.DragDrop != null)
            {
                m_editor.DragDrop.Drop -= OnDrop;
            }
        }

        private void OnDrop(PointerEventData pointerEventData)
        {
            if(!IsPointerOver)
            {
                return;
            }

            if (CanDrop())
            {
                Drop(this, EventArgs.Empty);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!m_editor.DragDrop.InProgress)
            {
                return;
            }

            IObjectEditorLoader loader = IOC.Resolve<IObjectEditorLoader>();
            if (CanDrop())
            {
                m_editor.DragDrop.SetCursor(Utils.KnownCursor.DropAllowed);
            }
            else
            {
                m_editor.DragDrop.SetCursor(Utils.KnownCursor.DropNotAllowed);
            }

            IsPointerOver = true;
        }

        private bool CanDrop()
        {
            if(m_editor.DragDrop.DragObjects == null || m_editor.DragDrop.DragObjects.Length == 0)
            {
                return false;
            }

            IObjectEditorLoader loader = IOC.Resolve<IObjectEditorLoader>();
            return loader.GetObjectType(m_editor.DragDrop.DragObjects[0], typeof(Texture2D)) == typeof(Texture2D);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if(m_editor.DragDrop.InProgress)
            {
                m_editor.DragDrop.SetCursor(Utils.KnownCursor.DropNotAllowed);
            }
            
            IsPointerOver = false;
        }
    }
}
