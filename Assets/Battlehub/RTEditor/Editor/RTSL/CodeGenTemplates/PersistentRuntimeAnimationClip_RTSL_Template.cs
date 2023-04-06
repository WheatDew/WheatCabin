//#define RTSL_COMPILE_TEMPLATES
#if RTSL_COMPILE_TEMPLATES
//<TEMPLATE_USINGS_START>
using ProtoBuf;
using System;
using Battlehub.RTEditor;
using System.Collections.Generic;
using UnityEngine;
//<TEMPLATE_USINGS_END>
#else
using UnityEngine;
#endif

namespace Battlehub.RTSL.Internal
{
    [PersistentTemplate("Battlehub.RTEditor.RuntimeAnimationClip", 
        new []{ "Properties" }, new[] { "Battlehub.RTEditor.RuntimeAnimationProperty" })]
    public class PersistentRuntimeAnimationClip_RTSL_Template : PersistentSurrogateTemplate
    {
        public class PersistentRuntimeAnimationProperty<T> : PersistentSurrogateTemplate { }

#if RTSL_COMPILE_TEMPLATES
        //<TEMPLATE_BODY_START>

        [ProtoMember(1)]
        public PersistentRuntimeAnimationProperty<TID>[] Properties;

        public override void ReadFrom(object obj)
        {
            base.ReadFrom(obj);

            RuntimeAnimationClip clip = (RuntimeAnimationClip)obj;
            if (clip == null)
            {
                return;
            }

            ICollection<RuntimeAnimationProperty> properties = clip.Properties;
            if(properties != null)
            {
                int index = 0;
                Properties = new PersistentRuntimeAnimationProperty<TID>[properties.Count];
                foreach(RuntimeAnimationProperty property in properties)
                {
                    Properties[index] = new PersistentRuntimeAnimationProperty<TID>();
                    Properties[index].ReadFrom(property);
                    index++;
                }
            }
        }

        public override object WriteTo(object obj)
        {
            obj = base.WriteTo(obj);
            RuntimeAnimationClip clip = (RuntimeAnimationClip)obj;
            if (clip == null)
            {
                return null;
            }

            clip.Clear();

            if(Properties != null)
            {
                for(int i = 0; i < Properties.Length; ++i)
                {
                    RuntimeAnimationProperty property = new RuntimeAnimationProperty();
                    Properties[i].WriteTo(property);

                    clip.Add(property);
                }
            }
           
            return clip;
        }

        public override void GetDeps(GetDepsContext<TID> context)
        {
            base.GetDeps(context);
        }

        public override void GetDepsFrom(object obj, GetDepsFromContext context)
        {
            base.GetDepsFrom(obj, context);
        }

        public override bool CanInstantiate(Type type)
        {
            return true;
        }

        public override object Instantiate(Type type)
        {
              return ScriptableObject.CreateInstance<RuntimeAnimationClip>();
        }

        //<TEMPLATE_BODY_END>
#endif
    }
}


