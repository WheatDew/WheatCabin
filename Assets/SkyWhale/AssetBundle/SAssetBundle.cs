using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class SAssetBundle : MonoBehaviour
{
    private Dictionary<string, AssetBundle> abCache;
    private AssetBundle mainAB = null; //主包
    private AssetBundleManifest mainManifest = null; //主包中配置文件---用以获取依赖包

    public string basePath= "Resource Bundle/AssetBundles/StandaloneWindows/";

    private static SAssetBundle instance;
    public static SAssetBundle Instance { get { return instance; } }

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }

        if (mainAB == null)
        {
            abCache = new Dictionary<string, AssetBundle>();
            mainAB = AssetBundle.LoadFromFile(basePath + "StandaloneWindows");
            mainManifest = mainAB.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        }

        print("SAssetBundle Awake");
    }

    public AssetBundle Load(string abName)
    {
        AssetBundle ab;
        
        //根据manifest获取所有依赖包的名称 固定API
        string[] dependencies = mainManifest.GetAllDependencies(abName);
        //循环加载所有依赖包
        for (int i = 0; i < dependencies.Length; i++)
        {
            //如果不在缓存则加入
            if (!abCache.ContainsKey(dependencies[i]))
            {
                //根据依赖包名称进行加载
                ab = AssetBundle.LoadFromFile(basePath + dependencies[i]);
                //注意添加进缓存 防止重复加载AB包
                abCache.Add(dependencies[i], ab);
            }
        }
        //加载目标包 -- 同理注意缓存问题
        if (abCache.ContainsKey(abName))
        {
            Debug.Log($"have load {abName}");
            return abCache[abName];
        }
        else
        {
            ab = AssetBundle.LoadFromFile(basePath + abName);
            abCache.Add(abName, ab);
            Debug.Log($"new load {abName}");
            return ab;
        }
    }


    private IEnumerator LoadResAsyncTest(AssetBundle ab)
    {
        if (ab == null) yield return null;
        var model1 = ab.LoadAssetAsync<GameObject>("Cube");
        yield return model1;
        var async_model = Instantiate((GameObject)model1.asset);
        // dosomething
    }

    //====================AB包的两种卸载方式=================
    //单个包卸载
    public void UnLoad(string abName)
    {
        if (abCache.ContainsKey(abName))
        {
            abCache[abName].Unload(false);
            //注意缓存需一并移除
            abCache.Remove(abName);
        }
    }

    //所有包卸载
    public void UnLoadAll()
    {
        AssetBundle.UnloadAllAssetBundles(false);
        //注意清空缓存
        abCache.Clear();
        mainAB = null;
        mainManifest = null;
    }
}
