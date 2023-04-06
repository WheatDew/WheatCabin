using UnityEngine;
//using UnityEditor;
using System;
using System.Linq;

namespace Battlehub.RTSL
{
    [Serializable]
    public class FilePathRecord
    {
        public string PeristentTypeName;

        public UnityEngine.Object File;
    }

    public class FilePathStorage : MonoBehaviour
    {
        public FilePathRecord[] PathRecords;

        //public string FromType(Type type)
        //{
        //    FilePathRecord pathRecord = PathRecords.Where(r => r.PeristentTypeName == type.FullName).FirstOrDefault();
        //    if(pathRecord != null && pathRecord.File != null)
        //    {
        //        return AssetDatabase.GetAssetPath(pathRecord.File);
        //    }
        //    return null;
        //}
    }

}
