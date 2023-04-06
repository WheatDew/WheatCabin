using UnityEngine;
using System.Reflection;
using Battlehub.Utils;
using Battlehub.RTCommon;

namespace Battlehub.RTEditor
{
    [BuiltInDescriptor]
    public class MeshFilterComponentDescriptor : ComponentDescriptorBase<MeshFilter>
    {
        public override PropertyDescriptor[] GetProperties(ComponentEditor editor, object converter)
        {
            ILocalization lc = IOC.Resolve<ILocalization>();

            MemberInfo sharedMeshInfo = Strong.PropertyInfo((MeshFilter x) => x.sharedMesh, "sharedMesh");
            return new[]
            {
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_MeshFilter_Mesh", "Mesh"), editor.Components, sharedMeshInfo, sharedMeshInfo)
            };
        }
    }
}

