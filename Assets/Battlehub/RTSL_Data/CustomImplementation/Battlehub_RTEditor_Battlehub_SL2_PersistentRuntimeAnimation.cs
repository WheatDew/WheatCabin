#if !RTSL_MAINTENANCE
using Battlehub.RTSL;
using ProtoBuf;
using Battlehub.RTEditor;
using System.Collections.Generic;
using System.Linq;

namespace Battlehub.RTEditor.Battlehub.SL2
{
    [CustomImplementation]
    public partial class PersistentRuntimeAnimation<TID> 
    {        

        [ProtoMember(1)]
        public TID[] Clips;

        [ProtoMember(2)]
        public int ClipIndex;

        [ProtoMember(3)]
        public bool PlayOnAwake;

        [ProtoMember(4)]
        public bool Loop;

        public override void ReadFrom(object obj)
        {
            base.ReadFrom(obj);

            RuntimeAnimation animation = (RuntimeAnimation)obj;
            if (animation == null)
            {
                return;
            }

            IList<RuntimeAnimationClip> clips = animation.Clips;
            if(clips != null)
            {
                Clips = ToID(clips.ToArray());
            }

            ClipIndex = animation.ClipIndex;
            PlayOnAwake = animation.PlayOnAwake;
            Loop = animation.Loop;
        }

        public override object WriteTo(object obj)
        {
            obj = base.WriteTo(obj);
            RuntimeAnimation animation = (RuntimeAnimation)obj;
            if (animation == null)
            {
                return null;
            }

            animation.PlayOnAwake = PlayOnAwake;
            animation.Loop = Loop;

            IList<RuntimeAnimationClip> clips = FromID<RuntimeAnimationClip>(Clips, animation.Clips.ToArray());
            animation.SetClips(clips, ClipIndex);
            return animation;
        }

        public override void GetDeps(GetDepsContext<TID> context)
        {
            base.GetDeps(context);
            AddDep(Clips, context);
        }

        public override void GetDepsFrom(object obj, GetDepsFromContext context)
        {
            base.GetDepsFrom(obj, context);
            RuntimeAnimation o = (RuntimeAnimation)obj;
            if (obj == null)
            {
                return;
            }

            AddDep(o.Clips, context);
        }
    }
}
#endif

