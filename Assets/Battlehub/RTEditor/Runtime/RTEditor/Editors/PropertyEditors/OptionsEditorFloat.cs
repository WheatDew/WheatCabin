using System;
using System.Reflection;
using UnityEngine;

namespace Battlehub.RTEditor
{
    public class OptionsEditorFloat : OptionsEditor<float>
    {
        protected override void InitOverride(object[] target, object[] accessor, MemberInfo memberInfo, Action<object, object> eraseTargetCallback, string label = null)
        {
            base.InitOverride(target, accessor, memberInfo, eraseTargetCallback, label);
            CurrentValue = -1;
        }
    }
}
