using System;
using System.Collections.Generic;
using UnityEngine;

namespace Battlehub.RTSL
{
    [Serializable]
    public struct AssetLibraryListEntry
    {
        public string Library;
        public int Ordinal;
    }

    public class AssetLibrariesListAsset : ScriptableObject
    {
        [SerializeField]
        public int Identity;

        [SerializeField]
        public List<AssetLibraryListEntry> List;
    }
}

