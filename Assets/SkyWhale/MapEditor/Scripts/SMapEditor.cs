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
        var data = PropertyMap.s.map["MapEditor"];

        string[] childLayers = data.s["ChildLayer"].Split(',');

        for(int i = 0; i < childLayers.Length; i++)
        {
            var t = new GameObject
            {
                name = childLayers[i]
            };
            t.transform.parent = childLayer;
            childLayerList.Add(childLayers[i], t.transform);
        }


        //初始化摄像机位置
        runtimeSceneComponent.cameraPosition = new Vector3(data.f["MapEditorCameraPositionX"], data.f["MapEditorCameraPositionY"], data.f["MapEditorCameraPositionZ"]);
        runtimeSceneComponent.cameraRotation = new Quaternion(data.f["MapEditorCameraRotationX"], data.f["MapEditorCameraRotationY"], data.f["MapEditorCameraRotationZ"],data.f["MapEditorCameraRotationW"]);
        //初始化资源包列表
        string[] assetBundleMaps = data.s["AssetBundleMap"].Split(',');

        for (int i=0;i<assetBundleMaps.Length;i++)
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

    #region 拖拽存储资源
    public DragStorePage dragStorePage;


    private UnityEvent<string,GameObject> OnInstantiateObj=new UnityEvent<string, GameObject>();

    public UnityEvent<ExposeToEditor> rteComponentEvent = new UnityEvent<ExposeToEditor>();

    public Sprite defaultSprite;

    public void InitDragStoreAsset()
    {
        var map = PropertyMap.s.map;
        string packName = "SkyWhaleEditor";

        //foreach (var item in map)
        //{
            
        //    LoadDragItem(packName, files[i]);
        //}


        //dragStorePage.DragEndEvent.AddListener(delegate (string value)
        //{

        //    var obj = Instantiate(storeItemMap[value].gameObject);
        //    Regex regex = new Regex(@"\([C|c]lone\)$");
        //    if (regex.IsMatch(obj.name))
        //    {
        //        obj.name = obj.name[..^7];
        //    }


        //    obj.AddComponent<CMapEditorModel>();
        //    var rteComponent = obj.AddComponent<ExposeToEditor>();
        //    mapEditorModelEvent.Invoke(storeItemMap[value], obj);
        //    rteComponentEvent.Invoke(rteComponent);

        //});
    }



    private void LoadDragItem(string packName, Dictionary<string,PropertyData> datas)
    {
        var assetBundle = assetBundleMap[packName];

        foreach(var item in datas)
        {
            var data = item.Value;
            var itemSprite = assetBundle.LoadAsset<Sprite>(data.s["storeIconName"]) ?? defaultSprite;
            var itemGameObject = assetBundleMap[packName].LoadAsset<GameObject>(data.s["prefabName"]);
            dragStorePage.CreateElement(data.s["storeName"], itemSprite);
        }



    }

    #endregion


}



