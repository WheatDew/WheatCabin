//#define RTSL_COMPILE_TEMPLATES
#if RTSL_COMPILE_TEMPLATES
//<TEMPLATE_USINGS_START>
using Battlehub.RTCommon;
using Battlehub.Utils;
using ProtoBuf;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Battlehub.SL2;
//<TEMPLATE_USINGS_END>
#else
using UnityEngine;
#endif

namespace Battlehub.RTSL.Internal
{
    [PersistentTemplate("UnityEngine.Animator",
        new[] { "avatar"},
        new[] { "UnityEngine.Avatar"})]
    public class PersistentAnimator_RTSL_Template : PersistentSurrogateTemplate
    {
         public class PersistentAvatar<T> : PersistentSurrogateTemplate { }

#if RTSL_COMPILE_TEMPLATES
        //<TEMPLATE_BODY_START>

        [ProtoMember(272)]  //Moved from auto-generated code. 
        public TID avatar; //refrence to avatar from asset library

        [ProtoMember(1)]    //data of avatar created at runtime
        public PersistentAvatar<TID> animatorAvatar;

        public override object WriteTo(object obj)
        {
            obj = base.WriteTo(obj);
            if (obj == null)
            {
                return null;
            }

            Animator uo = (Animator)obj;
            if (!m_assetDB.IsMapped(avatar))
            {
                animatorAvatar.WriteTo(uo); //NOTE: This is not mistake AvatarBuilder requires refrence to game object root.
            }
            else
            {
                uo.avatar = FromID<Avatar>(avatar);
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

            Animator uo = (Animator)obj;
            avatar = ToID(uo.avatar);
            if (!m_assetDB.IsMapped(avatar))
            {
                animatorAvatar = new PersistentAvatar<TID>();
                animatorAvatar.ReadFrom(uo.avatar);
            }
        }

        public override void GetDeps(GetDepsContext<TID> context)
        {
            base.GetDeps(context);

            if (animatorAvatar == null)
            {
                AddDep(avatar, context);
            }
        }

        public override void GetDepsFrom(object obj, GetDepsFromContext context)
        {
            base.GetDepsFrom(obj, context);
            if (obj == null)
            {
                return;
            }

            Animator uo = (Animator)obj;
            if (m_assetDB.IsMapped(uo.avatar))
            {
                AddDep(uo.avatar, context);
            }
        }
        //<TEMPLATE_BODY_END>
#endif
    }
}


