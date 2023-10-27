using System.Collections.Generic;
using UnityEngine;
using LitJson;
using System.IO;
using Battlehub.RTHandles;
using SkyWhale;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Windows.Forms;
using Battlehub.RTCommon;
using Microsoft.Win32.SafeHandles;
using System.Text.RegularExpressions;
using static UnityEngine.Rendering.DebugUI;
using JetBrains.Annotations;
using NPOI.SS.Formula.Functions;

public class SMapEditor : MonoBehaviour
{

    #region 主体

    public static Dictionary<string, Transform> childLayerList = new Dictionary<string, Transform>();
    public static Dictionary<string, AssetBundle> assetBundleMap = new Dictionary<string, AssetBundle>();


    public Transform childLayer;
    public RuntimeSceneComponent runtimeSceneComponent;


    public void Awake()
    {
        InitMapEditor();
        print("MapEditor Awake");
    }



    public void InitMapEditor()
    {
        //Debug.Log(PropertyMap.s);
        var data = PropertyMap.s.map["MapEditor"];

        List<INya> childLayers = data.Map["ChildLayer"].List;

        for(int i = 0; i < childLayers.Count; i++)
        {
            //Debug.Log(childLayers[i]);
            var t = new GameObject
            {
                name = childLayers[i].String
            };
            t.transform.parent = childLayer;
            childLayerList.Add(childLayers[i].String, t.transform);
        }


        //初始化摄像机位置
        runtimeSceneComponent.cameraPosition = data.Map["MapEditorCameraPosition"].Vector3;
        runtimeSceneComponent.cameraRotation = data.Map["MapEditorCameraRotation"].Quaternion;
        //初始化资源包列表
        List<INya> assetBundleMaps = data.Map["AssetBundleMap"].List;

        for (int i=0;i<assetBundleMaps.Count;i++)
        {
            if (SAssetBundle.Instance == null)
            {
                Debug.LogError("ab包实例为空");
            }
            assetBundleMap.Add("SkyWhaleEditor", SAssetBundle.Instance.Load(assetBundleMaps[i].String));
        }
        //初始化资源
        InitMapEditorAsset();

        InitDragStoreAsset();
    }

    public void InitMapEditorAsset()
    {
        var terrainPrefab = assetBundleMap["SkyWhaleEditor"].LoadAsset<GameObject>("TestTerrain");
        var terrain = Instantiate(terrainPrefab, childLayerList["Terrain"]);
        terrain.layer = LayerMask.NameToLayer("Ground");
        terrain.AddComponent<NyaTerrain>();

        var go1 = assetBundleMap["SkyWhaleEditor"].LoadAsset<GameObject>("TestWater");
        Instantiate(go1, childLayerList["Water"]);
    }

    #endregion

    #region 属性转换

    [HideInInspector] public static string mapEditorPropertyName = "MapEditor";
    [HideInInspector] public static string storeElements = "StoreElements";
    [HideInInspector] public static string storeElementKey = "StoreElement";



    #endregion

    #region 拖拽存储资源
    public DragStorePage dragStorePage;


    private UnityEvent<string,GameObject> OnInstantiateObj=new UnityEvent<string, GameObject>();

    public UnityEvent<ExposeToEditor> rteComponentEvent = new UnityEvent<ExposeToEditor>();

    public Sprite defaultSprite;

    public Dictionary<string, GameObject> prefabMap = new Dictionary<string, GameObject>();

    public UnityEvent<INya,GameObject> elementTypeInitEvent = new UnityEvent<INya,GameObject>(); 


    public void InitDragStoreAsset()
    {
        var map = PropertyMap.s.map;
        string packName = "SkyWhaleEditor";

        LoadDragItem(packName);


        dragStorePage.DragEndEvent.AddListener(delegate (INya value)
        {
            var obj = Instantiate(prefabMap[value.Get(storeElementKey,1).String]);

            obj.AddComponent<CMapEditorModel>();
            var rteComponent = obj.AddComponent<ExposeToEditor>();
            rteComponentEvent.Invoke(rteComponent);
            elementTypeInitEvent.Invoke(value,obj);
        });
    }



    private void LoadDragItem(string packName)
    {
        var assetBundle = assetBundleMap[packName];
        var datas = PropertyMap.s.map[mapEditorPropertyName];
        if (datas.Map.ContainsKey(storeElements))
        {
            foreach (var item in datas.Map[storeElements].List)
            {

                var data = PropertyMap.s.map[item.String];
                var itemSprite = assetBundle.LoadAsset<Sprite>(data.Get(storeElementKey, 2).String) ?? defaultSprite;
                var itemGameObject = assetBundleMap[packName].LoadAsset<GameObject>(data.Get(storeElementKey, 1).String);
                if (!prefabMap.ContainsKey(data.Get(storeElementKey, 1).String))
                {
                    prefabMap.Add(data.Get(storeElementKey, 1).String, itemGameObject);
                    //Debug.Log(data.GetString(storeElementKey, 1));
                }
                dragStorePage.CreateElement(data, itemSprite);
            }
        }

    }


    #endregion

    public static T GetAssetBundleElement<T>(string packName,string objName) where T:Object
    {
        return assetBundleMap[packName].LoadAsset<T>(objName);
    }

    public static GameObject GetAssetBundleElement(string packName, string objName)
    {
        return assetBundleMap[packName].LoadAsset<GameObject>(objName);
    }
}



