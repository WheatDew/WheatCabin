using Battlehub.RTCommon;
using Battlehub.RTSL.Interface;
using System;
using UnityEngine;

namespace Battlehub.RTSL
{
    public class PlayerPrefsStorage : PlayerPrefsStorage<long>
    { }

    public class PlayerPrefsStorage<TID> : IPlayerPrefsStorage
    {
        public ProjectAsyncOperation<T> GetValue<T>(string key, ProjectEventHandler<T> callback = null)
        {
            ProjectAsyncOperation<T> ao = new ProjectAsyncOperation<T>();
            ITypeMap typeMap = IOC.Resolve<ITypeMap>();
            Type persistentType = typeMap.ToPersistentType(typeof(T));
            if (persistentType == null || !PlayerPrefs.HasKey(key))
            {
                ao.Error = new Error(Error.E_NotFound);
                if (callback != null)
                {
                    callback(ao.Error, default(T));
                }
                ao.IsCompleted = true;
            }
            else
            {
                string data = PlayerPrefs.GetString(key);
                byte[] bytes = Convert.FromBase64String(data);

                ISerializer serializer = IOC.Resolve<ISerializer>();
                PersistentSurrogate<TID> surrogate = (PersistentSurrogate<TID>)serializer.Deserialize(bytes, persistentType);

                T obj = (T)surrogate.Instantiate(typeof(T));
                surrogate.WriteTo(obj);

                ao.Result = obj;
                ao.Error = Error.NoError;
                if (callback != null)
                {
                    callback(ao.Error, ao.Result);
                }
                ao.IsCompleted = true;
            }

            return ao;
        }

        public ProjectAsyncOperation SetValue<T>(string key, T obj, ProjectEventHandler callback = null)
        {
            ProjectAsyncOperation ao = new ProjectAsyncOperation();
            ITypeMap typeMap = IOC.Resolve<ITypeMap>();
            Type persistentType = typeMap.ToPersistentType(typeof(T));
            if (persistentType == null)
            {
                ao.Error = new Error(Error.E_NotFound);
                if (callback != null)
                {
                    callback(ao.Error);
                }
                ao.IsCompleted = true;
            }
            else
            {
                IPersistentSurrogate surrogate = (IPersistentSurrogate)Activator.CreateInstance(persistentType);
                surrogate.ReadFrom(obj);
                ISerializer serializer = IOC.Resolve<ISerializer>();
                byte[] bytes = serializer.Serialize(surrogate);
                string data = Convert.ToBase64String(bytes);
                PlayerPrefs.SetString(key, data);
                ao.Error = Error.NoError;
                if (callback != null)
                {
                    callback(ao.Error);
                }
                ao.IsCompleted = true;
            }

            return ao;
        }

        public ProjectAsyncOperation DeleteValue<T>(string key, ProjectEventHandler callback = null)
        {
            ProjectAsyncOperation ao = new ProjectAsyncOperation();
            PlayerPrefs.DeleteKey(key);
            if(callback != null)
            {
                callback(Error.NoError);
            }
            ao.Error = Error.NoError;
            ao.IsCompleted = true;
            return ao;
        }

    }

}
