using System.Reflection;
using Battlehub.RTCommon;
using Battlehub.Utils;

namespace Battlehub.RTEditor
{
    [BuiltInDescriptor]
    public class RuntimeAnimationComponentDescriptor : ComponentDescriptorBase<RuntimeAnimation>
    {
        public override PropertyDescriptor[] GetProperties(ComponentEditor editor, object converter)
        {
            ILocalization lc = IOC.Resolve<ILocalization>();

            MemberInfo playOnAwakeInfo = Strong.PropertyInfo((RuntimeAnimation x) => x.PlayOnAwake, "PlayOnAwake");
            MemberInfo loopInfo = Strong.PropertyInfo((RuntimeAnimation x) => x.Loop, "Loop");
            MemberInfo clipsInfo = Strong.PropertyInfo((RuntimeAnimation x) => x.Clips, "Clips");

            return new[]
            {   
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_RuntimeAnimation_PlayOnAwake", "Play On Awake"), editor.Components, playOnAwakeInfo),
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_RuntimeAnimation_Loop", "Loop"), editor.Components, loopInfo),
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_RuntimeAnimation_Clips", "Clips"), editor.Components, clipsInfo)
            };
        }
    }
}

