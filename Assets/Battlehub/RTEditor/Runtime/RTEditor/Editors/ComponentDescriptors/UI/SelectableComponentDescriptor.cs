using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine.UI;
using Battlehub.Utils;
using Battlehub.RTCommon;

namespace Battlehub.RTEditor
{
    [BuiltInDescriptor]
    public class SelectableComponentDescriptor : SelectableComponentDescriptor<Selectable>
    {
    }

    public abstract class SelectableComponentDescriptor<TComponent> : ComponentDescriptorBase<TComponent>
    {
        protected virtual void BeforeBaseClassProperties(ComponentEditor editor, object converter, List<PropertyDescriptor> properties)
        {
        }

        public override PropertyDescriptor[] GetProperties(ComponentEditor editor, object converter)
        {
            ILocalization lc = IOC.Resolve<ILocalization>();

            List<PropertyDescriptor> properties = new List<PropertyDescriptor>();
            BeforeBaseClassProperties(editor, converter, properties);

            MemberInfo interactableInfo = Strong.MemberInfo((Selectable x) => x.interactable);
            MemberInfo transitionInfo = Strong.MemberInfo((Selectable x) => x.transition);
            MemberInfo targetGraphicInfo = Strong.MemberInfo((Selectable x) => x.targetGraphic);
            MemberInfo colorInfo = Strong.MemberInfo((Selectable x) => x.colors);
            MemberInfo spriteStateInfo = Strong.MemberInfo((Selectable x) => x.spriteState);
            MemberInfo navigationInfo = Strong.MemberInfo((Selectable x) => x.navigation);

            List<PropertyDescriptor> navigationDescriptors = new List<PropertyDescriptor>();
            PropertyDescriptor modeDescriptor = new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Selectable_Mode", "Mode"), null, Strong.MemberInfo((Navigation x) => x.mode));
            navigationDescriptors.Add(modeDescriptor);

            Selectable[] selectables = editor.NotNullComponents.OfType<Selectable>().ToArray();
            if (selectables.All(s => s.navigation.mode == Navigation.Mode.Explicit))
            {
                navigationDescriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Selectable_SelectOnUp", "Select On Up"), null, Strong.MemberInfo((Navigation x) => x.selectOnUp)));
                navigationDescriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Selectable_SelectOnDown",  "Select On Down"), null, Strong.MemberInfo((Navigation x) => x.selectOnDown)));
                navigationDescriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Selectable_SelectOnLeft", "Select On Left"), null, Strong.MemberInfo((Navigation x) => x.selectOnLeft)));
                navigationDescriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Selectable_SelectOnRight", "Select On Right"), null, Strong.MemberInfo((Navigation x) => x.selectOnRight)));
            }

            properties.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Selectable_Interactable", "Interactable"), editor.Components, interactableInfo));
            properties.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Selectable_TargetGraphic","Target Graphic"), editor.Components, targetGraphicInfo));

            if (selectables.All(s => s.transition == Selectable.Transition.ColorTint))
            {
                properties.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Selectable_Colors", "Colors"), editor.Components, colorInfo)
                {
                    ChildDesciptors = new[]
                    {
                        new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Selectable_NormalColor", "Normal Color"), null, Strong.MemberInfo((ColorBlock x) => x.normalColor)),
                        new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Selectable_HighlightedColor", "Highlighted Color"), null, Strong.MemberInfo((ColorBlock x) => x.highlightedColor)),
                        new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Selectable_PressedColor", "Pressed Color"), null, Strong.MemberInfo((ColorBlock x) => x.pressedColor)),
                        new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Selectable_SelectedColor", "Selected Color"), null, Strong.MemberInfo((ColorBlock x) => x.selectedColor)),
                        new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Selectable_DisabledColor", "Disabled Color"), null, Strong.MemberInfo((ColorBlock x) => x.disabledColor)),
                        new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Selectable_ColorMultiplier", "Color Multiplier"), null, Strong.MemberInfo((ColorBlock x) => x.colorMultiplier))
                        {
                            PropertyMetadata = new Range(0, 1)
                        },
                        new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Selectable_FadeDuration", "Fade Duration"), null, Strong.MemberInfo((ColorBlock x) => x.fadeDuration)),
                    }
                }); ;
            }
        
            properties.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Selectable_Navigation", "Navigation"), editor.Components, navigationInfo)
            {
                ChildDesciptors = navigationDescriptors.ToArray(),
                ValueChangedCallback = () => editor.BuildEditor()
            });

            AfterBaseClassProperties(editor, converter, properties);
            return properties.ToArray();
        }

        protected virtual void AfterBaseClassProperties(ComponentEditor editor, object converter, List<PropertyDescriptor> properties)
        {
        }
    }

    public abstract class SelectableComponentDescriptor<TComponent, TGizmo> : SelectableComponentDescriptor<TComponent>
    {
        public override Type GizmoType
        {
            get { return typeof(TGizmo); }
        }
    }

}
