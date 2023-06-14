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

public class SMapEditor : MonoBehaviour
{
    #region ����

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
        foreach (var item in currentMapEditorData.AssetBundleMap)
        {
            if (SAssetBundle.Instance == null)
            {
                Debug.LogError("ab��ʵ��Ϊ��");
            }
            assetBundleMap.Add(item.Key, SAssetBundle.Instance.Load(item.Value));
        }
        //��ʼ����Դ
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

    #region ��ק�洢��Դ
    public DragStorePage dragStorePage;
    public Dictionary<string, StoreItem> storeItemMap = new Dictionary<string, StoreItem>();
    public Dictionary<string,Texture> storeImageMap=new Dictionary<string, Texture>();
    public Sprite defaultSprite;

    private UnityEvent<string,GameObject> OnInstantiateObj=new UnityEvent<string, GameObject>();

    public void InitDragStoreAsset()
    {
        string packName = "SkyWhaleEditor",path= "Core/MapEditor/Data/";
        LoadBuildingDragItem(packName, path);
        LoadCharacterDragItem(packName, path);

        dragStorePage.DragEndEvent.AddListener(delegate (string value)
        {

            var obj = Instantiate(storeItemMap[value].gameObject);
            Regex regex = new Regex(@"\([C|c]lone\)$");
            if (regex.IsMatch(obj.name))
            {
                obj.name = obj.name[..^7];
            }
            

            obj.AddComponent<CMapEditorModel>();
            obj.AddComponent<ExposeToEditor>();
            OnInstantiateObj.Invoke(value,obj);



        });
    }


    private void LoadBuildingDragItem(string packName,string path)
    {
        var assetBundle = assetBundleMap[packName];
        var buildingList = JsonMapper.ToObject<BuildingPrefabDataList>(File.ReadAllText(path + "Building.json"));

        foreach (var building in buildingList.buildings)
        {
            var itemSprite = assetBundle.LoadAsset<Sprite>(building.name + "��ͼ") ?? defaultSprite;
            var itemGameObject = assetBundleMap[packName].LoadAsset<GameObject>(building.name);
            storeItemMap.Add(building.name, new StoreItem(building.name, itemGameObject, itemSprite,"Building",building.detailType));
            dragStorePage.CreateElement(building.name, storeItemMap[building.name].sprite);

        }

        OnInstantiateObj.AddListener(delegate(string value,GameObject obj)
        {
            if (storeItemMap[value].type == "Building")
            {
                var cobj = obj.AddComponent<NormalObject>();
                cobj.type = "Building";
            }
        });

    }

    public void SetProperty(string type,string detailType,GameObject obj)
    {
        Debug.LogFormat("��������{0} {1}", type, detailType);

        if (type == "Building")
        {
            var cobj = obj.AddComponent<NormalObject>();
            cobj.type = "Building";
        }
        if (type == "Character")
        {
            var cobj = obj.AddComponent<NormalObject>();
            cobj.type = "Character";
            if (detailType == "Player") {
                SPlayer.s.currentPlayer = obj;
                cobj.detailType = "Player";
            }


        }
    }

    private void LoadCharacterDragItem(string packName,string path)
    {
        var assetBundle = assetBundleMap[packName];
        var characterList = JsonMapper.ToObject<CharacterPrefabDataList>(File.ReadAllText(path + "Character.json"));

        foreach (var character in characterList.characters)
        {
            var itemSprite = assetBundle.LoadAsset<Sprite>(character.name + "��ͼ") ?? defaultSprite;
            var itemGameObject = assetBundleMap[packName].LoadAsset<GameObject>(character.name);
            storeItemMap.Add(character.name, new StoreItem(character.name, itemGameObject, itemSprite,"Character",character.detailType));
            dragStorePage.CreateElement(character.name, storeItemMap[character.name].sprite);
        }

        OnInstantiateObj.AddListener(delegate (string value, GameObject obj)
        {

            if (storeItemMap[value].type == "Character")
            {
                var cobj = obj.AddComponent<NormalObject>();
                cobj.type = "Character";
                if (storeItemMap[value].type == "Player")
                {
                    SPlayer.s.currentPlayer = obj;
                    cobj.detailType = "Player";
                }


            }
        });
    }

    #endregion

    #region ��������

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
        Debug.Log("д���ļ��ɹ�");
    }

    public void LoadScene()
    {
        var result= File.ReadAllText("Core/MapEditor/SaveData/test.fzy");
        
    }


    #endregion

}

#region ����
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

public class EditorItem
{
    public Vector3 position;
    public Vector3 rotation;
}
#endregion


