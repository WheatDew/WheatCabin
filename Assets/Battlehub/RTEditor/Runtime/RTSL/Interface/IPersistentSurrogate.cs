using System;
using System.Collections.Generic;

namespace Battlehub.RTSL
{
    [Obsolete("Use generic version of this class")]
    public class GetDepsContext : GetDepsContext<long>
    {
        //required to prevent compiler errors on rtsl update
    }

    public class GetDepsContext<TID>
    {
        public readonly HashSet<TID> Dependencies = new HashSet<TID>();
        public readonly HashSet<object> VisitedObjects = new HashSet<object>();

        public void Clear()
        {
            Dependencies.Clear();
            VisitedObjects.Clear();
        }
    }

    public class GetDepsFromContext
    {
        public readonly HashSet<object> Dependencies = new HashSet<object>();
        public readonly HashSet<object> VisitedObjects = new HashSet<object>();

        public void Clear()
        {
            Dependencies.Clear();
            VisitedObjects.Clear();
        }
    }

    public interface IPersistentSurrogate
    {
        void ReadFrom(object obj);

        object WriteTo(object obj);

        void GetDepsFrom(object obj, GetDepsFromContext context);

        bool CanInstantiate(Type type);

        object Instantiate(Type type);
    }

    public interface IPersistentSurrogate<TID> : IPersistentSurrogate
    {
        void GetDeps(GetDepsContext<TID> context);
    }
}

