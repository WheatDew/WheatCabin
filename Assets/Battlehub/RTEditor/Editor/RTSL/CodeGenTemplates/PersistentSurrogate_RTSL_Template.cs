using System;
using System.Collections.Generic;
using UnityEngine;

namespace Battlehub.RTSL.Internal
{
    public class PersistentTemplateAttribute : Attribute
    {
        public readonly string ForType;
        public readonly string[] FieldNames;
        public readonly string[] RequiredTypes;

        public PersistentTemplateAttribute(string forType)
        {
            ForType = forType;
            FieldNames = new string[0];
            RequiredTypes = new string[0];
        }

        public PersistentTemplateAttribute(string forType, string[] templateFields)
        {
            ForType = forType;
            FieldNames = templateFields;
            RequiredTypes = new string[0];
        }

        public PersistentTemplateAttribute(string forType, string[] templateFields, string[] requiredTypes)
        {
            ForType = forType;
            FieldNames = templateFields;
            RequiredTypes = requiredTypes;
        }
    }

    public class PersistentSurrogateTemplate : ScriptableObject
    {
        public class TID { }

        protected new int hideFlags;

        protected IAssetDB<TID> m_assetDB;

        public virtual void ReadFrom(object obj)
        {
            throw new InvalidOperationException();
        }

        public virtual object WriteTo(object obj)
        {
            throw new InvalidOperationException();
        }

        public List<T> Assign<V, T>(List<V> list, Func<object, T> convert)
        {
            throw new InvalidOperationException();
        }

        public T[] Assign<V, T>(V[] arr, Func<object, T> convert)
        {
            throw new InvalidOperationException();
        }

        public virtual void GetDeps(GetDepsContext<TID> context)
        {
            throw new InvalidOperationException();
        }

        public virtual void GetDepsFrom(object obj, GetDepsFromContext context)
        {
            throw new InvalidOperationException();
        }

        protected void WriteSurrogateTo(object from, object to)
        {
            throw new InvalidOperationException();
        }

        protected T ReadSurrogateFrom<T>(object obj)
        {
            throw new InvalidOperationException();
        }

        protected void AddDep(TID depenency, object context)
        {
            throw new InvalidOperationException();
        }

        protected void AddDep(TID[] depenencies, object context)
        {
            throw new InvalidOperationException();
        }

        protected void AddDep(object obj, object context)
        {
            throw new InvalidOperationException();
        }

        protected void AddDep<T>(T[] dependencies, object context)
        {
            throw new InvalidOperationException();
        }

        protected void AddSurrogateDeps(object surrogate, object context)
        {
            throw new InvalidOperationException();
        }

        protected void AddSurrogateDeps<T>(T[] surrogateArray, object context)
        {
            throw new InvalidOperationException();
        }

        protected void AddSurrogateDeps<T>(T obj, Func<object, object> convert, GetDepsContext<TID> context)
        {
            throw new InvalidOperationException();
        }

        protected void AddSurrogateDeps<T>(T[] objArray, Func<object, object> convert, GetDepsContext<TID> context)
        {
            throw new InvalidOperationException();
        }

        protected void AddSurrogateDeps<T>(T obj, Func<object, object> convert, GetDepsFromContext context)
        {
            throw new InvalidOperationException();
        }

        protected void AddSurrogateDeps<T>(T[] objArray, Func<object, object> convert, GetDepsFromContext context)
        {
            throw new InvalidOperationException();
        }

        protected TID ToID(object uo)
        {
            throw new InvalidOperationException();
        }

        protected TID[] ToID(object[] uo)
        {
            throw new InvalidOperationException();
        }

        public T FromID<T>(TID id)
        {
            throw new InvalidOperationException();
        }


        public T[] FromID<T>(TID[] id)
        {
            throw new InvalidOperationException();
        }

        public T[] FromID<T>(TID[] id, T[] fallback)
        {
            throw new InvalidOperationException();
        }

        public virtual bool CanInstantiate(Type type)
        {
            throw new InvalidOperationException();
        }

        public virtual object Instantiate(Type type)
        {
            throw new InvalidOperationException();
        }
    }
}