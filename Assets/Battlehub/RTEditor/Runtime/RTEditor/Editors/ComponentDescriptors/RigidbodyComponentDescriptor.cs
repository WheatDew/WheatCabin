using UnityEngine;
using System.Reflection;
using Battlehub.Utils;
using Battlehub.RTCommon;

namespace Battlehub.RTEditor
{
    [BuiltInDescriptor]
    public class RigidbodyComponentDescriptor : ComponentDescriptorBase<Rigidbody>
    {
        public override PropertyDescriptor[] GetProperties(ComponentEditor editor, object converter)
        {
            ILocalization lc = IOC.Resolve<ILocalization>();

            MemberInfo massInfo = Strong.PropertyInfo((Rigidbody x) => x.mass, "mass");
            MemberInfo dragInfo = Strong.PropertyInfo((Rigidbody x) => x.drag, "drag");
            MemberInfo angularDragInfo = Strong.PropertyInfo((Rigidbody x) => x.angularDrag, "angularDrag");
            MemberInfo useGravityInfo = Strong.PropertyInfo((Rigidbody x) => x.useGravity, "useGravity");
            MemberInfo isKinematicInfo = Strong.PropertyInfo((Rigidbody x) => x.isKinematic, "isKinematic");
            MemberInfo interpolationInfo = Strong.PropertyInfo((Rigidbody x) => x.interpolation, "interpolation");
            MemberInfo collisionDetectionInfo = Strong.PropertyInfo((Rigidbody x) => x.collisionDetectionMode, "collisionDetectionMode");

            return new[]
            {
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Rigidbody_Mass", "Mass"), editor.Components, massInfo, "m_Mass"),
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Rigidbody_Drag", "Drag"), editor.Components, dragInfo, "m_Drag"),
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Rigidbody_AngularDrag", "Angular Drag"), editor.Components, angularDragInfo, "m_AngularDrag"),
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Rigidbody_UseGravity", "Use Gravity"), editor.Components, useGravityInfo, "m_UseGravity"),
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Rigidbody_IsKinematic", "Is Kinematic"), editor.Components, isKinematicInfo, "m_IsKinematic"),
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Rigidbody_Interpolation", "Interpolation"), editor.Components, interpolationInfo, interpolationInfo),
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Rigidbody_CollisionDetection", "Collision Detection"), editor.Components, collisionDetectionInfo, collisionDetectionInfo)
            };
        }
    }

}

