using Battlehub.RTGizmos;
using Battlehub.RTCommon;
using UnityEngine;

namespace Battlehub.RTEditor
{
    [BuiltInDescriptor]
    public class RectTransformComponentDescriptor : ComponentDescriptorBase<RectTransform, RectTransformGizmo>
    {
        public override object CreateConverter(ComponentEditor editor)
        {
            object[] converters = new object[editor.Components.Length];
            Component[] components = editor.Components;
            for (int i = 0; i < components.Length; ++i)
            {
                RectTransform rt = (RectTransform)components[i];
                if (rt != null)
                {
                    converters[i] = new RectTransformPropertyConverter
                    {
                        RectTransform = rt,
                        ExposeToEditor = rt.GetComponent<ExposeToEditor>()
                    };
                }
            }
            return converters;
        }

        private readonly PropertyDescriptor[] m_properties = new[] { new PropertyDescriptor() };
        public override PropertyDescriptor[] GetProperties(ComponentEditor editor, object converter)
        {
            //property descriptors created in RectTransformEditor.BuildEditor method. 
            //return array with 1 empty descriptor to trigger BuildEditor method
            return m_properties;
        }
    }

    public class RectTransformPropertyConverter
    {
        public Vector3 Pos
        {
            get
            {
                if (RectTransform == null)
                {
                    return Vector3.zero;
                }

                Vector3 aPos = RectTransform.anchoredPosition;

                Vector2 aMin = RectTransform.anchorMin;
                Vector2 aMax = RectTransform.anchorMax;

                Vector2 oMin = RectTransform.offsetMin;
                Vector2 oMax = RectTransform.offsetMax;

                return new Vector3(Approximately(aMin.x, aMax.x) ? aPos.x : oMin.x, Approximately(aMin.y, aMax.y) ? aPos.y : -oMax.y, RectTransform.localPosition.z);
            }
            set
            {
                if (RectTransform == null)
                {
                    return;
                }

                Vector2 aMin = RectTransform.anchorMin;
                Vector2 aMax = RectTransform.anchorMax;

                if (Approximately(aMin.x, aMax.x))
                {
                    Vector3 aPos = RectTransform.anchoredPosition;
                    aPos.x = value.x;
                    RectTransform.anchoredPosition = aPos;
                }
                else
                {
                    Vector2 oMin = RectTransform.offsetMin;
                    oMin.x = value.x;
                    RectTransform.offsetMin = oMin;
                }

                if (Approximately(aMin.y, aMax.y))
                {
                    Vector3 aPos = RectTransform.anchoredPosition;
                    aPos.y = value.y;
                    RectTransform.anchoredPosition = aPos;
                }
                else
                {
                    Vector2 oMax = RectTransform.offsetMax;
                    oMax.y = -value.y;
                    RectTransform.offsetMax = oMax;
                }

                Vector3 localPosition = RectTransform.localPosition;
                localPosition.z = value.z;
                RectTransform.localPosition = localPosition;

            }
        }

        public Vector3 Size
        {
            get
            {
                if (RectTransform == null)
                {
                    return Vector3.zero;
                }

                Vector2 aMin = RectTransform.anchorMin;
                Vector2 aMax = RectTransform.anchorMax;

                Vector2 oMin = RectTransform.offsetMin;
                Vector2 oMax = RectTransform.offsetMax;

                Vector3 size = RectTransform.sizeDelta;

                return new Vector3(Approximately(aMin.x, aMax.x) ? size.x : -oMax.x, Approximately(aMin.y, aMax.y) ? size.y : oMin.y, 0);
            }
            set
            {
                if (RectTransform == null)
                {
                    return;
                }

                Vector2 aMin = RectTransform.anchorMin;
                Vector2 aMax = RectTransform.anchorMax;

                if (Approximately(aMin.x, aMax.x))
                {
                    Vector3 size = RectTransform.sizeDelta;
                    size.x = value.x;
                    RectTransform.sizeDelta = size;
                }
                else
                {
                    Vector2 oMax = RectTransform.offsetMax;
                    oMax.x = -value.x;
                    RectTransform.offsetMax = oMax;
                }

                if (Approximately(aMin.y, aMax.y))
                {
                    Vector3 size = RectTransform.sizeDelta;
                    size.y = value.y;
                    RectTransform.sizeDelta = size;
                }
                else
                {
                    Vector2 oMin = RectTransform.offsetMin;
                    oMin.y = value.y;
                    RectTransform.offsetMin = oMin;
                }
            }
        }

        public Vector3 LocalEuler
        {
            get
            {
                if (ExposeToEditor == null)
                {
                    return RectTransform.localEulerAngles;
                }

                return ExposeToEditor.LocalEuler;
            }
            set
            {
                if (ExposeToEditor == null)
                {
                    RectTransform.localEulerAngles = value;
                    return;
                }
                ExposeToEditor.LocalEuler = value;
            }
        }

        public Vector3 LocalScale
        {
            get
            {
                if (ExposeToEditor == null)
                {
                    return RectTransform.localScale;
                }

                return ExposeToEditor.LocalScale;
            }
            set
            {
                if (ExposeToEditor == null)
                {
                    RectTransform.localScale = value;
                    return;
                }
                ExposeToEditor.LocalScale = value;
            }
        }


        public RectTransform RectTransform
        {
            get;
            set;
        }

        public ExposeToEditor ExposeToEditor
        {
            get;
            set;
        }

        public static bool Approximately(float a, float b)
        {
            return Mathf.Approximately(a, b);
        }
    }
}

