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

public class SMapEditor : MonoBehaviour
{
    #region 系统函数



    #endregion

    #region 主体

    public Dictionary<string, Transform> childLayerList = new Dictionary<string, Transform>();
    public Dictionary<string, AssetBundle> assetBundleMap = new Dictionary<string, AssetBundle>();


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

        List<string> childLayers = data.GetStrings("ChildLayer");

        for(int i = 0; i < childLayers.Count; i++)
        {
            Debug.Log(childLayers[i]);
            var t = new GameObject
            {
                name = childLayers[i]
            };
            t.transform.parent = childLayer;
            childLayerList.Add(childLayers[i], t.transform);
        }


        //初始化摄像机位置
        runtimeSceneComponent.cameraPosition = data.GetVector3("MapEditorCameraPosition");
        runtimeSceneComponent.cameraRotation = data.GetQuaternion("MapEditorCameraRotation");
        //初始化资源包列表
        List<string> assetBundleMaps = data.GetStrings("AssetBundleMap");

        for (int i=0;i<assetBundleMaps.Count;i++)
        {
            if (SAssetBundle.Instance == null)
            {
                Debug.LogError("ab包实例为空");
            }
            assetBundleMap.Add("SkyWhaleEditor", SAssetBundle.Instance.Load(assetBundleMaps[i]));
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

    private string storeElementIconName = "StoreElementIconName";
    private string mapEditorPropertyName = "MapEditor";
    private string storeElements = "StoreElements";
    private string storeElementPrefabName = "StoreElementPrefabName";


    #endregion

    #region 拖拽存储资源
    public DragStorePage dragStorePage;


    private UnityEvent<string,GameObject> OnInstantiateObj=new UnityEvent<string, GameObject>();

    public UnityEvent<ExposeToEditor> rteComponentEvent = new UnityEvent<ExposeToEditor>();

    public Sprite defaultSprite;

    public Dictionary<string, GameObject> prefabMap = new Dictionary<string, GameObject>();

    public UnityEvent<PropertyData,GameObject> elementTypeInitEvent = new UnityEvent<PropertyData,GameObject>(); 


    public void InitDragStoreAsset()
    {
        var map = PropertyMap.s.map;
        string packName = "SkyWhaleEditor";

        LoadDragItem(packName);


        dragStorePage.DragEndEvent.AddListener(delegate (PropertyData value)
        {
            var obj = Instantiate(prefabMap[value.GetString(storeElementPrefabName)]);


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
        foreach (var item in datas.GetStrings(storeElements))
        {

            var data = PropertyMap.s.map[item];
            var itemSprite = assetBundle.LoadAsset<Sprite>(data.GetString(storeElementIconName)) ?? defaultSprite;
            var itemGameObject = assetBundleMap[packName].LoadAsset<GameObject>(data.GetString(storeElementPrefabName));
            if (!prefabMap.ContainsKey(data.GetString(storeElementPrefabName)))
            {
                prefabMap.Add(data.GetString(storeElementPrefabName), itemGameObject);
                Debug.Log(data.GetString(storeElementPrefabName));
            }
            dragStorePage.CreateElement(data, itemSprite);
        }
    }

    #endregion


}



