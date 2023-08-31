using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class SAssetBundle : MonoBehaviour
{
    private Dictionary<string, AssetBundle> abCache;
    private AssetBundle mainAB = null; //����
    private AssetBundleManifest mainManifest = null; //�����������ļ�---���Ի�ȡ������

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
        
        //����manifest��ȡ���������������� �̶�API
        string[] dependencies = mainManifest.GetAllDependencies(abName);
        //ѭ����������������
        for (int i = 0; i < dependencies.Length; i++)
        {
            //������ڻ��������
            if (!abCache.ContainsKey(dependencies[i]))
            {
                //�������������ƽ��м���
                ab = AssetBundle.LoadFromFile(basePath + dependencies[i]);
                //ע����ӽ����� ��ֹ�ظ�����AB��
                abCache.Add(dependencies[i], ab);
            }
        }
        //����Ŀ��� -- ͬ��ע�⻺������
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

    //====================AB��������ж�ط�ʽ=================
    //������ж��
    public void UnLoad(string abName)
    {
        if (abCache.ContainsKey(abName))
        {
            abCache[abName].Unload(false);
            //ע�⻺����һ���Ƴ�
            abCache.Remove(abName);
        }
    }

    //���а�ж��
    public void UnLoadAll()
    {
        AssetBundle.UnloadAllAssetBundles(false);
        //ע����ջ���
        abCache.Clear();
        mainAB = null;
        mainManifest = null;
    }
}
