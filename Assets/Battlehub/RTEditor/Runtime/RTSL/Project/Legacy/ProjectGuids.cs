using Battlehub.RTCommon;
using Battlehub.RTSL.Interface;
using System;

namespace Battlehub.RTSL
{
    public interface IGuidGenerator
    {
        void Generate(int count, Action<Error, Guid[]> callback);
    }

    [Obsolete]
    public class ProjectGuids : Project<Guid>, IProject
    {
        private class DefaultGuidGenerator : IGuidGenerator
        {
            public void Generate(int count, Action<Error, Guid[]> callback)
            {
                Guid[] result = new Guid[count];
                for (int i = 0; i < count; ++i)
                {
                    result[i] = Guid.NewGuid();
                }

                callback(Error.NoError, result);
            }
        }

        private IGuidGenerator m_guidGen;

        public override void Awake_Internal()
        {
            base.Awake_Internal();

            m_guidGen = IOC.Resolve<IGuidGenerator>();
            if(m_guidGen == null)
            {
                m_guidGen = new DefaultGuidGenerator();
            }
        }

        protected override void GenerateIdentifiers(int count, Action<Error, Guid[]> callback)
        {
            m_guidGen.Generate(count, callback);
        }

        protected override Guid[] GetDependencies(AssetItem assetItem)
        {
            return assetItem.DependenciesGuids;
        }

        protected override Guid GetID(ProjectItem projectItem)
        {
            return projectItem.ItemGUID;
        }

        protected override Guid GetID(PrefabPart prefabPart)
        {
            return prefabPart.PartGUID;
        }

        protected override Guid GetID(Preview preview)
        {
            return preview.ItemGUID;
        }

        protected override bool IsEqual(Guid id1, Guid id2)
        {
            return id1 == id2;
        }

        protected override void SetDependencies(AssetItem assetItem, Guid[] dependencies)
        {
            assetItem.DependenciesGuids = dependencies;
        }

        protected override void SetID(ProjectItem projectItem, Guid id)
        {
            projectItem.ItemGUID = id;
        }

        protected override void SetID(PrefabPart prefabPart, Guid id)
        {
            prefabPart.PartGUID = id;
        }

        protected override void SetParentID(PrefabPart prefabPart, Guid id)
        {
            prefabPart.ParentGUID = id;
        }

        protected override void SetID(Preview preview, Guid id)
        {
            preview.ItemGUID = id;
        }
    }
        
}