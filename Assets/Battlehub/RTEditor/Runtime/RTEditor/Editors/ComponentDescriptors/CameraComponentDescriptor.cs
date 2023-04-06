using Battlehub.RTCommon;
using Battlehub.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Battlehub.RTEditor
{
    [BuiltInDescriptor]
    public class CameraComponentDescriptor : ComponentDescriptorBase<Camera>
    {
        public enum Projection
        {
            Perspective,
            Orthographic
        }

        public class CameraPropertyConverter
        {
            public Projection Projection
            {
                get
                {
                    if (Component == null) {return Projection.Perspective; }
                    return Component.orthographic ? Projection.Orthographic : Projection.Perspective;
                }
                set
                {
                    if (Component == null) { return; }
                    Component.orthographic = value == Projection.Orthographic;
                }
            }

            public Camera Component { get; set; }
        }

        public override object CreateConverter(ComponentEditor editor)
        {
            object[] converters = new object[editor.Components.Length];
            Component[] components = editor.Components;
            for(int i = 0; i < components.Length; ++i)
            {
                Camera camera = (Camera)components[i];
                if (camera != null)
                {
                    converters[i] = new CameraPropertyConverter
                    {
                        Component = camera
                    };
                }
            }
            return converters;
        }

        public override PropertyDescriptor[] GetProperties(ComponentEditor editor, object converter)
        {
            object[] converters = (object[])converter;

            ILocalization lc = IOC.Resolve<ILocalization>();
            
            PropertyEditorCallback valueChanged = () => editor.BuildEditor();
            MemberInfo projection = Strong.PropertyInfo((CameraPropertyConverter x) => x.Projection, "Projection");
            MemberInfo orthographic = Strong.PropertyInfo((Camera x) => x.orthographic, "orthographic");
            MemberInfo fov = Strong.PropertyInfo((Camera x) => x.fieldOfView, "fieldOfView");
            MemberInfo orthographicSize = Strong.PropertyInfo((Camera x) => x.orthographicSize, "orthographicSize");
            MemberInfo cullingMask = Strong.PropertyInfo((Camera x) => x.cullingMask, "cullingMask");

            List<PropertyDescriptor> descriptors = new List<PropertyDescriptor>();
            descriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Camera_Projection", "Projection"), converters, projection, orthographic, valueChanged));

            Camera[] cameras = editor.NotNullComponents.OfType<Camera>().ToArray();
            if(cameras.Length > 0)
            {
                bool isCameraOrthographic = cameras[0].orthographic;
                for(int i = 1; i < cameras.Length; ++i)
                {
                    if(cameras[i].orthographic != isCameraOrthographic)
                    {
                        return descriptors.ToArray();
                    }
                }

                if (!isCameraOrthographic)
                {
                    descriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Camera_Fov", "Field Of View"), editor.Components, fov, "field of view"));
                }
                else
                {
                    descriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Camera_Size", "Size"), editor.Components, orthographicSize, "orthographic size"));
                }

                List<RangeOptions.Option> flags = new List<RangeOptions.Option>();
                LayersInfo layersInfo = LayersEditor.LoadedLayers;
                foreach(LayersInfo.Layer layer in layersInfo.Layers)
                {
                    if(!string.IsNullOrEmpty(layer.Name))
                    {
                        flags.Add(new RangeOptions.Option(layer.Name, layer.Index));
                    }
                }


                descriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Camera_CullingMask", "Culling Mask"), editor.Components, cullingMask)
                {
                    PropertyMetadata = new RangeFlags(flags.ToArray())
                });
            }
            
            return descriptors.ToArray();
        }
    }

}

