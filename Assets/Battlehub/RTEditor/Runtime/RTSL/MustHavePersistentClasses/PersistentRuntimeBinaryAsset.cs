using Battlehub.RTSL.Interface;
using System;

namespace UnityEngine.Battlehub.SL2
{
    //This class is not serializable
    public class PersistentRuntimeBinaryAsset<TID> : PersistentObject<TID>
    {
        public byte[] Data;
        public string Ext;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);

            RuntimeBinaryAsset bin = (RuntimeBinaryAsset)obj;
            Data = bin.Data;
            Ext = bin.Ext;
        }

        protected override object WriteToImpl(object obj)
        {
            RuntimeBinaryAsset bin = (RuntimeBinaryAsset)base.WriteToImpl(obj);
            bin.Data = Data;
            bin.Ext = Ext;
            return bin;
        }
    }

    [Obsolete("Use generic version")]
    public class PersistentRuntimeBinaryAsset : PersistentRuntimeBinaryAsset<long>
    { }
}
