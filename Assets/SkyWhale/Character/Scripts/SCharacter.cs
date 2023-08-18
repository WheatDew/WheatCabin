using LitJson;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine;

public class SCharacter : MonoBehaviour
{
    #region 单例代码
    private static SCharacter _s;
    public static SCharacter s { get { return _s; } }

    private void Awake()
    {
        if (!_s) _s = this;

        InitOriginPropertyData();
        mapEditor.mapEditorModelEvent.AddListener(InitCharacter);
    }
    #endregion

    #region 变量

    public Dictionary<string, PropertyData> originPropertyDatas = new Dictionary<string, PropertyData>();

    #endregion

    #region 系统函数
    private void Start()
    {


    }
    #endregion

    public void WriteOriginPropertyData()
    {
        Dictionary<string, int> intData = new Dictionary<string, int> { { "复活次数", 10 } };
        Dictionary<string, float> floatData = new Dictionary<string, float> { { "健康值", 1.6f } };
        Dictionary<string, string> stringData = new Dictionary<string, string> { { "名字", "白元元" } };

        SCharacterData data = new SCharacterData("白元元", intData, floatData, stringData);
        string s = JsonMapper.ToJson(data);

        s = Regex.Replace(s, @"\\u(?<Value>[a-zA-Z0-9]{4})", m =>
        {
            return ((char)int.Parse(m.Groups["Value"].Value, NumberStyles.HexNumber)).ToString();
        });
        Debug.Log(s);
        File.WriteAllText("Core/MapEditor/Data/CharacterProperty.json", JsonMapper.ToJson(data));
    }

    public void InitOriginPropertyData()
    {
        JsonData originDatas = JsonMapper.ToObject(File.ReadAllText("Core/MapEditor/Data/CharacterProperty.json"));


        for(int i = 0; i < originDatas.Count; i++)
        {
            Debug.Log(originDatas[i].ToJson());
        }

        //foreach (var item in originDatas)
        //{
        //    Debug.Log(item.ConvertTo(typeof(SCharacterData)));
        //    //originPropertyDatas.Add(item.name, new PropertyData(item.intProperty, item.floatProperty, item.stringProperty));
        //}
    }

    public SMapEditor mapEditor;
    public void InitCharacter(string name,string type,string detailType,PropertyData propertyData,GameObject obj)
    {
        if (type == "Character")
        {
            var cobj = obj.AddComponent<CharacterObject>();
            cobj.propertyData = propertyData;
            cobj.type = "Character";
            cobj.detailType = detailType;
            if (detailType == "Player")
            {
                SPlayer.s.currentPlayer = obj;
                cobj.detailType = "Player";
            }
        }
    }
    public void InitCharacter(SceneObjData data,GameObject obj)
    {
        InitCharacter(data.name, data.type, data.detailType, new PropertyData(data.intStatus, data.floatStatus, data.stringStatus), obj);
    }

    public void InitCharacter(StoreItem data,GameObject obj)
    {
        InitCharacter(data.name, data.type, data.detailType,new PropertyData( originPropertyDatas[data.name]), obj);
    }
}

public class SCharacterDatas
{
    public SCharacterData[] datas;

}

public class SCharacterData
{
    public string name;
    public Dictionary<string, int> intProperty;
    public Dictionary<string, float> floatProperty;
    public Dictionary<string, string> stringProperty;

    public SCharacterData()
    {
        name = "";
        intProperty = new Dictionary<string, int>();
        floatProperty = new Dictionary<string, float>();
        stringProperty = new Dictionary<string, string>();
    }

    public SCharacterData(string name)
    {
        this.name = name;
        intProperty = new Dictionary<string, int>();
        floatProperty = new Dictionary<string, float>();
        stringProperty = new Dictionary<string, string>();
    }

    public SCharacterData(string name,Dictionary<string,int> intProperty)
    {
        this.name = name;
        this.intProperty = intProperty;
        floatProperty = new Dictionary<string, float>();
        stringProperty = new Dictionary<string, string>();
    }

    public SCharacterData(string name,Dictionary<string,float> floatProperty)
    {
        this.name = name;
        intProperty = new Dictionary<string, int>();
        this.floatProperty = floatProperty;
        stringProperty = new Dictionary<string, string>();
    }

    public SCharacterData(string name, Dictionary<string, int> intProperty, Dictionary<string, float> floatProperty,Dictionary<string,string> stringProperty)
    {
        this.name = name;
        this.intProperty = intProperty;
        this.floatProperty = floatProperty;
        this.stringProperty = stringProperty;
    }
}


