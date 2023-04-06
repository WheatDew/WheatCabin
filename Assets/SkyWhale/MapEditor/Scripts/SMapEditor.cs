using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using System.IO;
using Battlehub.RTHandles;
//using UnityEditor.Callbacks;

public class SMapEditor : MonoBehaviour
{
    public Dictionary<string,Transform> childLayerList=new Dictionary<string, Transform>();
    public Dictionary<string,string> abpackList=new Dictionary<string,string>();
    public Transform childLayer;
    public RuntimeSceneComponent runtimeSceneComponent;
    public MapEditorData currentMapEditorData;

    public void Awake()
    {
        InitMapEditor();
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
        foreach(var item in currentMapEditorData.ABPackList)
        {
            abpackList.Add(item.name,item.path);
        }
        //��ʼ����Դ
        InitMapEditorAsset();
    }

    public void InitMapEditorAsset()
    {
        AssetBundle ab = AssetBundle.LoadFromFile("AssetBundles/StandaloneWindows/mapeditor.normal");
        var go = ab.LoadAsset<GameObject>("Terrain");
        Instantiate(go, childLayerList["Terrain"]);
        var go1 = ab.LoadAsset<GameObject>("Water");
        Instantiate(go1, childLayerList["Water"]);
    }
}

public class MapEditorData
{
    public List<string> ChildLayer;
    public List<ABPack> ABPackList;
    public Vector3 CameraPosition;
    public Quaternion CameraRotation;
    public MapEditorData()
    {
        ChildLayer = new List<string>();
        ABPackList = new List<ABPack>();
        CameraPosition = Vector3.zero;
        CameraRotation = Quaternion.identity;
    }
}

public class ABPack
{
    public string name, path;
}

