using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

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

    public GameObject SetMainBuff(string buff,Transform target,GameObject lastBuff)
    {
        GameObject currentBuff=null;
        if (target != null)
        {
            if (lastBuff != null)
                Destroy(lastBuff);
            currentBuff = Instantiate(objectList[buff], target.position, Quaternion.identity);
            StartCoroutine(BuffEffect(currentBuff, target, 60));
        }
        return currentBuff;
    }

    public void CreateBuffEffect(string objname, Transform target, float time)
    {
        if (target!=null)
        {
            var obj = Instantiate(objectList[objname], target.position, Quaternion.identity);
            StartCoroutine(BuffEffect(obj,target, time));
        }
    }

    public void CreateBeadEffect(string objname,Vector3 position,float time)
    {
        if (position != Vector3.zero)
        {
            var obj = Instantiate(objectList[objname], position, Quaternion.identity);
            StartCoroutine(BeadEffect(obj, time));
        }
    }

    public void CreateTargetEffect(string objname,Vector3 origin, Transform target,float speed, string collision)
    {
        if (target != null)
        {
            var obj = Instantiate(objectList[objname], origin, Quaternion.identity);
            StartCoroutine(TargetEffect(obj,target, speed,collision));
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

    private IEnumerator TargetEffect(GameObject obj,Transform target,float speed,string collision)
    {
        float distence=999;
        GameObject collisionEffect=null;
        while (distence>0.1f)
        {
            distence = Vector3.Distance(obj.transform.position, target.position+Vector3.up);
            float interval = speed * Time.deltaTime;
            if (interval < distence)
                obj.transform.position = Vector3.Lerp(obj.transform.position, target.position+Vector3.up, interval / distence);
            else
            {
                break;
            }

            yield return null;
        }

        obj.transform.position = target.position;
        collisionEffect = Instantiate(objectList[collision], obj.transform.position, Quaternion.identity);
        Destroy(obj);
        yield return new WaitForSeconds(3);
        if (collisionEffect != null)
            Destroy(collisionEffect);
    }
    private IEnumerator BeadEffect(GameObject obj, float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(obj);
    }

    private IEnumerator BuffEffect(GameObject obj,Transform target,float time)
    {
        float timer = 0;
        while (timer < time)
        {
            if (obj == null)
                break;
            timer += Time.deltaTime;
            obj.transform.position = target.position;
            yield return null;
        }
        if (obj != null)
            Destroy(obj);
    }

    public GameObject CreateEffect(string objname)
    {
        var obj = Instantiate(objectList[objname]);
        return obj;
    }


}
