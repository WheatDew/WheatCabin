using Battlehub.RTCommon;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace UnityEngine.Battlehub.SL2
{ }
namespace Battlehub.RTSL
{
    public class CustomImplementationAttribute : Attribute
    {
    }

    [ProtoContract]
    public class IntArray
    {
        [ProtoMember(1)]
        public int[] Array;
    }


    public abstract class PersistentSurrogateBase : IPersistentSurrogate
    {
        protected virtual void ReadFromImpl(object obj) { }
        protected virtual object WriteToImpl(object obj) { return obj; }
        protected virtual void GetDepsFromImpl(object obj, GetDepsFromContext context) { }
        public virtual bool CanInstantiate(Type type)
        {
            return type != null && type.GetConstructor(Type.EmptyTypes) != null;
        }

        public virtual object Instantiate(Type type)
        {
            return Activator.CreateInstance(type);
        }

        public virtual void ReadFrom(object obj)
        {
            if (obj == null)
            {
                return;
            }
            ReadFromImpl(obj);
        }

        public virtual object WriteTo(object obj)
        {
            if (obj == null)
            {
                return null;
            }
            obj = WriteToImpl(obj);
            return obj;
        }

        public virtual void GetDepsFrom(object obj, GetDepsFromContext context)
        {
            if (context.VisitedObjects.Contains(obj))
            {
                return;
            }
            context.VisitedObjects.Add(obj);
            GetDepsFromImpl(obj, context);
        }

        protected void WriteSurrogateTo(IPersistentSurrogate from, object to)
        {
            if (from == null)
            {
                return;
            }

            from.WriteTo(to);
        }

        protected T ReadSurrogateFrom<T>(object obj) where T : IPersistentSurrogate, new()
        {
            T surrogate = new T();
            surrogate.ReadFrom(obj);
            return surrogate;
        }
    }

    public abstract class PersistentSurrogate<TID> : PersistentSurrogateBase, IPersistentSurrogate<TID>
    {
        protected readonly IAssetDB<TID> m_assetDB;
        protected PersistentSurrogate()
        {
            m_assetDB = IOC.Resolve<IAssetDB<TID>>();
        }

        protected virtual void GetDepsImpl(GetDepsContext<TID> context) { }

        public virtual void GetDeps(GetDepsContext<TID> context)
        {
            if (context.VisitedObjects.Contains(this))
            {
                return;
            }
            context.VisitedObjects.Add(this);
            GetDepsImpl(context);
        }

        protected void AddDep(TID depenency, GetDepsContext<TID> context)
        {
            if (!m_assetDB.IsNullID(depenency) && !context.Dependencies.Contains(depenency))
            {
                context.Dependencies.Add(depenency);
            }
        }

        protected void AddDep(TID[] depenencies, GetDepsContext<TID> context)
        {
            if (depenencies == null)
            {
                return;
            }

            for (int i = 0; i < depenencies.Length; ++i)
            {
                AddDep(depenencies[i], context);
            }
        }

        protected void AddDep(Dictionary<TID, TID> depenencies, GetDepsContext<TID> context)
        {
            if (depenencies == null)
            {
                return;
            }

            foreach (KeyValuePair<TID, TID> kvp in depenencies)
            {
                AddDep(kvp.Key, context);
                AddDep(kvp.Value, context);
            }
        }

        protected void AddDep<V>(Dictionary<TID, V> depenencies, GetDepsContext<TID> context)
        {
            if (depenencies == null)
            {
                return;
            }

            foreach (KeyValuePair<TID, V> kvp in depenencies)
            {
                AddDep(kvp.Key, context);
            }
        }

        protected void AddDep<T>(Dictionary<T, TID> depenencies, GetDepsContext<TID> context)
        {
            if (depenencies == null)
            {
                return;
            }

            foreach (KeyValuePair<T, TID> kvp in depenencies)
            {
                AddDep(kvp.Value, context);
            }
        }

        protected void AddDep(object obj, GetDepsFromContext context)
        {
            if (obj != null && !obj.Equals(null) && !context.Dependencies.Contains(obj))
            {
                context.Dependencies.Add(obj);
            }
        }

        protected void AddDep<T>(T[] dependencies, GetDepsFromContext context)
        {
            if (dependencies == null)
            {
                return;
            }
            for (int i = 0; i < dependencies.Length; ++i)
            {
                AddDep(dependencies[i], context);
            }
        }

        protected void AddDep<T>(List<T> dependencies, GetDepsFromContext context)
        {
            if (dependencies == null)
            {
                return;
            }
            for (int i = 0; i < dependencies.Count; ++i)
            {
                AddDep(dependencies[i], context);
            }
        }

        protected void AddDep<T>(HashSet<T> dependencies, GetDepsFromContext context)
        {
            if (dependencies == null)
            {
                return;
            }
            foreach (T dep in dependencies)
            {
                AddDep(dep, context);
            }
        }

        protected void AddDep<T, V>(Dictionary<T, V> dependencies, GetDepsFromContext context)
        {
            if (dependencies == null)
            {
                return;
            }
            foreach (KeyValuePair<T, V> kvp in dependencies)
            {
                AddDep(kvp.Key, context);
                if (kvp.Value != null)
                {
                    AddDep(kvp.Value, context);
                }
            }
        }

        protected void AddSurrogateDeps(IPersistentSurrogate<TID> surrogate, GetDepsContext<TID> context)
        {
            if (surrogate == null)
            {
                return;
            }

            surrogate.GetDeps(context);
        }

        protected void AddSurrogateDeps<T>(T[] surrogateArray, GetDepsContext<TID> context) where T : IPersistentSurrogate<TID>
        {
            if (surrogateArray == null)
            {
                return;
            }
            for (int i = 0; i < surrogateArray.Length; ++i)
            {
                IPersistentSurrogate<TID> surrogate = surrogateArray[i];
                surrogate.GetDeps(context);
            }
        }

        protected void AddSurrogateDeps<T>(List<T> surrogateList, GetDepsContext<TID> context) where T : IPersistentSurrogate<TID>
        {
            if (surrogateList == null)
            {
                return;
            }
            for (int i = 0; i < surrogateList.Count; ++i)
            {
                IPersistentSurrogate<TID> surrogate = surrogateList[i];
                surrogate.GetDeps(context);
            }
        }

        protected void AddSurrogateDeps<T>(HashSet<T> surrogatesHS, GetDepsContext<TID> context) where T : IPersistentSurrogate<TID>
        {
            if (surrogatesHS == null)
            {
                return;
            }
            foreach (IPersistentSurrogate<TID> surrogate in surrogatesHS)
            {
                surrogate.GetDeps(context);
            }
        }

        protected void AddSurrogateDeps<T, V>(Dictionary<T, V> surrogateDict, GetDepsContext<TID> context)
        {
            if (surrogateDict == null)
            {
                return;
            }

            foreach (KeyValuePair<T, V> kvp in surrogateDict)
            {
                IPersistentSurrogate<TID> surrogate = kvp.Key as IPersistentSurrogate<TID>;
                if (surrogate != null)
                {
                    surrogate.GetDeps(context);
                }

                surrogate = kvp.Value as IPersistentSurrogate<TID>;
                if (surrogate != null)
                {
                    surrogate.GetDeps(context);
                }
            }
        }

        protected void AddSurrogateDeps<V>(Dictionary<TID, V> surrogateDict, GetDepsContext<TID> context) where V : IPersistentSurrogate<TID>
        {
            if (surrogateDict == null)
            {
                return;
            }

            foreach (KeyValuePair<TID, V> kvp in surrogateDict)
            {
                AddDep(kvp.Key, context);
                if (kvp.Value != null)
                {
                    kvp.Value.GetDeps(context);
                }
            }
        }

        protected void AddSurrogateDeps<T>(Dictionary<T, TID> surrogateDict, GetDepsContext<TID> context) where T : IPersistentSurrogate<TID>
        {
            if (surrogateDict == null)
            {
                return;
            }
            foreach (KeyValuePair<T, TID> kvp in surrogateDict)
            {
                kvp.Key.GetDeps(context);
                AddDep(kvp.Value, context);
            }
        }

        protected void AddSurrogateDeps<T>(T obj, Func<T, IPersistentSurrogate<TID>> convert, GetDepsContext<TID> context)
        {
            if (obj != null)
            {
                IPersistentSurrogate<TID> surrogate = convert(obj);
                surrogate.GetDeps(context);
            }
        }

        protected void AddSurrogateDeps<T>(T[] objArray, Func<T, IPersistentSurrogate<TID>> convert, GetDepsContext<TID> context)
        {
            if (objArray == null)
            {
                return;
            }
            for (int i = 0; i < objArray.Length; ++i)
            {
                T obj = objArray[i];
                if (obj != null)
                {
                    IPersistentSurrogate<TID> surrogate = convert(obj);
                    surrogate.GetDeps(context);
                }
            }
        }

        protected void AddSurrogateDeps<T>(List<T> objList, Func<T, IPersistentSurrogate<TID>> convert, GetDepsContext<TID> context)
        {
            if (objList == null)
            {
                return;
            }
            for (int i = 0; i < objList.Count; ++i)
            {
                T obj = objList[i];
                if (obj != null)
                {
                    IPersistentSurrogate<TID> surrogate = convert(obj);
                    surrogate.GetDeps(context);
                }
            }
        }

        protected void AddSurrogateDeps<T>(HashSet<T> objHs, Func<T, IPersistentSurrogate<TID>> convert, GetDepsContext<TID> context)
        {
            if (objHs == null)
            {
                return;
            }
            foreach (T obj in objHs)
            {
                if (obj != null)
                {
                    IPersistentSurrogate<TID> surrogate = convert(obj);
                    surrogate.GetDeps(context);
                }
            }
        }

        protected void AddSurrogateDeps<T>(T obj, Func<T, IPersistentSurrogate> convert, GetDepsFromContext context)
        {
            if (obj != null)
            {
                IPersistentSurrogate surrogate = convert(obj);
                surrogate.GetDepsFrom(obj, context);
            }
        }

        protected void AddSurrogateDeps<T>(T[] objArray, Func<T, IPersistentSurrogate> convert, GetDepsFromContext context)
        {
            if (objArray == null)
            {
                return;
            }
            for (int i = 0; i < objArray.Length; ++i)
            {
                T obj = objArray[i];
                if (obj != null)
                {
                    IPersistentSurrogate surrogate = convert(obj);
                    surrogate.GetDepsFrom(obj, context);
                }
            }
        }

        protected void AddSurrogateDeps<T>(List<T> objList, Func<T, IPersistentSurrogate> convert, GetDepsFromContext context)
        {
            if (objList == null)
            {
                return;
            }
            for (int i = 0; i < objList.Count; ++i)
            {
                T obj = objList[i];
                if (obj != null)
                {
                    IPersistentSurrogate surrogate = convert(obj);
                    surrogate.GetDepsFrom(obj, context);
                }
            }
        }

        protected void AddSurrogateDeps<T>(HashSet<T> objHs, Func<T, IPersistentSurrogate> convert, GetDepsFromContext context)
        {
            if (objHs == null)
            {
                return;
            }
            foreach (T obj in objHs)
            {
                if (obj != null)
                {
                    IPersistentSurrogate surrogate = convert(obj);
                    surrogate.GetDepsFrom(obj, context);
                }
            }
        }

        protected void AddSurrogateDeps<T, V, T1, V1>(Dictionary<T, V> dict, Func<T, T1> convertKey, Func<V, V1> convertValue, GetDepsFromContext context)
        {
            if (dict == null)
            {
                return;
            }
            foreach (KeyValuePair<T, V> kvp in dict)
            {
                T obj = kvp.Key;

                IPersistentSurrogate surrogate = convertKey(obj) as IPersistentSurrogate;
                if (surrogate != null)
                {
                    surrogate.GetDepsFrom(obj, context);
                }

                surrogate = convertValue(kvp.Value) as IPersistentSurrogate;
                if (surrogate != null)
                {
                    surrogate.GetDepsFrom(obj, context);
                }
            }
        }

        public T[] Assign<V, T>(V[] arr, Func<V, T> convert)
        {
            if (arr == null)
            {
                return null;
            }

            T[] result = new T[arr.Length];
            for (int i = 0; i < arr.Length; ++i)
            {
                result[i] = convert(arr[i]);
            }
            return result;
        }

        public List<T> Assign<V, T>(List<V> list, Func<V, T> convert)
        {
            if (list == null)
            {
                return null;
            }

            List<T> result = new List<T>(list.Count);
            for (int i = 0; i < list.Count; ++i)
            {
                result.Add(convert(list[i]));
            }
            return result;
        }

        public HashSet<T> Assign<V, T>(HashSet<V> hs, Func<V, T> convert)
        {
            if (hs == null)
            {
                return null;
            }

            HashSet<T> result = new HashSet<T>();
            foreach (V obj in hs)
            {
                result.Add(convert(obj));
            }
            return result;
        }

        protected Dictionary<TOUT, VOUT> Assign<TIN, TOUT, VIN, VOUT>(Dictionary<TIN, VIN> dict, Func<TIN, TOUT> convertKey, Func<VIN, VOUT> convertValue)
        {
            if (dict == null)
            {
                return null;
            }

            Dictionary<TOUT, VOUT> result = new Dictionary<TOUT, VOUT>();
            foreach (KeyValuePair<TIN, VIN> kvp in dict)
            {
                TOUT key = convertKey(kvp.Key);
                VOUT value = convertValue(kvp.Value);

                if (key != null)
                {
                    result.Add(key, value);
                }
            }
            return result;
        }

        protected virtual TID ToID(UnityObject uo)
        {
            return m_assetDB.ToID(uo);
        }

        public virtual TID[] ToID(UnityObject[] uo)
        {
            if (uo == null)
            {
                return null;
            }

            TID[] ids = new TID[uo.Length];
            for (int i = 0; i < ids.Length; ++i)
            {
                ids[i] = ToID(uo[i]);
            }
            return ids;
        }

        public virtual TID[] ToID<T>(List<T> uo) where T : UnityObject
        {
            if (uo == null)
            {
                return null;
            }

            TID[] ids = new TID[uo.Count];
            for (int i = 0; i < ids.Length; ++i)
            {
                ids[i] = ToID(uo[i]);
            }
            return ids;
        }

        public virtual TID[] ToID<T>(HashSet<T> uo) where T : UnityObject
        {
            if (uo == null)
            {
                return null;
            }

            TID[] ids = uo.Select(o => ToID(o)).ToArray();
            return ids;
        }

        protected Dictionary<TID, TID> ToID<T, V>(Dictionary<T, V> uo) where T : UnityObject where V : UnityObject
        {
            if (uo == null)
            {
                return null;
            }

            Dictionary<TID, TID> result = new Dictionary<TID, TID>();
            foreach (KeyValuePair<T, V> kvp in uo)
            {
                TID key = ToID(kvp.Key);
                TID value = ToID(kvp.Value);
                if (!result.ContainsKey(key))
                {
                    result.Add(key, value);
                }
            }
            return result;
        }

        protected Dictionary<TID, VOUT> ToID<T, VOUT, VIN>(Dictionary<T, VIN> uo, Func<VIN, VOUT> convert) where T : UnityObject
        {
            if (uo == null)
            {
                return null;
            }

            Dictionary<TID, VOUT> result = new Dictionary<TID, VOUT>();
            foreach (KeyValuePair<T, VIN> kvp in uo)
            {
                TID key = ToID(kvp.Key);
                VOUT value = convert(kvp.Value);
                if (!result.ContainsKey(key))
                {
                    result.Add(key, value);
                }
            }
            return result;
        }

        protected Dictionary<TOUT, TID> ToID<TOUT, TIN, V>(Dictionary<TIN, V> uo, Func<TIN, TOUT> convert) where V : UnityObject
        {
            if (uo == null)
            {
                return null;
            }

            Dictionary<TOUT, TID> result = new Dictionary<TOUT, TID>();
            foreach (KeyValuePair<TIN, V> kvp in uo)
            {
                TOUT key = convert(kvp.Key);
                TID value = ToID(kvp.Value);
                if (key != null)
                {
                    result.Add(key, value);
                }
            }
            return result;
        }

        protected T FromID<T>(TID id, T fallback = null) where T : UnityObject
        {
            if (m_assetDB.IsNullID(id))
            {
                return default;
            }

            T value = m_assetDB.FromID<T>(id);
            if (value == default(T))
            {
                return fallback;
            }

            return value;
        }

        protected T[] FromID<T>(TID[] id, T[] fallback = null) where T : UnityObject
        {
            if (id == null)
            {
                return null;
            }

            T[] objs = new T[id.Length];
            for (int i = 0; i < id.Length; ++i)
            {
                if (fallback != null && i < fallback.Length)
                {
                    objs[i] = FromID(id[i], fallback[i]);
                }
                else
                {
                    objs[i] = FromID<T>(id[i]);
                }
            }
            return objs;
        }

        protected List<T> FromID<T>(TID[] id, List<T> fallback = null) where T : UnityObject
        {
            if (id == null)
            {
                return null;
            }

            List<T> objs = new List<T>();
            for (int i = 0; i < id.Length; ++i)
            {
                if (fallback != null && i < fallback.Count)
                {
                    objs.Add(FromID(id[i], fallback[i]));
                }
                else
                {
                    objs.Add(FromID<T>(id[i]));
                }
            }
            return objs;
        }

        protected HashSet<T> FromID<T>(TID[] id, HashSet<T> fallback = null) where T : UnityObject
        {
            if (id == null)
            {
                return null;
            }

            HashSet<T> objs = new HashSet<T>();

            int count = 0;
            if (fallback != null)
            {
                foreach (T f in fallback)
                {
                    if (count >= id.Length)
                    {
                        break;
                    }

                    T obj = FromID(id[count], f);
                    if (obj != null)
                    {
                        objs.Add(obj);
                    }

                    count++;
                }
            }

            for (int i = count; i < id.Length; ++i)
            {
                T obj = FromID<T>(id[i]);
                if (obj != null)
                {
                    objs.Add(obj);
                }
            }
            return objs;
        }

        protected Dictionary<T, V> FromID<T, V>(Dictionary<TID, TID> id, Dictionary<T, V> fallback = null) where T : UnityObject where V : UnityObject
        {
            if (id == null)
            {
                return null;
            }

            Dictionary<T, V> objs = new Dictionary<T, V>();
            foreach (KeyValuePair<TID, TID> kvp in id)
            {
                T key = FromID<T>(kvp.Key);
                V value = FromID<V>(kvp.Value);
                if (key != null)
                {
                    objs.Add(key, value);
                }
            }
            return objs;
        }

        protected Dictionary<T, VOUT> FromID<T, VOUT, VIN>(Dictionary<TID, VIN> id, Func<VIN, VOUT> convert, Dictionary<T, VOUT> fallback = null) where T : UnityObject
        {
            if (id == null)
            {
                return null;
            }

            Dictionary<T, VOUT> objs = new Dictionary<T, VOUT>();
            foreach (KeyValuePair<TID, VIN> kvp in id)
            {
                T key = FromID<T>(kvp.Key);
                if (key != null)
                {
                    objs.Add(key, convert(kvp.Value));
                }
            }
            return objs;
        }

        protected Dictionary<TOUT, V> FromID<TOUT, TIN, V>(Dictionary<TIN, TID> id, Func<TIN, TOUT> convert, Dictionary<TOUT, V> fallback = null) where V : UnityObject
        {
            if (id == null)
            {
                return null;
            }

            Dictionary<TOUT, V> objs = new Dictionary<TOUT, V>();
            foreach (KeyValuePair<TIN, TID> kvp in id)
            {
                TOUT key = convert(kvp.Key);
                V value = FromID<V>(kvp.Value);
                if (key != null)
                {
                    objs.Add(key, value);
                }
            }
            return objs;
        }

        protected T GetPrivate<T>(object obj, string fieldName)
        {
            FieldInfo fieldInfo = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            if (fieldInfo == null)
            {
                return default(T);
            }
            object val = fieldInfo.GetValue(obj);
            if (val is T)
            {
                return (T)val;
            }
            return default(T);
        }

        protected T GetPrivate<V, T>(object obj, string fieldName)
        {
            FieldInfo fieldInfo = typeof(V).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            if (fieldInfo == null)
            {
                return default(T);
            }
            object val = fieldInfo.GetValue(obj);
            if (val is T)
            {
                return (T)val;
            }
            return default(T);
        }

        protected void SetPrivate<T>(object obj, string fieldName, T value)
        {
            FieldInfo fieldInfo = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            if (fieldInfo == null)
            {
                return;
            }

            if (!fieldInfo.FieldType.IsAssignableFrom(typeof(T)))
            {
                return;
            }

            fieldInfo.SetValue(obj, value);
        }

        protected void SetPrivate<V, T>(V obj, string fieldName, T value)
        {
            FieldInfo fieldInfo = typeof(V).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            if (fieldInfo == null)
            {
                return;
            }

            if (!fieldInfo.FieldType.IsAssignableFrom(typeof(T)))
            {
                return;
            }

            fieldInfo.SetValue(obj, value);
        }

        private static readonly Dictionary<object, object> m_refrencesCache = new Dictionary<object, object>();
        protected T ResolveReference<T, V>(V v, Func<T> fallback)
        {
            if (v == null)
            {
                return default(T);
            }

            object result;
            if (!m_refrencesCache.TryGetValue(v, out result))
            {
                result = fallback();
                m_refrencesCache.Add(v, result);
            }
            return (T)result;
        }

        protected T[] ResolveReference<T, V>(V[] v, Func<int, T> fallback)
        {
            if (v == null)
            {
                return null;
            }

            T[] result = new T[v.Length];
            for (int i = 0; i < v.Length; ++i)
            {
                if (v[i] == null)
                {
                    continue;
                }
                object res;
                if (!m_refrencesCache.TryGetValue(v[i], out res))
                {
                    res = fallback(i);
                    m_refrencesCache.Add(v[i], res);
                }
                result[i] = (T)res;
            }
            return result;
        }

        protected List<T> ResolveReference<T, V>(List<V> v, Func<int, T> fallback)
        {
            if (v == null)
            {
                return null;
            }

            List<T> result = new List<T>(v.Count);
            for (int i = 0; i < v.Count; ++i)
            {
                if (v[i] == null)
                {
                    continue;
                }
                object res;
                if (!m_refrencesCache.TryGetValue(v[i], out res))
                {
                    res = fallback(i);
                    m_refrencesCache.Add(v[i], res);
                }
                result.Add((T)res);
            }
            return result;
        }


        protected void ClearReferencesCache()
        {
            m_refrencesCache.Clear();
        }
    }


    [Obsolete("Use generic version instead")]
    public abstract class PersistentSurrogate : PersistentSurrogate<long>
    {
        //required to prevent compiler errors on rtsl update
        public virtual void GetDeps(GetDepsContext context)
        {
            GetDepsContext<long> ctx = context;
            GetDeps(ctx);
        }

        protected virtual void GetDepsImpl(GetDepsContext context)
        {
            GetDepsContext<long> ctx = context;
            GetDepsImpl(ctx);
        }
    }
}

