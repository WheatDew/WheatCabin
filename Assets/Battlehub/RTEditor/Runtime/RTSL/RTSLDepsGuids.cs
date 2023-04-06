using Battlehub.RTSL.Interface;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Battlehub.RTSL
{
    [DefaultExecutionOrder(-100)]
    [RequireComponent(typeof(RTSLIgnore))]
    public class RTSLDepsGuids : RTSLDeps<Guid>
    {
        private IAssetDB<Guid> m_assetDB;
        protected override IAssetDB<Guid> AssetDB
        {
            get
            {
                if (m_assetDB == null)
                {
                    m_assetDB = new AssetDB<Guid>(obj => Guid.NewGuid());
                }
                return m_assetDB;
            }
        }

        private ProjectAsyncImpl<Guid> m_projectAsync;
        protected override IProjectAsync ProjectAsync
        {
            get
            {
                if (m_projectAsync == null)
                {
                    m_projectAsync = new ProjectAsyncImpl<Guid> ();

                }
                return m_projectAsync;
            }
        }

        private IIDGenerator<Guid> m_idGen;
        protected override IIDGenerator<Guid> IDGen
        {
            get
            {
                if (m_idGen == null)
                {
                    m_idGen = new GuidGenerator();
                }
                return m_idGen;
            }
        }
    }

    public class GuidGenerator : IIDGenerator<Guid>
    {
        public async Task<Guid[]> GenerateAsync(int count, CancellationToken ct)
        {
            await Task.Yield();
            ct.ThrowIfCancellationRequested();

            Guid[] ids = new Guid[count];
            for (int i = 0; i < ids.Length; ++i)
            {
                ids[i] = Guid.NewGuid();
            }
            return ids;
        }
    }

}
