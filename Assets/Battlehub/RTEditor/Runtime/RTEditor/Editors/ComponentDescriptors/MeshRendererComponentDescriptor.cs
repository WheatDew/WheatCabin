#define SIMPLIFIED_MESHRENDERER

using UnityEngine;
using System.Reflection;

using Battlehub.Utils;
using Battlehub.RTCommon;

namespace Battlehub.RTEditor
{
    [BuiltInDescriptor]
#if SIMPLIFIED_MESHRENDERER

    public class MeshRendererComponentDescriptor : ComponentDescriptorBase<MeshRenderer>
    {
        public override PropertyDescriptor[] GetProperties(ComponentEditor editor, object converter)
        {
            ILocalization lc = IOC.Resolve<ILocalization>();

            MemberInfo shadowCastingMode = Strong.PropertyInfo((MeshRenderer x) => x.shadowCastingMode, "shadowCastingMode");
            MemberInfo receiveShadows = Strong.PropertyInfo((MeshRenderer x) => x.receiveShadows, "receiveShadows");
            MemberInfo materials = Strong.PropertyInfo((MeshRenderer x) => x.sharedMaterials, "sharedMaterials");
            
            return new[]
                {
                    new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_MeshRenderer_CastShadows", "Cast Shadows"), editor.Components, shadowCastingMode),
                    new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_MeshRenderer_ReceiveShadows", "Receive Shadows"), editor.Components, receiveShadows, "m_ReceiveShadows"),
                    new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_MeshRenderer_Materials", "Materials"), editor.Components, materials),
                };
        }
    }
#else
    public class MeshRendererComponentDescriptor : ComponentDescriptorBase<MeshRenderer>
    {
        public override PropertyDescriptor[] GetProperties(ComponentEditor editor, object converter)
        {
            ILocalization lc = IOC.Resolve<ILocalization>();

            MemberInfo shadowCastingMode = Strong.PropertyInfo((MeshRenderer x) => x.shadowCastingMode, "shadowCastingMode");
            MemberInfo receiveShadows = Strong.PropertyInfo((MeshRenderer x) => x.receiveShadows, "receiveShadows");
            MemberInfo materials = Strong.PropertyInfo((MeshRenderer x) => x.sharedMaterials, "sharedMaterials");
            MemberInfo lightProbes = Strong.PropertyInfo((MeshRenderer x) => x.lightProbeUsage, "lightProbeUsage");
            MemberInfo reflectionProbes = Strong.PropertyInfo((MeshRenderer x) => x.reflectionProbeUsage, "reflectionProbeUsage");
            MemberInfo anchorOverride = Strong.PropertyInfo((MeshRenderer x) => x.probeAnchor, "probeAnchor");

            return new[]
                {
                    new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_MeshRenderer_CastShadows", "Cast Shadows"), editor.Component, shadowCastingMode),
                    new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_MeshRenderer_ReceiveShadows", "Receive Shadows"), editor.Component, receiveShadows, "m_ReceiveShadows"),
                    new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_MeshRenderer_Materials", "Materials"), editor.Component, materials),
                    new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_MeshRenderer_LightProbes", "Light Probes"), editor.Component, lightProbes),
                    new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_MeshRenderer_ReflectionProbes", "Reflection Probes"), editor.Component, reflectionProbes),
                    new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_MeshRenderer_AnchorOverride", "Anchor Override"), editor.Component, anchorOverride),
                };
        }
    }
#endif
}