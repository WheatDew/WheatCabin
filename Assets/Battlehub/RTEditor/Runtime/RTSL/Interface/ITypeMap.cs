using System;

namespace Battlehub.RTSL.Interface
{
    public interface ITypeMap<TID> : ITypeMap
    {
    }

    public interface ITypeMap
    {
        Type ToPersistentType(Type unityType);
        Type ToUnityType(Type persistentType);
        Type ToType(Guid typeGuid);
        Guid ToGuid(Type type);

        void RegisterRuntimeSerializableType(Type type, Guid typeGuid);
        void UnregisterRuntimeSerialzableType(Type type);

        void Register(Type type, Type persistentType, Guid unityTypeGuid, Guid persistentTypeGuid);

        void Unregister(Type type, Type persistentType);
    }

}