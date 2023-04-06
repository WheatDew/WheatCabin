//#define RTSL_COMPILE_TEMPLATES
#if RTSL_COMPILE_TEMPLATES
//<TEMPLATE_USINGS_START>
using Battlehub.RTCommon;
using Battlehub.Utils;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Battlehub.SL2;
//<TEMPLATE_USINGS_END>
#else
using UnityEngine;
#endif

namespace Battlehub.RTSL.Internal
{
    [PersistentTemplate("UnityEngine.Avatar",
        new string[0],
        new[] { "UnityEngine.HumanDescription" })]
    public class PersistentAvatar_RTSL_Template : PersistentSurrogateTemplate
    {
        public class PersistentHumanDescription<T> : PersistentSurrogateTemplate { }

#if RTSL_COMPILE_TEMPLATES
        //<TEMPLATE_BODY_START>
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

        //<TEMPLATE_BODY_END>
#endif
    }
}


