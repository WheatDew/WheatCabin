#if !RTSL_MAINTENANCE
using Battlehub.RTSL;
using ProtoBuf;
using UnityEngine;
using Battlehub.RTEditor;
using System.Collections.Generic;
using UnityEngine.Battlehub.SL2;

namespace Battlehub.RTEditor.Battlehub.SL2
{
    [CustomImplementation]
    public partial class PersistentRuntimeAnimationProperty<TID> 
    {        

        [ProtoMember(1)]
        public string ComponentTypeName;

        [ProtoMember(2)]
        public string ComponentDisplayName;

        [ProtoMember(3)]
        public string PropertyName;

        [ProtoMember(4)]
        public string PropertyDisplayName;

        [ProtoMember(5)]
        public string AnimationPropertyName;

        [ProtoMember(6)]
        public List<PersistentRuntimeAnimationProperty<TID>> Children;

        [ProtoMember(7)]
        public PersistentAnimationCurve<TID> Curve;

        public override void ReadFrom(object obj)
        {
            base.ReadFrom(obj);

            RuntimeAnimationProperty property = (RuntimeAnimationProperty)obj;
            if (property == null)
            {
                return;
            }

            ComponentTypeName = property.ComponentTypeName;
            ComponentDisplayName = property.ComponentDisplayName;
            PropertyName = property.PropertyName;
            PropertyDisplayName = property.PropertyDisplayName;
            AnimationPropertyName = property.AnimationPropertyName;
           
            if(property.Children != null)
            {
                Children = new List<PersistentRuntimeAnimationProperty<TID>>();
                for(int i = 0; i < property.Children.Count; ++i)
                {
                    PersistentRuntimeAnimationProperty<TID> persistentChildProperty = new PersistentRuntimeAnimationProperty<TID>();
                    persistentChildProperty.ReadFrom(property.Children[i]);
                    Children.Add(persistentChildProperty);
                }
            }

            if (property.Curve != null)
            {
                Curve = new PersistentAnimationCurve<TID>();
                Curve.ReadFrom(property.Curve);
            }
        }

        public override object WriteTo(object obj)
        {
            obj = base.WriteTo(obj);
            RuntimeAnimationProperty property = (RuntimeAnimationProperty)obj;
            if (property == null)
            {
                return null;
            }

            property.ComponentTypeName = ComponentTypeName;
            property.ComponentDisplayName = ComponentDisplayName;
            property.PropertyName = PropertyName;
            property.PropertyDisplayName = PropertyDisplayName;
            property.AnimationPropertyName = AnimationPropertyName;

            if (Children != null)
            {
                property.Children = new List<RuntimeAnimationProperty>();
                for(int i = 0; i < Children.Count; ++i)
                {
                    RuntimeAnimationProperty childProperty = new RuntimeAnimationProperty();
                    Children[i].WriteTo(childProperty);

                    childProperty.Parent = property;
                    property.Children.Add(childProperty);
                }
            }

            if(Curve != null)
            {
                property.Curve = new AnimationCurve();
                Curve.WriteTo(property.Curve);
            }

            return property;
        }
    }
}
#endif

