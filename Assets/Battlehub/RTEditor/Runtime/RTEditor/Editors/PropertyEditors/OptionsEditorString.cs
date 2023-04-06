using System;
using System.Reflection;

namespace Battlehub.RTEditor
{
    public class OptionsEditorString : OptionsEditor<string>
    {
        protected override void InitOverride(object[] target, object[] accessor, MemberInfo memberInfo, Action<object, object> eraseTargetCallback, string label = null)
        {
            base.InitOverride(target, accessor, memberInfo, eraseTargetCallback, label);
            CurrentValue = string.Empty;
        }
    }
}
