using LitJson;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class NormalObjectSystem : MonoBehaviour
{
    public List<WDEntity> objects;
    public string saveDataPath;
    public Dictionary<string,WDEntity> normalObjects;
    public string packName;
    public AssetBundle assetBundle;

    public virtual void LoadPrefab()
    {
        assetBundle = SAssetBundle.Instance.Load(packName);

    }

    public virtual void SaveScene()
    {
        string result=JsonMapper.ToJson(objects);
        File.WriteAllText(saveDataPath, result);
    }

    public virtual void ReadScene()
    {
        string result = File.ReadAllText(saveDataPath);
        objects = JsonMapper.ToObject<List<WDEntity>>(result);
    }
}
