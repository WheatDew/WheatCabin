using Battlehub.RTCommon;
using Battlehub.Utils;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTEditor
{
    public class LayoutGroupPadding
    {
        public int Left;
        public int Right;
        public int Top;
        public int Bottom;
    }

    public class LayoutGroupPropertyConverter
    {
        private LayoutGroupPadding m_padding = new LayoutGroupPadding();
        public LayoutGroupPadding Padding
        {
            get
            {
                RectOffset padding = LayoutElement.padding;
                m_padding.Left = padding.left;
                m_padding.Right = padding.right;
                m_padding.Top= padding.top;
                m_padding.Bottom = padding.bottom;
                return m_padding;
            }
            set 
            {
                m_padding = value != null ? value : new LayoutGroupPadding();
                LayoutElement.padding = new RectOffset(m_padding.Left, m_padding.Right, m_padding.Top, m_padding.Bottom);
            }
        }

        public LayoutGroup LayoutElement
        {
            get;
            set;
        }
    }

    public abstract class LayoutGroupComponentDescriptor<TComponent> : ComponentDescriptorBase<TComponent>
    {
        public override object CreateConverter(ComponentEditor editor)
        {
            object[] converters = new object[editor.Components.Length];
            Component[] components = editor.Components;
            for (int i = 0; i < components.Length; ++i)
            {
                LayoutGroup layoutGroup = (LayoutGroup)components[i];

                converters[i] = new LayoutGroupPropertyConverter
                {
                    LayoutElement = layoutGroup
                };
            }
            return converters;
        }

        protected virtual void BeforeBaseClassProperties(ComponentEditor editor, object converter, List<PropertyDescriptor> properties)
        {
        }

        public override PropertyDescriptor[] GetProperties(ComponentEditor editor, object converter)
        {
            object[] converters = (object[])converter;

            MemberInfo paddingInfo = Strong.MemberInfo((LayoutGroup x) => x.padding);
            MemberInfo paddingInfoConverted = Strong.MemberInfo((LayoutGroupPropertyConverter x) => x.Padding);
            MemberInfo leftInfo = Strong.MemberInfo((LayoutGroupPadding x) => x.Left);
            MemberInfo rightInfo = Strong.MemberInfo((LayoutGroupPadding x) => x.Right);
            MemberInfo topInfo = Strong.MemberInfo((LayoutGroupPadding x) => x.Top);
            MemberInfo bottomInfo = Strong.MemberInfo((LayoutGroupPadding x) => x.Bottom);

            List<PropertyDescriptor> properties = new List<PropertyDescriptor>();
            BeforeBaseClassProperties(editor, converter, properties);

            ILocalization lc = IOC.Resolve<ILocalization>();
            properties.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_LayoutGroup_Padding", "Padding"), converters, paddingInfoConverted, paddingInfo)
            {
                ChildDesciptors = new[]
                {
                    new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_LayoutGroup_Padding_Left", "Left"), null, leftInfo),
                    new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_LayoutGroup_Padding_Right", "Right"), null, rightInfo),
                    new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_LayoutGroup_Padding_Top", "Top"), null, topInfo),
                    new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_LayoutGroup_Padding_Bottom", "Bottom"), null, bottomInfo),
                }
            });

            AfterBaseClassProperties(editor, converter, properties);
            return properties.ToArray();
        }

        protected virtual void AfterBaseClassProperties(ComponentEditor editor, object converter, List<PropertyDescriptor> properties)
        {
        }
    }

}
