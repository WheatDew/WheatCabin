using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Battlehub.RTEditor
{
    public interface IToolCmd
    {
        object Arg
        {
            get;
            set;
        }
        object Run();
        bool Validate();
    }

    public class ToolCmd : IToolCmd
    {
        private Func<object, object> m_func;
        private Func<bool> m_validate;
        public string Text;
        public bool CanDrag;

        public ToolCmd Parent;
        public List<ToolCmd> Children;
        public bool HasChildren
        {
            get { return Children != null && Children.Count > 0; }
        }
        public bool HasParent
        {
            get { return Parent != null; }
        }

        public object Arg
        {
            get;
            set;
        }

        public ToolCmd(string text, Func<object, object> func, bool canDrag = false) : this(text, func, () => true, canDrag)
        {
        }

        public ToolCmd(string text, Func<object, object> func, Func<bool> validate = null, bool canDrag = false)
        {
            Text = text;
            m_func = func;
            m_validate = validate;
            if (m_validate == null)
            {
                m_validate = () => true;
            }
            CanDrag = canDrag;
        }

        public ToolCmd(string text, Action action, Func<bool> validate = null, bool canDrag = false)
        {
            Text = text;
            m_func = args => { action(); return null; };
            m_validate = validate;
            if (m_validate == null)
            {
                m_validate = () => true;
            }
            CanDrag = canDrag;
        }

        public object Run()
        {
            return m_func(Arg);
        }

        public bool Validate()
        {
            return m_validate();
        }
    }

    public class ToolCmdItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField]
        private Graphic m_graphics = null;

        [SerializeField]
        private Color m_normalColor = Color.white;
        public Color NormalColor
        {
            get { return m_normalColor; }
            set
            {
                m_normalColor = value;
                UpdateVisualState();
            }
        }

        [SerializeField]
        private Color m_pointerOverColor = Color.white;
        public Color PointerOverColor
        {
            get { return m_pointerOverColor; }
            set
            {
                m_pointerOverColor = value;
                UpdateVisualState();
            }
        }

        [SerializeField]
        private Color m_pressedColor = Color.white;
        public Color PressedColor
        {
            get { return m_pressedColor; }
            set
            {
                m_pressedColor = value;
                UpdateVisualState();
            }
        }

        private bool m_isPointerOver;
        private bool m_isPointerPressed;

        private void Awake()
        {
            UpdateVisualState();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            m_isPointerOver = true;
            UpdateVisualState();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            m_isPointerOver = false;
            UpdateVisualState();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            m_isPointerPressed = true;
            UpdateVisualState();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            m_isPointerPressed = false;
            UpdateVisualState();
        }

        private void UpdateVisualState()
        {
            if(m_graphics != null)
            {
                if (m_isPointerPressed)
                {
                    m_graphics.color = m_pressedColor;
                }
                else
                {
                    if(m_isPointerOver)
                    {
                        m_graphics.color = m_pointerOverColor;
                    }
                    else
                    {
                        m_graphics.color = m_normalColor;
                    }
                }
            }
            
        }  
    }
}

