
using Battlehub.RTCommon;
using Battlehub.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTEditor
{
    public class LayoutElementPropertyConverter
    {
        public float preferredWidth
        {
            get { return LayoutElement.preferredWidth; }
            set 
            {
                float oldValue = LayoutElement.preferredWidth;
                if (oldValue != value)
                {
                    if (oldValue == -1)
                    {
                        RectTransform rt = LayoutElement.GetComponent<RectTransform>();
                        if (rt != null)
                        {
                            LayoutElement.preferredWidth = LayoutUtility.GetPreferredWidth(rt);
                            return;
                        }
                    }
                    LayoutElement.preferredWidth = value;
                }
            }
        }

        public float preferredHeight
        {
            get { return LayoutElement.preferredHeight; }
            set
            {
                float oldValue = LayoutElement.preferredHeight;
                if (oldValue != value)
                {
                    if (oldValue == -1)
                    {
                        RectTransform rt = LayoutElement.GetComponent<RectTransform>();
                        if (rt != null)
                        {
                            LayoutElement.preferredHeight = LayoutUtility.GetPreferredHeight(rt);
                            return;
                        }
                    }
                    LayoutElement.preferredHeight = value;
                }
            }
        }

        public float flexibleWidth
        {
            get { return LayoutElement.flexibleWidth; }
            set
            {
                float oldValue = LayoutElement.flexibleWidth;
                if (oldValue != value)
                {
                    if (oldValue == -1)
                    {
                        RectTransform rt = LayoutElement.GetComponent<RectTransform>();
                        if (rt != null)
                        {
                            LayoutElement.flexibleWidth = LayoutUtility.GetFlexibleWidth(rt);
                            return;
                        }
                    }
                    LayoutElement.flexibleWidth = value;
                }
            }
        }

        public float flexibleHeight
        {
            get { return LayoutElement.flexibleHeight; }
            set
            {
                float oldValue = LayoutElement.flexibleHeight;
                if (oldValue != value)
                {
                    if (oldValue == -1)
                    {
                        RectTransform rt = LayoutElement.GetComponent<RectTransform>();
                        if (rt != null)
                        {
                            LayoutElement.flexibleHeight = LayoutUtility.GetFlexibleHeight(rt);
                            return;
                        }
                    }
                    LayoutElement.flexibleHeight = value;
                }
            }
        }

        public LayoutElement LayoutElement
        {
            get;
            set;
        }
    }


    [BuiltInDescriptor]
    public class LayoutElementComponentDescriptor : ComponentDescriptorBase<LayoutElement>
    {
        public override object CreateConverter(ComponentEditor editor)
        {
            object[] converters = new object[editor.Components.Length];
            Component[] components = editor.Components;
            for (int i = 0; i < components.Length; ++i)
            {
                LayoutElement layoutElement = (LayoutElement)components[i];

                converters[i] = new LayoutElementPropertyConverter
                {
                    LayoutElement = layoutElement
                };
            }
            return converters;
        }

        public override PropertyDescriptor[] GetProperties(ComponentEditor editor, object converter)
        {
            object[] converters = (object[])converter;

            MemberInfo ignoreLayoutInfo = Strong.MemberInfo((LayoutElement x) => x.ignoreLayout);
            MemberInfo minWidthInfo = Strong.MemberInfo((LayoutElement x) => x.minWidth);
            MemberInfo minHeightInfo = Strong.MemberInfo((LayoutElement x) => x.minHeight);
            MemberInfo preferredWidthConvertedInfo = Strong.MemberInfo((LayoutElementPropertyConverter x) => x.preferredWidth);
            MemberInfo preferredWidthInfo = Strong.MemberInfo((LayoutElement x) => x.preferredWidth);
            MemberInfo preferredHeightConvertedInfo = Strong.MemberInfo((LayoutElementPropertyConverter x) => x.preferredHeight);
            MemberInfo preferredHeightInfo = Strong.MemberInfo((LayoutElement x) => x.preferredHeight);
            MemberInfo flexibleWidthConvertedInfo = Strong.MemberInfo((LayoutElementPropertyConverter x) => x.flexibleWidth);
            MemberInfo flexibleWidthInfo = Strong.MemberInfo((LayoutElement x) => x.flexibleWidth);
            MemberInfo flexibleHeightConvertedInfo = Strong.MemberInfo((LayoutElementPropertyConverter x) => x.flexibleHeight);
            MemberInfo flexibleHeightInfo = Strong.MemberInfo((LayoutElement x) => x.flexibleHeight);
            MemberInfo layoutPriorityInfo = Strong.MemberInfo((LayoutElement x) => x.layoutPriority);

            ILocalization lc = IOC.Resolve<ILocalization>();

            List<PropertyDescriptor> properties = new List<PropertyDescriptor>();
            properties.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_LayoutElement_IgnoreLayout", "Ignore Layout"), editor.Components, ignoreLayoutInfo)
            {
                ValueChangedCallback = () =>
                {
                    editor.BuildEditor();
                }
            });

            if (editor.Components.OfType<LayoutElement>().All(le => !le.ignoreLayout))
            {
                properties.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_LayoutElement_MinWidth", "Min Width"), editor.Components, minWidthInfo)
                {
                    PropertyMetadata = new BoolFloat(-1, 0) } 
                );
                properties.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_LayoutElement_MinHeight", "Min Height"), editor.Components, minHeightInfo)
                { 
                    PropertyMetadata = new BoolFloat(-1, 0) 
                });
                properties.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_LayoutElement_PreferredWidth", "Preferred Width"), converters, preferredWidthConvertedInfo, preferredWidthInfo)
                {
                    PropertyMetadata = new BoolFloat(-1, 0) 
                });
                properties.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_LayoutElement_PreferredHeight", "Preferred Height"), converters, preferredHeightConvertedInfo,  preferredHeightInfo) 
                {
                    PropertyMetadata = new BoolFloat(-1, 0) 
                });
                properties.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_LayoutElement_FlexibleWidth", "Flexible Width"), converters, flexibleWidthConvertedInfo, flexibleWidthInfo) 
                { 
                    PropertyMetadata = new BoolFloat(-1, 0) 
                });
                properties.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_LayoutElement_FlexibleHeight", "Flexible Height"), converters, flexibleHeightConvertedInfo, flexibleHeightInfo) 
                { 
                    PropertyMetadata = new BoolFloat(-1, 0) 
                });
            }

            properties.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_LayoutElement_LayoutPriority", "Layout Priority"), editor.Components, layoutPriorityInfo));
            return properties.ToArray();
        }
    }
}


