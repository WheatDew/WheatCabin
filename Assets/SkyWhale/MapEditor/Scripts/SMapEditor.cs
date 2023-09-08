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
using Unity.VisualScripting;
using JetBrains.Annotations;
using NPOI.SS.Formula.Functions;

public class SMapEditor : MonoBehaviour
{
    #region 系统函数



    #endregion

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
        Debug.Log(PropertyMap.s);
        var data = PropertyMap.s.map["MapEditor"];

        List<Property> childLayers = data.GetDatas("ChildLayer");

        for(int i = 0; i < childLayers.Count; i++)
        {
            Debug.Log(childLayers[i]);
            var t = new GameObject
            {
                name = childLayers[i].GetString()
            };
            t.transform.parent = childLayer;
            childLayerList.Add(childLayers[i].GetString(), t.transform);
        }


        //初始化摄像机位置
        runtimeSceneComponent.cameraPosition = data.GetVector3("MapEditorCameraPosition");
        runtimeSceneComponent.cameraRotation = data.GetQuaternion("MapEditorCameraRotation");
        //初始化资源包列表
        List<Property> assetBundleMaps = data.GetDatas("AssetBundleMap");

        for (int i=0;i<assetBundleMaps.Count;i++)
        {
            if (SAssetBundle.Instance == null)
            {
                Debug.LogError("ab包实例为空");
            }
            assetBundleMap.Add("SkyWhaleEditor", SAssetBundle.Instance.Load(assetBundleMaps[i].GetString()));
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
        var go1 = assetBundleMap["SkyWhaleEditor"].LoadAsset<GameObject>("TestWater");
        Instantiate(go1, childLayerList["Water"]);
    }

    #endregion

    #region 属性转换

    private string mapEditorPropertyName = "MapEditor";
    private string storeElements = "StoreElements";
    private string storeElementKey = "StoreElement";

    #endregion

    #region 拖拽存储资源
    public DragStorePage dragStorePage;


    private UnityEvent<string,GameObject> OnInstantiateObj=new UnityEvent<string, GameObject>();

    public UnityEvent<ExposeToEditor> rteComponentEvent = new UnityEvent<ExposeToEditor>();

    public Sprite defaultSprite;

    public Dictionary<string, GameObject> prefabMap = new Dictionary<string, GameObject>();

    public UnityEvent<Property,GameObject> elementTypeInitEvent = new UnityEvent<Property,GameObject>(); 


    public void InitDragStoreAsset()
    {
        var map = PropertyMap.s.map;
        string packName = "SkyWhaleEditor";

        LoadDragItem(packName);


        dragStorePage.DragEndEvent.AddListener(delegate (Property value)
        {
            var obj = Instantiate(prefabMap[value.GetString(storeElementKey,1)]);

            Debug.Log(obj.name);
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
        foreach (var item in datas.GetDatas(storeElements))
        {

            var data = PropertyMap.s.map[item.GetString()];
            var itemSprite = assetBundle.LoadAsset<Sprite>(data.GetString(storeElementKey,2)) ?? defaultSprite;
            var itemGameObject = assetBundleMap[packName].LoadAsset<GameObject>(data.GetString(storeElementKey,1));
            if (!prefabMap.ContainsKey(data.GetString(storeElementKey, 1)))
            {
                prefabMap.Add(data.GetString(storeElementKey, 1), itemGameObject);
                Debug.Log(data.GetString(storeElementKey, 1));
            }
            dragStorePage.CreateElement(data, itemSprite);
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



