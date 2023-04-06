using UnityEngine;
using System.Reflection;
using Battlehub.Utils;
using Battlehub.RTCommon;

namespace Battlehub.RTEditor
{
    [BuiltInDescriptor]
    public class TransformComponentDescriptor : ComponentDescriptorBase<Transform>
    {
        public override object CreateConverter(ComponentEditor editor)
        {
            object[] converters = new object[editor.Components.Length];
            Component[] components = editor.Components;
            for (int i = 0; i < components.Length; ++i)
            {
                Transform transform = (Transform)components[i];
                if (transform != null)
                {
                    converters[i] = new TransformPropertyConverter
                    {
                        ExposeToEditor = transform.GetComponent<ExposeToEditor>(),
                        Transform = transform
                    };
                }
            }
            return converters;
        }

        public override PropertyDescriptor[] GetProperties(ComponentEditor editor, object converter)
        {
            object[] converters = (object[])converter;

            ILocalization lc = IOC.Resolve<ILocalization>();

            MemberInfo position = Strong.PropertyInfo((Transform x) => x.localPosition, "localPosition");
            MemberInfo positionConverted = Strong.PropertyInfo((TransformPropertyConverter x) => x.LocalPosition, "LocalPosition");
            MemberInfo rotation = Strong.PropertyInfo((Transform x) => x.localRotation, "localRotation");
            MemberInfo rotationConverted = Strong.PropertyInfo((TransformPropertyConverter x) => x.LocalEuler, "LocalEulerAngles");
            MemberInfo scale = Strong.PropertyInfo((Transform x) => x.localScale, "localScale");
            MemberInfo scaleConverted = Strong.PropertyInfo((TransformPropertyConverter x) => x.LocalScale, "LocalScale");

            return new[]
                {
                    new PropertyDescriptor( lc.GetString("ID_RTEditor_CD_Transform_Position", "Position"),converters, positionConverted, position, true) ,
                    new PropertyDescriptor( lc.GetString("ID_RTEditor_CD_Transform_Rotation", "Rotation"), converters, rotationConverted, rotation),
                    new PropertyDescriptor( lc.GetString("ID_RTEditor_CD_Transform_Scale", "Scale"), converters, scaleConverted, scale)
                };
        }
    }

    public class TransformPropertyConverter
    {
        //private ISettingsComponent m_settingsComponent = IOC.Resolve<ISettingsComponent>();

        public Vector3 LocalPosition
        {
            get
            {
                if (ExposeToEditor == null)
                {
                    return Transform.localPosition;
                }

                return ExposeToEditor.LocalPosition;
            }
            set
            {
                if (ExposeToEditor == null)
                {
                    Transform.localPosition = value;
                    return;
                }
                ExposeToEditor.LocalPosition = value;
            }
        }

        public Vector3 LocalEuler
        {
            get
            {
                if (ExposeToEditor == null)
                {
                    return Transform.localEulerAngles;
                }

                return ExposeToEditor.LocalEuler;
            }
            set
            {
                if (ExposeToEditor == null)
                {
                    Transform.localEulerAngles = value;
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
                    return Transform.localScale;
                }

                return ExposeToEditor.LocalScale;
            }
            set
            {
                if (ExposeToEditor == null)
                {
                    Transform.localScale = value;
                    return;
                }
                ExposeToEditor.LocalScale = value;
            }
        }

        public ExposeToEditor ExposeToEditor
        {
            get;
            set;
        }

        public Transform Transform
        {
            get;
            set;
        }
    }
}

