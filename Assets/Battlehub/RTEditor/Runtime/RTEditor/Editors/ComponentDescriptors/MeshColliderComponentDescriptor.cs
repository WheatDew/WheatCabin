using UnityEngine;
using System.Reflection;
using Battlehub.Utils;
using Battlehub.RTCommon;
using System.Linq;

namespace Battlehub.RTEditor
{
    [BuiltInDescriptor]
    public class MeshColliderComponentDescriptor : ComponentDescriptorBase<MeshCollider>
    {
        private bool GetIsConvex(ComponentEditor editor, out bool? hasMixedValues)
        {
            hasMixedValues = null;
            bool isConvex = false;
            MeshCollider[] colliders = editor.NotNullComponents.OfType<MeshCollider>().ToArray();
            if (colliders.Length > 0)
            {
                hasMixedValues = false;
                isConvex = colliders[0].convex;
                for (int i = 1; i < colliders.Length; ++i)
                {
                    if (colliders[i].convex != isConvex)
                    {
                        hasMixedValues = true;
                        break;
                    }
                }
            }
            return isConvex;
        }

        public override PropertyDescriptor[] GetProperties(ComponentEditor editor, object converter)
        {
            ILocalization lc = IOC.Resolve<ILocalization>();

            PropertyEditorCallback valueChanged = () => editor.BuildEditor();

            MemberInfo convexInfo = Strong.PropertyInfo((MeshCollider x) => x.convex, "convex");
            MemberInfo isTriggerInfo = Strong.PropertyInfo((MeshCollider x) => x.isTrigger, "isTrigger");
            MemberInfo materialInfo = Strong.PropertyInfo((MeshCollider x) => x.sharedMaterial, "sharedMaterial");
            MemberInfo meshInfo = Strong.PropertyInfo((MeshCollider x) => x.sharedMesh, "sharedMesh");

            bool? hasMixedValues;
            bool isConvex = GetIsConvex(editor, out hasMixedValues);

            if(hasMixedValues != null)
            {
                if (isConvex && hasMixedValues == false)
                {
                    return new[]
                    {
                        new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_MeshCollider_Convex", "Convex"), editor.Components, convexInfo, convexInfo, valueChanged) { AnimationPropertyName = "m_Convex"},
                        new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_MeshCollider_IsTrigger", "Is Trigger"), editor.Components, isTriggerInfo, "m_IsTrigger"),
                        new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_MeshCollider_Material", "Material"), editor.Components, materialInfo, materialInfo),
                        new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_MeshCollider_Mesh", "Mesh"), editor.Components, meshInfo, meshInfo),
                    };
                }
                
                return new[]
                {
                    new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_MeshCollider_Convex", "Convex"), editor.Components, convexInfo, convexInfo, valueChanged)  { AnimationPropertyName = "m_Convex"},
                    new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_MeshCollider_Material", "Material"), editor.Components, materialInfo, materialInfo),
                    new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_MeshCollider_Mesh", "Mesh"), editor.Components, meshInfo, meshInfo),
                };
            }
            return new PropertyDescriptor[0];
        }
    }

}

