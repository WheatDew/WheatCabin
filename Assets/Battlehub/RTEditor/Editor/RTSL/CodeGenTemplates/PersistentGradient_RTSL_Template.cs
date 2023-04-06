//#define RTSL_COMPILE_TEMPLATES
#if RTSL_COMPILE_TEMPLATES
//<TEMPLATE_USINGS_START>
using Battlehub.RTCommon;
using ProtoBuf;
using System;
using System.Collections.Generic;
using UnityEngine;
//<TEMPLATE_USINGS_END>
#else
using UnityEngine;
#endif

namespace Battlehub.RTSL.Internal
{
    [PersistentTemplate("UnityEngine.Gradient", new[] { "colorKeys", "alphaKeys" }, 
        new[] { "UnityEngine.GradientAlphaKey", "UnityEngine.GradientColorKey" } )]
    public class PersistentGradient_RTSL_Template : PersistentSurrogateTemplate
    {
        public class PersistentGradientColorKey<T> : PersistentSurrogateTemplate { }
        public class PersistentGradientAlphaKey<T> : PersistentSurrogateTemplate { }


#if RTSL_COMPILE_TEMPLATES
        //<TEMPLATE_BODY_START>

        [ProtoMember(1, IsRequired = true)]
        public PersistentGradientColorKey<TID>[] colorKeys;

        [ProtoMember(2, IsRequired = true)]
        public PersistentGradientAlphaKey<TID>[] alphaKeys;

        public override void ReadFrom(object obj)
        {
            base.ReadFrom(obj);
            if(obj == null)
            {
                return;
            }
            Gradient uo = (Gradient)obj;
            colorKeys = Assign(uo.colorKeys, v_ => (PersistentGradientColorKey<TID>)v_);
            alphaKeys = Assign(uo.alphaKeys, v_ => (PersistentGradientAlphaKey<TID>)v_);   
        }

        public override object WriteTo(object obj)
        {
            obj = base.WriteTo(obj);
            if(obj == null)
            {
                return obj;
            }
            Gradient uo = (Gradient)obj;
            if(colorKeys != null)
            {
                uo.colorKeys = Assign(colorKeys, v_ => (GradientColorKey)v_);
            }
            if(alphaKeys != null)
            {
                uo.alphaKeys = Assign(alphaKeys, v_ => (GradientAlphaKey)v_);
            }
            return uo;
        }

        public override void GetDeps(GetDepsContext<TID> context)
        {
            base.GetDeps(context);
        }

        public override void GetDepsFrom(object obj, GetDepsFromContext context)
        {
            base.GetDepsFrom(obj, context);
        }
        //<TEMPLATE_BODY_END>
#endif
    }
}


