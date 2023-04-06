using Battlehub.UIControls;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTEditor.UI
{
    public partial class AutoUI 
    {
        public Tuple<VerticalLayoutGroup, LayoutElement> BeginVerticalLayout(bool createLayoutElement = false)
        {
            return BeginLayout<VerticalLayoutGroup>(createLayoutElement);
        }

        public void EndVerticalLayout()
        {
            EndLayout();
        }

        public Tuple<HorizontalLayoutGroup, LayoutElement> BeginHorizontalLayout(bool createLayoutElement = false)
        {
            return BeginLayout<HorizontalLayoutGroup>(createLayoutElement);
        }

        public void EndHorizontalLayout()
        {
            EndLayout();
        }

        public Tuple<T, LayoutElement> BeginLayout<T>(bool createLayoutElement = false) where T : LayoutGroup
        {
            RectTransform panel = m_panelStack.Peek();
            LayoutElement layoutElement = null;
            if (panel.GetComponent<LayoutGroup>())
            {
                Transform childPanel = new GameObject().transform;
                childPanel.name = nameof(T);
                childPanel.transform.SetParent(panel, false);

                panel = childPanel.gameObject.AddComponent<RectTransform>();
                panel.Stretch();

                if(createLayoutElement)
                {
                    layoutElement = panel.gameObject.AddComponent<LayoutElement>();
                }
                
                m_panelStack.Push(panel);
            }

            return new Tuple<T, LayoutElement>(panel.gameObject.AddComponent<T>(), layoutElement);
        }

        public void EndLayout()
        {
            if (m_panelStack.Count > 0)
            {
                m_panelStack.Pop();
            }
        }
    }

}
