using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using System.IO;
using Battlehub.RTHandles;
using System.Windows.Forms;
using SkyWhale;
//using UnityEditor.Callbacks;

public class SMapEditor : MonoBehaviour
{
    public Dictionary<string,Transform> childLayerList=new Dictionary<string, Transform>();
    public Dictionary<string,AssetBundle> assetBundleMap=new Dictionary<string,AssetBundle>();
    public Dictionary<string,GameObject> storeItemMap=new Dictionary<string, GameObject>();
    public Transform childLayer;
    public RuntimeSceneComponent runtimeSceneComponent;
    public MapEditorData currentMapEditorData;

    public void Awake()
    {
        InitMapEditor();
        print("MapEditor Awake");
    }

    private void Start()
    {
        
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            SaveMapEditorData();
        }
    }

    public void SaveMapEditorData()
    {
        currentMapEditorData.CameraPosition = runtimeSceneComponent.cameraTransform.position;
        currentMapEditorData.CameraRotation = runtimeSceneComponent.cameraTransform.rotation;
        File.WriteAllText("Core/MapEditor/Data/MapEditor.json", JsonMapper.ToJson(currentMapEditorData));
        Debug.Log(currentMapEditorData.CameraPosition);
        Debug.Log("����ɹ�");
    }


    public void InitMapEditor()
    {
        //д��
        //MapEditorData data = new();
        //data.ChildLayer.Add("Terrain");
        //data.ChildLayer.Add("Water");
        //data.AssetBundleMap.Add("SkyWhale", "skywhale.editor");
        //File.WriteAllText("Core/MapEditor/Data/MapEditor.json", JsonMapper.ToJson(data));



        currentMapEditorData = JsonMapper.ToObject<MapEditorData>(File.ReadAllText("Core/MapEditor/Data/MapEditor.json"));
        //��ʼ���㼶�б�
        foreach (var p in currentMapEditorData.ChildLayer)
        {
            var t = new GameObject
            {
                name = p
            };
            t.transform.parent = childLayer;
            childLayerList.Add(p, t.transform);
        }
        //��ʼ�������λ��
        runtimeSceneComponent.cameraPosition = currentMapEditorData.CameraPosition;
        runtimeSceneComponent.cameraRotation = currentMapEditorData.CameraRotation;
        //��ʼ����Դ���б�
        foreach(var item in currentMapEditorData.AssetBundleMap)
        {
            if (SAssetBundle.Instance == null)
            {
                Debug.LogError("ab��ʵ��Ϊ��");
            }
            assetBundleMap.Add(item.Key,SAssetBundle.Instance.Load(item.Value));
        }
        //��ʼ����Դ
        InitMapEditorAsset();

        InitDragStoreAsset();
    }

    public void InitMapEditorAsset()
    {
        var go = assetBundleMap["SkyWhaleEditor"].LoadAsset<GameObject>("TestTerrain");
        Instantiate(go, childLayerList["Terrain"]);
        var go1 = assetBundleMap["SkyWhaleEditor"].LoadAsset<GameObject>("TestWater");
        Instantiate(go1, childLayerList["Water"]);
    }

    public void InitDragStoreAsset()
    {
        BuildingList buildingList = JsonMapper.ToObject<BuildingList>(File.ReadAllText("Core/MapEditor/Data/Building.json"));
        for(int i=0;i<buildingList.buildings.Length;i++)
        {
            Debug.Log(buildingList.buildings[i]);
        }
    }
}

public class MapEditorData
{
    public List<string> ChildLayer;
    public Dictionary<string,string> AssetBundleMap;
    public Vector3 CameraPosition;
    public Quaternion CameraRotation;
    public MapEditorData()
    {
        ChildLayer = new List<string>();
        AssetBundleMap = new Dictionary<string, string>();
        CameraPosition = Vector3.zero;
        CameraRotation = Quaternion.identity;
    }
}


