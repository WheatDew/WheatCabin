using Battlehub;
using ProtoBuf;
using System;

namespace UnityEngine.Battlehub.SL2
{
    [ProtoContract]
    public class PersistentRuntimeSerializableObject<TID> : PersistentObject<TID>
    {
        [ProtoMember(1)]
        public string AssemblyQualifiedName;

        public Type ObjectType
        {
            get { return Type.GetType(AssemblyQualifiedName); }
        }

   
        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            if (obj == null)
            {
                return null;
            }

            AssemblyQualifiedName = Reflection.GetAssemblyQualifiedName(obj.GetType());
            //TODO: Write data using reflection
            return obj;
        }

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            //TODO: Read data using reflection
        }

        public override bool CanInstantiate(Type type)
        {
            if(type == ObjectType)
            {
                return true;
            }
            return false;
        }

        public override object Instantiate(Type type)
        {
            if(typeof(ScriptableObject).IsSubclassOf(type))
            {
                return ScriptableObject.CreateInstance(type);
            }

            return base.Instantiate(type);
        }
    }

    [Obsolete("Use generic version")]
    [ProtoContract]
    public class PersistentRuntimeSerializableObject : PersistentRuntimeSerializableObject<long>
    {

    }

}
