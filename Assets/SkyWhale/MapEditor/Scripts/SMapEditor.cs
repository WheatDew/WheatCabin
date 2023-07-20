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

public class SMapEditor : MonoBehaviour
{
    #region 主体

    public Dictionary<string, Transform> childLayerList = new Dictionary<string, Transform>();
    public Dictionary<string, AssetBundle> assetBundleMap = new Dictionary<string, AssetBundle>();


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
        //if (Input.GetKeyDown(KeyCode.S))
        //{
        //    SaveMapEditorData();
        //}
    }

    public void SaveMapEditorData()
    {
        currentMapEditorData.CameraPosition = runtimeSceneComponent.cameraTransform.position;
        currentMapEditorData.CameraRotation = runtimeSceneComponent.cameraTransform.rotation;
        File.WriteAllText("Core/MapEditor/Data/MapEditor.json", JsonMapper.ToJson(currentMapEditorData));
        Debug.Log(currentMapEditorData.CameraPosition);
        Debug.Log("保存成功");
    }


    public void InitMapEditor()
    {
        //写入
        //MapEditorData data = new();
        //data.ChildLayer.Add("Terrain");
        //data.ChildLayer.Add("Water");
        //data.AssetBundleMap.Add("SkyWhale", "skywhale.editor");
        //File.WriteAllText("Core/MapEditor/Data/MapEditor.json", JsonMapper.ToJson(data));



        currentMapEditorData = JsonMapper.ToObject<MapEditorData>(File.ReadAllText("Core/MapEditor/Data/MapEditor.json"));
        //初始化层级列表
        foreach (var p in currentMapEditorData.ChildLayer)
        {
            var t = new GameObject
            {
                name = p
            };
            t.transform.parent = childLayer;
            childLayerList.Add(p, t.transform);
        }
        //初始化摄像机位置
        runtimeSceneComponent.cameraPosition = currentMapEditorData.CameraPosition;
        runtimeSceneComponent.cameraRotation = currentMapEditorData.CameraRotation;
        //初始化资源包列表
        foreach (var item in currentMapEditorData.AssetBundleMap)
        {
            if (SAssetBundle.Instance == null)
            {
                Debug.LogError("ab包实例为空");
            }
            assetBundleMap.Add(item.Key, SAssetBundle.Instance.Load(item.Value));
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
    public Dictionary<string, StoreItem> storeItemMap = new Dictionary<string, StoreItem>();
    public Dictionary<string,Texture> storeImageMap=new Dictionary<string, Texture>();
    public Sprite defaultSprite;

    private UnityEvent<string,GameObject> OnInstantiateObj=new UnityEvent<string, GameObject>();

    public UnityEvent<ExposeToEditor> rteComponentEvent = new UnityEvent<ExposeToEditor>();
    public UnityEvent<StoreItem, GameObject> mapEditorModelEvent = new UnityEvent<StoreItem, GameObject>();

    public void InitDragStoreAsset()
    {
        string packName = "SkyWhaleEditor", path = "Core/MapEditor/Data";

        string[] files = Directory.GetFiles(path, "*ItemData.json", SearchOption.AllDirectories);
        for (int i = 0; i < files.Length; i++)
        {
            LoadDragItem(packName,files[i]);
        }


        dragStorePage.DragEndEvent.AddListener(delegate (string value)
        {

            var obj = Instantiate(storeItemMap[value].gameObject);
            Regex regex = new Regex(@"\([C|c]lone\)$");
            if (regex.IsMatch(obj.name))
            {
                obj.name = obj.name[..^7];
            }
            

            obj.AddComponent<CMapEditorModel>();
            var rteComponent = obj.AddComponent<ExposeToEditor>();
            mapEditorModelEvent.Invoke(storeItemMap[value], obj);
            rteComponentEvent.Invoke(rteComponent);
            //SBuilding.s.InitBuilding(storeItemMap[value], obj);
            //SCharacter.s.InitCharacter(storeItemMap[value], obj);
            //Debug.Log(SBuilding.s.name);


        });
    }




    private void LoadDragItem(string packName,string path)
    {
        var assetBundle = assetBundleMap[packName];
        var mapItemList = JsonMapper.ToObject<PrefabDataList>(File.ReadAllText(path));

        foreach (var mapItem in mapItemList.prefabDatas)
        {
            var itemSprite = assetBundle.LoadAsset<Sprite>(mapItem.name + "贴图") ?? defaultSprite;
            var itemGameObject = assetBundleMap[packName].LoadAsset<GameObject>(mapItem.name);
            storeItemMap.Add(mapItem.name, new StoreItem(mapItem.name, itemGameObject, itemSprite,mapItem.type, mapItem.detailType));
            dragStorePage.CreateElement(mapItem.name, storeItemMap[mapItem.name].sprite);
        }

        
    }

    #endregion

    #region 场景功能

    public static HashSet<NormalObject> objMap=new HashSet<NormalObject>();
    public static Dictionary<string, UnityAction<string>> funMap = new Dictionary<string, UnityAction<string>>();

    public void SaveScene()
    {
        List<NormalData> savedata=new();
        foreach(var item in objMap)
        {
            savedata.Add(new NormalData(item.type, JsonMapper.ToJson(new SimpleData(item.transform.position, item.transform.eulerAngles))));
        }

        File.WriteAllText("Core/MapEditor/SaveData/test.fzy", JsonMapper.ToJson(savedata));
        Debug.Log("写入文件成功");
    }

    public void LoadScene()
    {
        var result= File.ReadAllText("Core/MapEditor/SaveData/test.fzy");
        
    }


    #endregion

}

#region 类型
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

public class NormalData
{
    public string type;
    public string data;

    public NormalData(string type,string data)
    {
        this.type = type;
        this.data = data;
    }
}
public class SimpleData
{
    public Vector3 position;
    public Vector3 rotation;

    public SimpleData(Vector3 position,Vector3 rotation)
    {
        this.rotation = rotation;
        this.position = position;
    }
}


public class StoreItem
{
    public string name;
    public GameObject gameObject;
    public Sprite sprite;
    public string type;
    public string detailType;

    public StoreItem(string name, GameObject gameObject,Sprite sprite, string type,string detailType)
    {
        this.name = name;
        this.gameObject = gameObject;
        this.sprite = sprite;
        this.type = type;
        this.detailType = detailType;
    }

}

public class PrefabData
{
    public string name;
    public string type;
    public string detailType;
}

public class PrefabDataList
{
    public PrefabData[] prefabDatas;
}

public class EditorItem
{
    public Vector3 position;
    public Vector3 rotation;
}
#endregion


