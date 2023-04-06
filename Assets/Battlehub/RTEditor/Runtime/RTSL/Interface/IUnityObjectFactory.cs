using System;
using UnityObject = UnityEngine.Object;

namespace Battlehub.RTSL.Interface
{
    public interface IUnityObjectFactory
    {
        bool CanCreateInstance(Type type);
        bool CanCreateInstance(Type type, IPersistentSurrogate surrogate);

        UnityObject CreateInstance(Type type);
        UnityObject CreateInstance(Type type, IPersistentSurrogate surrogate);
    }
}
