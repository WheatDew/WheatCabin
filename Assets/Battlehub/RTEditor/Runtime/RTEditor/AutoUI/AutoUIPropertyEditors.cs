using Battlehub.UIControls;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

using UnityObject = UnityEngine.Object;

namespace Battlehub.RTEditor.UI
{
    public partial class AutoUI
    {
        public Tuple<RectTransform, LayoutElement> PropertyEditor(PropertyInfo propertyInfo,  bool createLayoutElement = true, bool wrap = true)
        {
            RectTransform panel = m_panelStack.Peek();

            GameObject editor = m_editorsMap.GetPropertyEditor(propertyInfo.PropertyType);
            if (editor == null)
            {
                editor = new GameObject("NoEditor");
            }
            else
            {
                string name = editor.name;
                editor = UnityObject.Instantiate(editor);
                editor.name = name;
            }

            if (wrap)
            {
                GameObject wrapper = new GameObject();
                wrapper.name = $"{editor.name} Layout";
                wrapper.transform.SetParent(panel, false);
                wrapper.AddComponent<RectTransform>();

                editor.transform.SetParent(wrapper.transform);
                RectTransform rt = (RectTransform)editor.transform;
                rt.Stretch();

                return new Tuple<RectTransform, LayoutElement>((RectTransform)editor.transform, CreateLayoutElement(wrapper, createLayoutElement));
            }

            return new Tuple<RectTransform, LayoutElement>((RectTransform)editor.transform, CreateLayoutElement(editor, createLayoutElement));
        }

        private static LayoutElement CreateLayoutElement(GameObject editor, bool createLayoutElement)
        {
            LayoutElement layoutElement = editor.GetComponent<LayoutElement>();
            if (layoutElement == null && createLayoutElement)
            {
                layoutElement = editor.gameObject.AddComponent<LayoutElement>();
            }

            return layoutElement;
        }
    }
}

