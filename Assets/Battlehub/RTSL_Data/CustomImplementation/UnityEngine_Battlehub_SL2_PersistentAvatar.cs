#if !RTSL_MAINTENANCE
using Battlehub.RTSL;
using Battlehub.RTCommon;
using Battlehub.Utils;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Battlehub.SL2;

namespace UnityEngine.Battlehub.SL2
{
    [CustomImplementation]
    public partial class PersistentAvatar<TID> 
    {        
        [ProtoMember(1)]
        public bool isHuman;

        [ProtoMember(2)]
        public PersistentHumanDescription<TID> humanDescription;

        public override object WriteTo(object obj)
        {
            if(obj is Avatar)
            {
                obj = base.WriteTo(obj);
                if (obj == null)
                {
                    return null;
                }
                return obj;
            }

            Animator animator = (Animator)obj;
            if(isHuman)
            {
                HumanDescription desc = (HumanDescription)humanDescription.WriteTo(new HumanDescription());
                desc.skeleton[0].name = animator.gameObject.name;
                animator.avatar = AvatarBuilder.BuildHumanAvatar(animator.gameObject, desc);
                base.WriteTo(animator.avatar);
            }
            else
            {
                animator.avatar = AvatarBuilder.BuildGenericAvatar(animator.gameObject, "");
            }

            return obj;
        }

        public override void ReadFrom(object obj)
        {
            base.ReadFrom(obj);
            if (obj == null)
            {
                return;
            }

            Avatar uo = (Avatar)obj;
            
            isHuman = uo.isHuman;
            humanDescription = new PersistentHumanDescription<TID>();
            humanDescription.ReadFrom(uo.humanDescription);
        }
    }
}
#endif

