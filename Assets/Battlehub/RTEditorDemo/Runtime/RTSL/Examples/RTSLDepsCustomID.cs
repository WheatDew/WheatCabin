using Battlehub.RTSL.Demo;
using Battlehub.RTSL.Interface;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Battlehub.RTSL
{
    [DefaultExecutionOrder(-100)]
    [RequireComponent(typeof(RTSLIgnore))]
    public class RTSLDepsCustomID : RTSLDeps<CustomID>
    {
        private IAssetDB<CustomID> m_assetDB;
        protected override IAssetDB<CustomID> AssetDB
        {
            get
            {
                if (m_assetDB == null)
                {
                    m_assetDB = new AssetDB<CustomID>(obj => CustomID.NewID());
                }
                return m_assetDB;
            }
        }

        private IProjectAsync m_projectAsync;
        protected override IProjectAsync ProjectAsync
        {
            get
            {
                if (m_projectAsync == null)
                {
                    m_projectAsync = new ProjectAsyncWithAssetLibraries<CustomID>();
                }
                return m_projectAsync;
            }
        }

        private IStorageAsync<CustomID> m_storageAsync;
        protected override IStorageAsync<CustomID> StorageAsync
        {
            get
            {
                if (m_storageAsync == null)
                {
                    m_storageAsync = new FileSystemStorageAsync<CustomID>();
                }
                return m_storageAsync;
            }
        }

        private IIDGenerator<CustomID> m_idGen;
        protected override IIDGenerator<CustomID> IDGen
        {
            get
            {
                if(m_idGen == null)
                {
                    m_idGen = new CustomIDGenerator();
                }
                return m_idGen;
            }
        }
    }

    public class CustomIDGenerator : IIDGenerator<CustomID>
    {
        public async Task<CustomID[]> GenerateAsync(int count, CancellationToken ct)
        {
            await Task.Yield();
            ct.ThrowIfCancellationRequested();

            CustomID[] ids = new CustomID[count];
            for (int i = 0; i < ids.Length; ++i)
            {
                ids[i] = CustomID.NewID();
            }
            return ids;
        }
    }
}

