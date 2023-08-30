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
        //mapEditor.mapEditorModelEvent.AddListener(InitCharacter);
    }
    #endregion

    #region 变量


    #endregion

    #region 系统函数
    private void Start()
    {
        //WriteOriginPropertyData();

    }
    #endregion


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
        //InitCharacter(data.name, data.type, data.detailType, new PropertyData(data.intStatus, data.floatStatus, data.stringStatus), obj);
    }

    //public void InitCharacter(StoreItem data,GameObject obj)
    //{
    //    InitCharacter(data.name, data.type, data.detailType,new PropertyData( PropertyMap.s.map[data.name]), obj);
    //}
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


