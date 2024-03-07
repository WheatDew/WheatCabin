using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static UnityEngine.UI.Image;

public class EffectSystem : MonoBehaviour
{
    #region 单例主体

    private static EffectSystem _s;
    public static EffectSystem s { get { return _s; } }

    private void Awake()
    {
        if (_s == null)
            _s = this;
    }

    #endregion

    AssetBundle assetBundle;

    public List<string> effectList;
    private Dictionary<string, GameObject> objectList = new Dictionary<string, GameObject>();

    private void Start()
    {
        //string path = "Resource Bundle/AssetBundles/StandaloneWindows/effect.normal";
        //if (File.Exists(path))
        //{
        //    Debug.Log("找到了AssetBundle文件。");
        //}
        //else
        //{
        //    Debug.LogError("在路径上未找到AssetBundle文件: " + path);
        //}

        //// 现有代码加载AssetBundle
        //var myLoadedAssetBundle = AssetBundle.LoadFromFile(path);
        //if (myLoadedAssetBundle == null)
        //{
        //    Debug.LogError("加载AssetBundle失败！");
        //    return;
        //}

        assetBundle = SAssetBundle.Instance.Load("effect.normal");
        for (int i = 0; i < effectList.Count; i++)
        {
            objectList.Add(effectList[i], assetBundle.LoadAsset<GameObject>(effectList[i]));
        }
    }

    public void CreateDirectivityEffect(string objname,Vector3 origin,Vector3 direction,float distence,float time)
    {
        if (direction != Vector3.zero)
        {
            var obj = Instantiate(objectList[objname], origin, Quaternion.identity);
            StartCoroutine(DirectivityEffect(obj, origin, direction, distence, time));
        }
    }

    private IEnumerator DirectivityEffect(GameObject obj,Vector3 origin,Vector3 direction,float distence,float time)
    {
        float timer=0;
        Vector3 target = origin + direction * distence;
        while (timer < time)
        {
            obj.transform.position = Vector3.Lerp(origin, target, timer / time);
            yield return null;
            timer += Time.deltaTime;
        }
        Destroy(obj);
    }

    public GameObject CreateEffect(string objname)
    {
        var obj = Instantiate(objectList[objname]);
        return obj;
    }

}
