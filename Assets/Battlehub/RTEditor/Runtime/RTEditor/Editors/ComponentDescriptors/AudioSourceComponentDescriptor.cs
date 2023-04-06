using UnityEngine;
using Battlehub.Utils;
using System.Reflection;
using Battlehub.RTCommon;
using Battlehub.RTGizmos;

namespace Battlehub.RTEditor
{
    [BuiltInDescriptor]
    public class AudioSourceComponentDescriptor : ComponentDescriptorBase<AudioSource, AudioSourceGizmo>
    {
        public override PropertyDescriptor[] GetProperties(ComponentEditor editor, object converter)
        {
            ILocalization lc = IOC.Resolve<ILocalization>();

            MemberInfo clipInfo = Strong.PropertyInfo((AudioSource x) => x.clip, "clip");
            MemberInfo volumeInfo = Strong.PropertyInfo((AudioSource x) => x.volume, "volume");
            MemberInfo loopInfo = Strong.PropertyInfo((AudioSource x) => x.loop, "loop");
            MemberInfo playOnAwake = Strong.PropertyInfo((AudioSource x) => x.playOnAwake, "playOnAwake");

            return new[]
            {
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_AudioSource_Loop", "Loop"), editor.Components, loopInfo),
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_AudioSource_PlayOnAwake", "Play On Awake"), editor.Components, playOnAwake),
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_AudioSource_Clip", "Clip"), editor.Components, clipInfo),
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_AudioSource_Volume", "Volume"), editor.Components, volumeInfo, volumeInfo,
                    null, new Range(0.0f, 1.0f)) { AnimationPropertyName = "m_Volume" },
            };
        }
    }
}
