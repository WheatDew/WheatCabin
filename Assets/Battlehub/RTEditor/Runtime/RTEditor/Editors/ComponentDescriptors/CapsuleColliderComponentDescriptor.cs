using UnityEngine;
using System.Reflection;
using Battlehub.Utils;
using Battlehub.RTGizmos;
using Battlehub.RTCommon;

namespace Battlehub.RTEditor
{
    [BuiltInDescriptor]
    public class CapsuleColliderPropertyConverter 
    {
        public enum CapsuleColliderDirection
        {
            X,
            Y,
            Z
        }

        public CapsuleColliderDirection Direction
        {
            get
            {
                if(Component == null)
                {
                    return CapsuleColliderDirection.X;
                }

                return (CapsuleColliderDirection)Component.direction;
            }
            set
            {
                if (Component == null)
                {
                    return;
                }
                Component.direction = (int)value;
            }
        }

        public CapsuleCollider Component
        {
            get;
            set;
        }
    }

    public class CapsuleColliderComponentDescriptor : ComponentDescriptorBase<CapsuleCollider, CapsuleColliderGizmo>
    {
        public override object CreateConverter(ComponentEditor editor)
        {
            object[] converters = new object[editor.Components.Length];
            Component[] components = editor.Components;
            for (int i = 0; i < components.Length; ++i)
            {
                CapsuleCollider collider = (CapsuleCollider)components[i];
                if (collider != null)
                {
                    converters[i] = new CapsuleColliderPropertyConverter
                    {
                        Component = collider
                    };
                }
            }
            return converters;
        }

        public override PropertyDescriptor[] GetProperties(ComponentEditor editor, object converter)
        {
            object[] converters = (object[])converter;

            ILocalization lc = IOC.Resolve<ILocalization>();

            MemberInfo isTriggerInfo = Strong.PropertyInfo((CapsuleCollider x) => x.isTrigger, "isTrigger");
            MemberInfo materialInfo = Strong.PropertyInfo((CapsuleCollider x) => x.sharedMaterial, "sharedMaterial");
            MemberInfo centerInfo = Strong.PropertyInfo((CapsuleCollider x) => x.center, "center");
            MemberInfo radiusInfo = Strong.PropertyInfo((CapsuleCollider x) => x.radius, "radius");
            MemberInfo heightInfo = Strong.PropertyInfo((CapsuleCollider x) => x.height, "height");
            MemberInfo directionInfo = Strong.PropertyInfo((CapsuleCollider x) => x.direction, "direction");
            MemberInfo directionConvertedInfo = Strong.PropertyInfo((CapsuleColliderPropertyConverter x) => x.Direction, "Direction");

            return new[]
            {
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_CapsuleCollider_IsTrigger", "Is Trigger"), editor.Components, isTriggerInfo, "m_IsTrigger"),
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_CapsuleCollider_Material", "Material"), editor.Components, materialInfo),
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_CapsuleCollider_Center", "Center"), editor.Components, centerInfo, "m_Center"),
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_CapsuleCollider_Radius", "Radius"), editor.Components, radiusInfo, "m_Radius"),
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_CapsuleCollider_Height", "Height"), editor.Components, heightInfo, "m_Height"),
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_CapsuleCollider_Direction", "Direction"), converters, directionConvertedInfo, "m_Direction"),
            };
        }
    }
}

