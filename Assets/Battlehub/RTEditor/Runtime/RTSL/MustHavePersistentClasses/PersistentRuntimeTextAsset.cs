using Battlehub.RTSL.Interface;
using System;

namespace UnityEngine.Battlehub.SL2
{
    //This class is not serializable
    public class PersistentRuntimeTextAsset<TID> : PersistentObject<TID>
    {
        public string Text;
        public string Ext;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);

            RuntimeTextAsset text = (RuntimeTextAsset)obj;
            Text = text.Text;
            Ext = text.Ext;
        }

        protected override object WriteToImpl(object obj)
        {
            RuntimeTextAsset text = (RuntimeTextAsset)base.WriteToImpl(obj);
            text.Text = Text;
            text.Ext = Ext;
            return text;
        }
    }

    [Obsolete("Use generic version")]
    public class PersistentRuntimeTextAsset : PersistentRuntimeTextAsset<long>
    {
    }

}
