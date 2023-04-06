using Battlehub.RTCommon;
using Battlehub.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTEditor
{
    public class ImageProperyConverter
    {
        public Image Image
        {
            get;
            set;
        }

        public Texture2D SourceImage
        {
            get
            {
                Sprite sprite = Image.sprite;
                if(sprite == null)
                {
                    return null;
                }
                return sprite.texture;
            }
            set
            {
                if (value == null)
                {
                    Image.sprite = null;
                }
                else
                {
                    Image.sprite = Sprite.Create(value, new Rect(0, 0, value.width, value.height), Vector2.one * 0.5f);
                }
            }
        }
    }

    [BuiltInDescriptor]
    public class ImageComponentDescriptor : ComponentDescriptorBase<Image>
    {
        private Texture GetTexture(Sprite sprite)
        {
            if(sprite == null)
            {
                return null;
            }

            return sprite.texture;
        }
        public override object CreateConverter(ComponentEditor editor)
        {
            object[] converters = new object[editor.Components.Length];
            Component[] components = editor.Components;
            for (int i = 0; i < components.Length; ++i)
            {
                Image image = (Image)components[i];
                converters[i] = new ImageProperyConverter
                {
                    Image = image
                };
            }
            return converters;
        }

        public override PropertyDescriptor[] GetProperties(ComponentEditor editor, object converter)
        {
            ILocalization lc = IOC.Resolve<ILocalization>();

            object[] converters = (object[])converter;

            MemberInfo sourceImageInfoConverted = Strong.MemberInfo((ImageProperyConverter x) => x.SourceImage);
            MemberInfo sourceImageInfo = Strong.MemberInfo((Image x) => x.sprite);
            MemberInfo colorInfo = Strong.MemberInfo((Image x) => x.color);
            MemberInfo materialInfo = Strong.MemberInfo((Image x) => x.material);
            MemberInfo maskableInfo = Strong.MemberInfo((Image x) => x.maskable);
            MemberInfo imageTypeInfo = Strong.MemberInfo((Image x) => x.type);

            //Simple
            MemberInfo useSpriteMeshInfo = Strong.MemberInfo((Image x) => x.useSpriteMesh);

            //Simple, Filled
            MemberInfo preserveAspectInfo = Strong.MemberInfo((Image x) => x.preserveAspect);
            MethodInfo setNaviveSizeInfo = Strong.MethodInfo((Image x) => x.SetNativeSize());

            //Sliced, Tiled
            MemberInfo fillCenterInfo = Strong.MemberInfo((Image x) => x.fillCenter);
            MemberInfo pixelsPerUnityMultInfo = Strong.MemberInfo((Image x) => x.pixelsPerUnitMultiplier);

            //Filled
            MemberInfo fillMethodInfo = Strong.MemberInfo((Image x) => x.fillMethod);
            MemberInfo fillOriginInfo = Strong.MemberInfo((Image x) => x.fillOrigin);
            MemberInfo fillAmoutInfo = Strong.MemberInfo((Image x) => x.fillAmount);
            MemberInfo fillClockwiseInfo = Strong.MemberInfo((Image x) => x.fillClockwise);

            List<PropertyDescriptor> descriptors = new List<PropertyDescriptor>();
            descriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Image_SourceImage", "Source Image"), converters, sourceImageInfoConverted, sourceImageInfo)
            {
                ValueChangedCallback = () => editor.BuildEditor()
            });
            descriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Image_Color", "Color"), editor.Components, colorInfo));
            descriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Image_Material", "Material"), editor.Components, materialInfo));
            descriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Image_Maskable", "Maskable"), editor.Components, maskableInfo));
            
            Image[] images = editor.NotNullComponents.OfType<Image>().ToArray();
            Sprite firstSprite = images.Select(img => img.sprite).FirstOrDefault();
            if(GetTexture(firstSprite) != null && images.All(i => GetTexture(i.sprite) == GetTexture(firstSprite)))
            {
                descriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Image_ImageType", "Image Type"), editor.Components, imageTypeInfo) { ValueChangedCallback = () => editor.BuildEditor() });
                if (images.All(i => i.type == Image.Type.Sliced))
                {
                    descriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Image_FillCenter", "Fill Center"), editor.Components, fillCenterInfo));
                    descriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Image_PixelsPerUnitMultiplier", "Pixels Per Unity Multiplier"), editor.Components, pixelsPerUnityMultInfo));
                }
                else if (images.All(i => i.type == Image.Type.Simple))
                {
                    descriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Image_PreserveAspect", "Preserve Aspect"), editor.Components, preserveAspectInfo));
                    descriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Image_SetNativeSize", "Set Native Size"), editor.Components, setNaviveSizeInfo));
                }
                else if (images.All(i => i.type == Image.Type.Tiled))
                {
                    descriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Image_FillCenter", "Fill Center"), editor.Components, fillCenterInfo));
                    descriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Image_PixelsPerUnitMultiplier", "Pixels Per Unit Multiplier"), editor.Components, pixelsPerUnityMultInfo));
                }
                else if (images.All(i => i.type == Image.Type.Filled))
                {
                    descriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Image_FillMethod", "Fill Method"), editor.Components, fillMethodInfo)
                    {
                        ValueChangedCallback = () => editor.BuildEditor()
                    }); ;

                    bool showClockwiseProperty = true;
                    RangeOptions options = null;
                    if (images.All(i => i.fillMethod == Image.FillMethod.Radial180))
                    {
                        options = new RangeOptions(
                            new[]
                            {
                            new RangeOptions.Option(lc.GetString("ID_RTEditor_CD_Image_Bottom", "Bottom"), 0),
                            new RangeOptions.Option(lc.GetString("ID_RTEditor_CD_Image_Left", "Left"), 1),
                            new RangeOptions.Option(lc.GetString("ID_RTEditor_CD_Image_Top", "Top"), 2),
                            new RangeOptions.Option(lc.GetString("ID_RTEditor_CD_Image_Right", "Right"), 3),
                            }
                       );
                    }
                    else if (images.All(i => i.fillMethod == Image.FillMethod.Radial90))
                    {
                        options = new RangeOptions(
                            new[]
                            {
                            new RangeOptions.Option(lc.GetString("ID_RTEditor_CD_Image_BottomLeft", "Bottom Left"), 0),
                            new RangeOptions.Option(lc.GetString("ID_RTEditor_CD_Image_TopLeft", "Top Left"), 1),
                            new RangeOptions.Option(lc.GetString("ID_RTEditor_CD_Image_TopRight", "Top Right"), 2),
                            new RangeOptions.Option(lc.GetString("ID_RTEditor_CD_Image_BottomRight", "Bottom Right"), 3),
                            }
                        );
                    }
                    else if (images.All(i => i.fillMethod == Image.FillMethod.Radial360))
                    {
                        options = new RangeOptions(
                            new[]
                            {
                            new RangeOptions.Option(lc.GetString("ID_RTEditor_CD_Image_Bottom", "Bottom"), 0),
                            new RangeOptions.Option(lc.GetString("ID_RTEditor_CD_Image_Right", "Right"), 1),
                            new RangeOptions.Option(lc.GetString("ID_RTEditor_CD_Image_Top", "Top"), 2),
                            new RangeOptions.Option(lc.GetString("ID_RTEditor_CD_Image_Left", "Left"), 3),
                            }
                        );
                    }
                    else if (images.All(i => i.fillMethod == Image.FillMethod.Horizontal))
                    {
                        showClockwiseProperty = false;
                        options = new RangeOptions(
                            new[]
                            {
                            new RangeOptions.Option(lc.GetString("ID_RTEditor_CD_Image_Left", "Left"), 0),
                            new RangeOptions.Option(lc.GetString("ID_RTEditor_CD_Image_Right", "Right"), 1),
                            }
                        );
                    }
                    else if (images.All(i => i.fillMethod == Image.FillMethod.Vertical))
                    {
                        showClockwiseProperty = false;
                        options = new RangeOptions(
                            new[]
                            {
                            new RangeOptions.Option(lc.GetString("ID_RTEditor_CD_Image_Bottom", "Bottom"), 0),
                            new RangeOptions.Option(lc.GetString("ID_RTEditor_CD_Image_Top", "Top"), 1),

                            }
                        );
                    }

                    if (options != null)
                    {
                        descriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Image_FillOrigin", "Fill Origin"), editor.Components, fillOriginInfo)
                        {
                            PropertyMetadata = options
                        });
                    }

                    descriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Image_FillAmount", "Fill Amount"), editor.Components, fillAmoutInfo)
                    {
                        PropertyMetadata = new Range(0, 1)
                    });

                    if(showClockwiseProperty)
                    {
                        descriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Image_Clockwise", "Clockwise"), editor.Components, fillClockwiseInfo));
                    }

                    descriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Image_PreserveAspect", "Preserve Aspect"), editor.Components, preserveAspectInfo));
                    descriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Image_SetNativeSize", "Set Native Size"), editor.Components, setNaviveSizeInfo));
                }
            }

            return descriptors.ToArray();
        }
    }
}

