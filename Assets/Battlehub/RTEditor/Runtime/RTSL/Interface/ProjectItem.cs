using ProtoBuf;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System;

using UnityObject = UnityEngine.Object;
using System.ComponentModel;

namespace Battlehub.RTSL.Interface
{
    [ProtoContract]
    public class AssetBundleItemInfo
    {
        [ProtoMember(1)]
        public string Path;

        [ProtoMember(2)]
        public int Id;

        [ProtoMember(3)]
        public int ParentId;
    }

    [ProtoContract]
    public class AssetBundleInfo
    {
        [ProtoMember(1)]
        public string UniqueName;

        [ProtoMember(2)]
        public int Ordinal;

        [ProtoMember(3)]
        public AssetBundleItemInfo[] AssetBundleItems;

        [ProtoMember(4)]
        public int Identifier = 4;
    }

    [ProtoContract]
    public class ProjectInfo
    {
        #region Deprecated
        [ProtoMember(2) /*, Obsolete*/]
        public int AssetIdentifier = 1;
        [ProtoMember(3)/*,, Obsolete*/]
        public int BundleIdentifier = 0;
        #endregion

        [ProtoMember(4)]
        public string Version = RTSLVersion.Version.ToString();
        public string Name
        {
            get;
            set;
        }
        public DateTime LastWriteTime
        {
            get;
            set;
        }
    }

    [ProtoContract]
    public class ProjectItem : INotifyPropertyChanged
    {
        public event EventHandler PreviewDataChanged;
        protected void RaisePropertyChanged(string name)
        {
            PreviewDataChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePreviewDataChanged()
        {
            PreviewDataChanged?.Invoke(this, EventArgs.Empty);
        }

        #region Deprecated
        [ProtoMember(2) /*, Obsolete*/]
        public long m_itemID;
        [ProtoMember(3) /*, Obsolete*/]
        public Guid m_ItemGUID;

        public virtual long ItemID
        {
            get { return m_itemID; }
            set { m_itemID = value; }
        }

        public virtual Guid ItemGUID
        {
            get { return m_ItemGUID; }
            set { m_ItemGUID = value; }
        }

        #endregion

        private string m_name;
        public string Name
        {
            get { return m_name; }
            set 
            {
                if(m_name != value)
                {
                    m_name = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
                }   
            }
        }
        public string Ext
        {
            get;
            set;
        }

        public ProjectItem Parent
        {
            get;
            set;
        }

        public List<ProjectItem> Children
        {
            get;
            set;
        }

        public ProjectItem()
        {
        }

        public ProjectItem(string name)
        {
            Name = name;
        }

        public string NameExt
        {
            get { return Name + Ext; }
        }

        /// <summary>
        /// Required for binding
        /// </summary>
        public ProjectItem Self
        {
            get { return this; }
        }

        public virtual bool IsFolder
        {
            get { return true; }
        }

        public virtual byte[] GetPreview()
        {
            return null;
        }

        public virtual void SetPreview(byte[] preview)
        {
        }

        public virtual Guid GetTypeGuid()
        {
            return Guid.Empty;
        }

        public virtual void SetTypeGuid(Guid guid)
        {
        }

        public virtual long GetCustomDataOffset()
        {
            return -1;
        }

        public virtual void SetCustomDataOffset(long offset)
        {
        }

        public virtual int[] GetAssetLibraryIDs()
        {
            return null;
        }

        public virtual void SetAssetLibraryIDs(int[] id)
        {
        }

        public static bool IsAssetItem(object obj)
        {
            ProjectItem projectItem = obj as ProjectItem;
            if(projectItem == null)
            {
                return false;
            }
            return !projectItem.IsFolder;
        }

        public static bool IsValidName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return true;
            }
            return Path.GetInvalidFileNameChars().All(c => !name.Contains(c));
        }

        public ProjectItem AddChild(ProjectItem item)
        {
            if (Children == null)
            {
                Children = new List<ProjectItem>();
            }

            if (item.Parent != null)
            {
                item.Parent.RemoveChild(item);
            }
            Children.Add(item);
            item.Parent = this;
            return item;
        }

        public void RemoveChild(ProjectItem item)
        {
            if (Children == null)
            {
                return;
            }
            Children.Remove(item);
            item.Parent = null;
        }

        public int GetSiblingIndex()
        {
            return Parent.Children.IndexOf(this);
        }

        public void SetSiblingIndex(int index)
        {
            Parent.Children.Remove(this);
            Parent.Children.Insert(index, this);
        }

        public ProjectItem[] Flatten(bool excludeFolders, bool excludeAssets = false)
        {
            List<ProjectItem> items = new List<ProjectItem>();
            Foreach(this, projectItem =>
            {
                if (excludeFolders && projectItem.IsFolder)
                {
                    return;
                }

                if (excludeAssets && !projectItem.IsFolder)
                {
                    return;
                }

                items.Add(projectItem);
            });
            return items.ToArray();
        }

        public void Foreach(ProjectItem item, Action<ProjectItem> callback)
        {
            if (item == null)
            {
                return;
            }

            callback(item);

            if (item.Children != null)
            {
                for (int i = 0; i < item.Children.Count; ++i)
                {
                    ProjectItem child = item.Children[i];
                    Foreach(child, callback);
                }
            }
        }

        [Obsolete]
        public ProjectItem Get(string path, bool forceCreate)
        {
            path = path.Trim('/');
            string[] pathParts = path.Split('/');

            ProjectItem item = this;
            for (int i = 1; i < pathParts.Length; ++i)
            {
                string pathPart = pathParts[i];
                if (item.Children == null)
                {
                    if (forceCreate)
                    {
                        item.Children = new List<ProjectItem>();
                    }
                    else
                    {
                        return item;
                    }
                }

                ProjectItem nextItem = item.Children.Where(child => child.NameExt == pathPart).FirstOrDefault();
                if (nextItem == null)
                {
                    if (forceCreate)
                    {
                        if (string.IsNullOrEmpty(Path.GetExtension(pathPart)))
                        {
                            nextItem = new ProjectItem
                            {
                                Name = pathPart
                            };
                            item.AddChild(nextItem);
                        }
                        else
                        {
                            nextItem = new AssetItem<long>
                            {
                                Name = Path.GetFileNameWithoutExtension(pathPart),
                                Ext = Path.GetExtension(pathPart),
                            };
                            item.AddChild(nextItem);
                        }
                    }
                    else
                    {
                        item = nextItem;
                        break;
                    }
                }
                item = nextItem;
            }
            return item;
        }

        public ProjectItem GetOrCreateFolder(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            int startIndex = 0;
            string[] pathParts = path.Trim('/').Split('/');
            if (path.StartsWith("/"))
            {
                startIndex = 1;
                if (!string.IsNullOrEmpty(pathParts[0]) && pathParts[0] != Name)
                {
                    return null;
                }
            }

            ProjectItem item = this;
            for (int i = startIndex; i < pathParts.Length; ++i)
            {
                if (item == null)
                {
                    break;
                }

                string pathPart = pathParts[i];
                if (item.Children == null)
                {
                    item.Children = new List<ProjectItem>();
                }

                ProjectItem nextItem = item.Children.Where(child => child.NameExt == pathPart).FirstOrDefault();
                if (nextItem == null)
                {
                    if (!string.IsNullOrWhiteSpace(pathPart) && string.IsNullOrEmpty(Path.GetExtension(pathPart)))
                    {
                        nextItem = new ProjectItem
                        {
                            Name = pathPart
                        };
                        item.AddChild(nextItem);
                    }
                }
                item = nextItem;
            }
            return item;
        }

        public ProjectItem Get(string path)
        {
            if(string.IsNullOrEmpty(path))
            {
                return null;
            }

            int startIndex = 0;
            string[] pathParts = path.Trim('/').Split('/');
            if (path.StartsWith("/"))
            {
                startIndex = 1;
                if (!string.IsNullOrEmpty(pathParts[0]) && pathParts[0] != Name)
                {
                    return null;
                }
            }
            ProjectItem item = this;
            for (int i = startIndex; i < pathParts.Length; ++i)
            {
                if (item == null)
                {
                    break;
                }

                string pathPart = pathParts[i];
                if (item.Children == null)
                {
                    break;
                }

                item = item.Children.Where(child => child.NameExt == pathPart).FirstOrDefault();
            }
            return item;
        }

        public bool IsDescendantOf(ProjectItem ancestor)
        {
            ProjectItem projectItem = this;
            while (projectItem != null)
            {
                if (projectItem == ancestor)
                {
                    return true;
                }

                projectItem = projectItem.Parent;
            }
            return false;
        }

        public string RelativePath(bool includeExt)
        {
            StringBuilder sb = new StringBuilder();
            ProjectItem parent = this;
            while (parent.Parent != null)
            {
                sb.Insert(0, parent.Name);
                sb.Insert(0, "/");
                parent = parent.Parent;
            }
            string ext = null;
            if (includeExt)
            {
                ext = Ext;
            }
            if (string.IsNullOrEmpty(ext))
            {
                return sb.ToString().TrimStart('/');
            }
            return string.Format("{0}{1}", sb.ToString(), Ext).TrimStart('/');
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            ProjectItem parent = this;
            while (parent != null)
            {
                sb.Insert(0, parent.Name);
                sb.Insert(0, "/");
                parent = parent.Parent;
            }

            string ext = Ext;
            if (string.IsNullOrEmpty(ext))
            {
                return sb.ToString();
            }
            return string.Format("{0}{1}", sb.ToString(), Ext);
        }
    }


    //[Obsolete]
    [ProtoContract]
    public class PrefabPart
    {
        [ProtoMember(1)]
        public long PartID;

        [ProtoMember(2)]
        public long ParentID;

        [ProtoMember(3)]
        public string Name;

        [ProtoMember(4)]
        public Guid TypeGuid;

        [ProtoMember(5) /*, Obsolete*/]
        public Guid PartGUID;

        [ProtoMember(6)/*, Obsolete*/]
        public Guid ParentGUID;
    }

    //[Obsolete]
    [ProtoContract]
    public class Preview // Preview<long>
    {
        [ProtoMember(1)]
        public long ItemID;

        [ProtoMember(2)]
        public byte[] PreviewData;

        [ProtoMember(3)]
        public Guid ItemGUID;

        public Preview<TID> ConvertToGenericPreview<TID>()
        {
            if(typeof(TID) == typeof(long))
            {
                return new Preview<TID>
                {
                    ID = (TID)(object)ItemID,
                    PreviewData = PreviewData
                };
            }

            else if (typeof(TID) == typeof(Guid))
            {
                return new Preview<TID>
                {
                    ID = (TID)(object)ItemGUID,
                    PreviewData = PreviewData
                };
            }

            throw new InvalidCastException();
        }

        public static implicit operator Preview(Preview<long> preview)
        {
            return new Preview
            {
                ItemID = preview.ID,
                PreviewData = preview.PreviewData
            };
        }

        public static implicit operator Preview(Preview<Guid> preview)
        {
            return new Preview
            {
                ItemGUID = preview.ID,
                PreviewData = preview.PreviewData
            };
        }
    }

    [ProtoContract]
    public class Preview<TID> 
    {
        [ProtoMember(1)]
        public TID ID;

        [ProtoMember(2)]
        public byte[] PreviewData;
    }


    /// <summary>
    /// This class required to restore data stored in RTSL 2.26 format
    /// </summary>
    [ProtoContract]
    public class LegacyAssetItem : ProjectItem
    {
        [ProtoMember(1)]
        public Guid TypeGuid;

        [ProtoMember(2) /*, Obsolete*/]
        public PrefabPart[] Parts;

        [ProtoMember(3) /*, Obsolete*/]
        public long[] Dependencies;

        [ProtoMember(4, IsRequired = true)]
        public long CustomDataOffset = -1;

        [ProtoMember(5) /*, Obsolete*/]
        public Guid[] DependenciesGuids;

        public override bool IsFolder
        {
            get { return false; }
        }

        public static LegacyAssetItem ConvertToLegacyAssetItem(ProjectItem projectItem)
        {
            if (projectItem is LegacyAssetItem)
            {
                return (LegacyAssetItem)projectItem;
            }
            else if (projectItem is AssetItem<long>)
            {
                return (AssetItem<long>)projectItem;
            }

            return (AssetItem<Guid>)projectItem;
        }

        public static ProjectItem ConvertToGenericAssetItem(ProjectItem projectItem)
        {
            LegacyAssetItem assetItem = projectItem as LegacyAssetItem;
            if (assetItem != null)
            {
                if (assetItem.ItemID != 0)
                {
                    projectItem = (AssetItem<long>)assetItem;
                }
                else 
                {
                    projectItem = (AssetItem<Guid>)assetItem;
                }
            }
            return projectItem;
        }


        private static LegacyAssetItem CreateAssetItem<TID>(AssetItem<TID> newAssetItem)
        {
            LegacyAssetItem assetItem = new LegacyAssetItem();
            assetItem.TypeGuid = newAssetItem.TypeGuid;
            assetItem.CustomDataOffset = newAssetItem.CustomDataOffset;
            assetItem.Name = newAssetItem.Name;
            assetItem.Ext = newAssetItem.Ext;

            return assetItem;
        }

        private static AssetItem<TID> CreateAssetItem<TID>(LegacyAssetItem oldAssetItem)
        {
            AssetItem<TID> assetItem = new AssetItem<TID>();
            assetItem.TypeGuid = oldAssetItem.TypeGuid;
            assetItem.CustomDataOffset = oldAssetItem.CustomDataOffset;
            assetItem.Name = oldAssetItem.Name;
            assetItem.Ext = oldAssetItem.Ext;

            return assetItem;
        }

        public static implicit operator LegacyAssetItem(AssetItem<Guid> assetItem)
        {
            LegacyAssetItem result = CreateAssetItem(assetItem);
            result.ItemGUID = assetItem.ID;
            result.DependenciesGuids = assetItem.DependencyIDs;
            Guid[] embeddedIDs = assetItem.EmbeddedIDs;
            if (embeddedIDs != null)
            {
                result.Parts = new PrefabPart[embeddedIDs.Length];
                for (int p = 0; p < embeddedIDs.Length; ++p)
                {
                    result.Parts[p] = new PrefabPart { PartGUID = embeddedIDs[p] };
                }
            }
            return result;
        }

        public static implicit operator LegacyAssetItem(AssetItem<long> assetItem)
        {
            LegacyAssetItem result = CreateAssetItem(assetItem);
            result.ItemID = assetItem.ID;
            result.Dependencies = assetItem.DependencyIDs;
            long[] embeddedIDs = assetItem.EmbeddedIDs;
            if (embeddedIDs != null)
            {
                result.Parts = new PrefabPart[embeddedIDs.Length];
                for (int p = 0; p < embeddedIDs.Length; ++p)
                {
                    result.Parts[p] = new PrefabPart { PartID = embeddedIDs[p] };
                }
            }
            return result;
        }

        public static implicit operator AssetItem<Guid>(LegacyAssetItem assetItem)
        {
            AssetItem<Guid> result = CreateAssetItem<Guid>(assetItem);
            result.ID = assetItem.ItemGUID;
            result.DependencyIDs = assetItem.DependenciesGuids;
            PrefabPart[] parts = assetItem.Parts;
            if (parts != null)
            {
                result.EmbeddedIDs = new Guid[parts.Length];
                for (int p = 0; p < parts.Length; ++p)
                {
                    result.EmbeddedIDs[p] = parts[p].PartGUID;
                }
            }
            return result;
        }

        public static implicit operator AssetItem<long>(LegacyAssetItem assetItem)
        {
            AssetItem<long> result = CreateAssetItem<long>(assetItem);
            result.ID = assetItem.ItemID;
            result.DependencyIDs = assetItem.Dependencies;
            PrefabPart[] parts = assetItem.Parts;
            if (parts != null)
            {
                result.EmbeddedIDs = new long[parts.Length];
                for (int p = 0; p < parts.Length; ++p)
                {
                    result.EmbeddedIDs[p] = parts[p].PartID;
                }
            }
            return result;
        }
    }


    //[Obsolete]
    /// <summary>
    ///  This class required for compatibility with IProject interface. 
    ///  This is not ProtoContract. 
    ///  Base class for AssetItem<TID>. 
    ///  Will be removed eventually
    /// </summary>
    public abstract class AssetItem : ProjectItem
    {
        public abstract Guid TypeGuid
        {
            get;
            set;
        }

        public abstract long CustomDataOffset
        {
            get;
            set;
        }

        //[Obsolete]
        public abstract PrefabPart[] Parts
        {
            get;
            set;
        }

        //[Obsolete]
        public abstract long[] Dependencies
        {
            get;
            set;
        }


        //[Obsolete]
        public abstract Guid[] DependenciesGuids
        {
            get;
            set;
        }

        //[Obsolete]
        public abstract Preview Preview
        {
            get;
            set;
        }
    }


    /// <summary>
    /// New Asset Item which can be used to store generic ID
    /// </summary>
    /// <typeparam name="TID"></typeparam>
    [ProtoContract]
    public class AssetItem<TID> : AssetItem 
    {
        [ProtoMember(1)]
        public Guid m_typeGuid;
        public override Guid TypeGuid
        {
            get { return m_typeGuid; }
            set { m_typeGuid = value; }
        }
        
        [ProtoMember(2)]
        public TID ID;

        [ProtoMember(3)]
        public TID[] DependencyIDs;

        [ProtoMember(4)] //Replacement for Prefab Parts (2)
        public TID[] EmbeddedIDs;

        [ProtoMember(6, IsRequired = true)]
        public long m_customDataOffset = -1;

        [ProtoMember(7)]
        public int[] AssetLibraryIDs;
        public override long CustomDataOffset
        { 
            get { return m_customDataOffset; }
            set { m_customDataOffset = value; }
        }

        public override bool IsFolder
        {
            get { return false; }
        }

        #region Deprecated. Required for compatibility with IProject interface
        public override PrefabPart[] Parts 
        { 
            get
            {
                if(EmbeddedIDs == null)
                {
                    return null;
                }

                if(this is AssetItem<long>)
                {
                    return EmbeddedIDs.Cast<long>().Select(id => new PrefabPart { PartID = id }).ToArray();
                }

                if(this is AssetItem<Guid>)
                {
                    return EmbeddedIDs.Cast<Guid>().Select(id => new PrefabPart { PartGUID = id }).ToArray();
                }

                return null;
            }
            set
            {
                if(value == null)
                {
                    EmbeddedIDs = null;
                }
                else
                {
                    if (this is AssetItem<long>)
                    {
                        EmbeddedIDs = value.Select(p => p.PartID).Cast<TID>().ToArray();
                    }
                    else if(this is AssetItem<Guid>)
                    {
                        EmbeddedIDs = value.Select(p => p.PartGUID).Cast<TID>().ToArray();
                    }
                }
            }
        }
        public override long[] Dependencies 
        {
            get
            { 
                if(DependencyIDs == null || !(this is AssetItem<long>))
                {
                    return null;
                }
                return DependencyIDs.Cast<long>().ToArray();
            }
            set 
            { 
                if(this is AssetItem<long>)
                {
                    DependencyIDs = value != null ? value.Cast<TID>().ToArray() : null;
                }
            }
        }
        public override Guid[] DependenciesGuids 
        { 
            get
            {
                if(DependencyIDs == null || !(this is AssetItem<Guid>))
                {
                    return null;
                }
                return DependencyIDs.Cast<Guid>().ToArray();
            }
            set
            {
                if (this is AssetItem<Guid>)
                {
                    DependencyIDs = value != null ? value.Cast<TID>().ToArray() : null;
                }
            }
        }
        public override Preview Preview 
        {
            get  { return new Preview { ItemID = ItemID, ItemGUID = ItemGUID, PreviewData = m_preview };  }
            set  { SetPreview(value != null ? value.PreviewData : null); }
        }

        public override long ItemID 
        { 
            get
            {
                if(this is AssetItem<long>)
                {
                    return (long)(object)ID;
                }
                return 0;
            }
            set
            {
                if(this is AssetItem<long>)
                {
                    ID = (TID)(object)value;
                }
            }
        }

        public override Guid ItemGUID
        {
            get
            {
                if (this is AssetItem<Guid>)
                {
                    return (Guid)(object)ID;
                }
                return Guid.Empty;
            }
            set
            {
                if (this is AssetItem<Guid>)
                {
                    ID = (TID)(object)value;
                }
            }
        }
        #endregion

        private byte[] m_preview;

        public AssetItem()
        {
        }

        public AssetItem(Guid typeGuid, TID id, TID[] dependencyIDs = null, TID[] embeddedIDs = null, int[] assetLibraryIDs = default)
        {
            TypeGuid = typeGuid;
            ID = id;
            DependencyIDs = dependencyIDs;
            EmbeddedIDs = embeddedIDs;
            AssetLibraryIDs = assetLibraryIDs;
        }

        public override void SetPreview(byte[] preview)
        {
            if (m_preview != preview)
            {
                m_preview = preview;
                RaisePreviewDataChanged();
            }
        }

        public override byte[] GetPreview()
        {
            return m_preview;
        }

        public override Guid GetTypeGuid()
        {
            return TypeGuid;
        }

        public override void SetTypeGuid(Guid guid)
        {
            TypeGuid = guid;
        }

        public override long GetCustomDataOffset()
        {
            return CustomDataOffset;
        }

        public override void SetCustomDataOffset(long offset)
        {
            CustomDataOffset = offset;
        }

        public override int[] GetAssetLibraryIDs()
        {
            return AssetLibraryIDs;
        }

        public override void SetAssetLibraryIDs(int[] ids)
        {
            AssetLibraryIDs = ids;
        }
    }

    public enum ImportStatus
    {
        None,
        New,
        Conflict,
        Overwrite
    }

    //[Obsolete]
    public class ImportItem : AssetItem<long>
    {
        public UnityObject Object;

        public ImportStatus Status;
    }

    public class ImportAssetItem : ProjectItem
    {
        public UnityObject Object;
        public ImportStatus Status;
        public AssetInfo AssetInfo;
        public int AssetLibraryID;

        public override bool IsFolder
        {
            get { return false; }
        }

        private Guid m_typeGuid;
        private byte[] m_preview;

     
        public override Guid GetTypeGuid()
        {
            return m_typeGuid;
        }

        public override void SetTypeGuid(Guid guid)
        {
            m_typeGuid = guid;
        }

        public override byte[] GetPreview()
        {
            return m_preview;
        }

        public override void SetPreview(byte[] preview)
        {
            m_preview = preview;
            RaisePreviewDataChanged();
        }
    }
}

