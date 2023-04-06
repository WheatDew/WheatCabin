using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using UnityObject = UnityEngine.Object;

namespace Battlehub.RTSL
{
    [ProtoContract]
    public class PersistentDescriptor<TID>
    {
        [ProtoIgnore]
        public PersistentDescriptor<TID> Parent;

        [ProtoMember(1)]
        public PersistentDescriptor<TID>[] Children;

        [ProtoMember(2)]
        public PersistentDescriptor<TID>[] Components;

        [ProtoMember(3)]
        public TID PersistentID;

        [ProtoMember(4)]
        public Guid PersistentTypeGuid;

        [ProtoMember(5)]
        public string Name;

        [ProtoMember(6)]
        public Guid RuntimeTypeGuid;

        public PersistentDescriptor()
        {
        }

        public PersistentDescriptor(Guid persistentTypeGuid, TID persistentID, string name, Guid runtimeTypeGuid)
        {
            PersistentID = persistentID;
            PersistentTypeGuid = persistentTypeGuid;
            Name = name;
            RuntimeTypeGuid = runtimeTypeGuid;

            Children = new PersistentDescriptor<TID>[0];
            Components = new PersistentDescriptor<TID>[0];
        }

        [ProtoAfterDeserialization]
        public void OnDeserialized()
        {
            if (Components != null)
            {
                for (int i = 0; i < Components.Length; ++i)
                {
                    Components[i].Parent = this;
                }
            }

            if (Children != null)
            {
                for (int i = 0; i < Children.Length; ++i)
                {
                    Children[i].Parent = this;
                }
            }
        }

        public override string ToString()
        {
            string pathToDesriptor = string.Empty;
            PersistentDescriptor<TID> descriptor = this;
            if (descriptor.Parent == null)
            {
                pathToDesriptor += "/";
            }
            else
            {
                while (descriptor.Parent != null)
                {
                    pathToDesriptor += "/" + descriptor.Parent.PersistentID;
                    descriptor = descriptor.Parent;
                }
            }
            return string.Format("Descriptor InstanceId = {0}, Type = {1}, Path = {2}, Children = {3} Components = {4}, RuntimeTypeGuid = {5}", PersistentID, PersistentTypeGuid, pathToDesriptor, Children != null ? Children.Length : 0, Components != null ? Components.Length : 0, RuntimeTypeGuid);
        }

        #region Obsolete
        [Obsolete]
        public PersistentDescriptor(Guid persistentTypeGuid, TID persistentID, string name) : this(persistentTypeGuid, persistentID, name, Guid.Empty)
        {
        }

        #endregion
    }
}



