using Battlehub.RTCommon;
using Battlehub.Utils;
using System;
using System.Reflection;
using UnityEngine;

namespace Battlehub.RTEditor
{
    [BuiltInDescriptor]
    public class HingeJointComponentDescriptor : ComponentDescriptorBase<HingeJoint>
    {
        public override PropertyDescriptor[] GetProperties(ComponentEditor editor, object converter)
        {
            ILocalization lc = IOC.Resolve<ILocalization>();

            MemberInfo connectedBodyInfo = Strong.PropertyInfo((HingeJoint x) => x.connectedBody, "connectedBody");
            MemberInfo anchorInfo = Strong.PropertyInfo((HingeJoint x) => x.anchor, "anchor");
            MemberInfo axisInfo = Strong.PropertyInfo((HingeJoint x) => x.axis, "axis");
            MemberInfo autoConfigAnchorInfo = Strong.PropertyInfo((HingeJoint x) => x.autoConfigureConnectedAnchor, "autoConfigureConnectedAnchor");
            MemberInfo connectedAnchorInfo = Strong.PropertyInfo((HingeJoint x) => x.connectedAnchor, "connectedAnchor");
            MemberInfo useSpringInfo = Strong.PropertyInfo((HingeJoint x) => x.useSpring, "useSpring");
            MemberInfo springInfo = Strong.PropertyInfo((HingeJoint x) => x.spring, "spring");
            MemberInfo useMotorInfo = Strong.PropertyInfo((HingeJoint x) => x.useMotor, "useMotor");
            MemberInfo motorInfo = Strong.PropertyInfo((HingeJoint x) => x.motor, "motor");
            MemberInfo useLimitsInfo = Strong.PropertyInfo((HingeJoint x) => x.useLimits, "useLimits");
            MemberInfo limitsInfo = Strong.PropertyInfo((HingeJoint x) => x.limits, "limits");
            MemberInfo breakForceInfo = Strong.PropertyInfo((HingeJoint x) => x.breakForce, "breakForce");
            MemberInfo breakTorqueInfo = Strong.PropertyInfo((HingeJoint x) => x.breakTorque, "breakTorque");
            MemberInfo enableCollisionInfo = Strong.PropertyInfo((HingeJoint x) => x.enableCollision, "enableCollision");
            MemberInfo enablePreporcessingInfo = Strong.PropertyInfo((HingeJoint x) => x.enablePreprocessing, "enablePreprocessing");

            return new[]
            {
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_HingeJoint_ConnectedBody", "Connected Body"), editor.Components, connectedBodyInfo),
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_HingeJoint_Anchor", "Anchor"), editor.Components, anchorInfo, "m_Anchor"),
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_HingeJoint_Axis", "Axis"), editor.Components, axisInfo, "m_Axis"),
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_HingeJoint_AutoConfigure", "Auto Configure Connected Anchor"), editor.Components, autoConfigAnchorInfo, "m_AutoConfigureConnectedAnchor"),
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_HingeJoint_ConnectedAnchor", "Connected Anchor"), editor.Components, connectedAnchorInfo, "m_ConnectedAnchor"),
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_HingeJoint_UseSpring", "Use Spring"), editor.Components, useSpringInfo, "m_UseSpring"),
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_HingeJoint_Spring", "Spring"), editor.Components, springInfo),
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_HingeJoint_UseMotor", "Use Motor"), editor.Components, useMotorInfo, "m_UseMotor"),
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_HingeJoint_Motor", "Motor"), editor.Components, motorInfo),
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_HingeJoint_UseLimits", "Use Limits"), editor.Components, useLimitsInfo, "m_UseLimits"),
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_HingeJoint_Limits", "Limits"), editor.Components, limitsInfo)
                {
                    ChildDesciptors = new[]
                    {
                        new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_HingeJoint_Min", "Min"), null, Strong.PropertyInfo((JointLimits x) => x.min, "min")),
                        new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_HingeJoint_Max", "Max"), null, Strong.PropertyInfo((JointLimits x) => x.max, "max")),
                        new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_HingeJoint_Bounciness", "Bounciness"), null, Strong.PropertyInfo((JointLimits x) => x.bounciness, "bounciness")),
                        new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_HingeJoint_BounceMinVelocity", "Bounce Min Velocity"), null, Strong.PropertyInfo((JointLimits x) => x.bounceMinVelocity, "bounceMinVelocity")),
                        new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_HingeJoint_ContactDistance", "Contact Distance"), null, Strong.PropertyInfo((JointLimits x) => x.contactDistance, "contactDistance")),
                    }
                },
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_HingeJoint_BreakForce", "Break Force"), editor.Components, breakForceInfo, "m_BreakForce"),
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_HingeJoint_BreakTorque", "Break Torque"), editor.Components, breakTorqueInfo, "m_BreakTorque"),
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_HingeJoint_BreakEnableCollision", "Enable Collision"), editor.Components, enableCollisionInfo, "m_EnableCollision"),
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_HingeJoint_BreakEnablePreprocessing", "Enable Preprocessing"), editor.Components, enablePreporcessingInfo, "m_EnablePreprocessing"),
            };            
        }
    }
}
