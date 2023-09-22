
using NPOI.HSSF.UserModel;
using NPOI.SS.Formula.Functions;
using NPOI.SS.UserModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions.Must;

public class PropertyMap : MonoBehaviour
{
    #region 单例代码

    private static PropertyMap _s;
    public static PropertyMap s { get { return _s; } }

    private void Awake()
    {
        if (_s == null)
            _s = this;

        Init();
    }

    #endregion

    //数据模板
    public Dictionary<string, INya> map = new Dictionary<string, INya>();
    public Dictionary<int, Entity> entityMap = new Dictionary<int, Entity>();
    public static Dictionary<int, GameObject> gameObjectMap = new Dictionary<int, GameObject>();

    //载入数据模板
    private void Init()
    {
        //ReadExcel("PropertyData/MapEditor/Data/数据.xlsx");

        //获取Excel数据
        foreach (var item in ReadExcel("PropertyData/MapEditor/Data/数据.xls"))
        {
            map.Add(item.Key, item.Value);
        }
        ////获取Json数据
        //foreach (var item in JsonSystem.ReadJsonProperty("Core/MapEditor/Data/Property.json"))
        //{
        //    map.Add(item.Key, item.Value);
        //}
    }

    #region 读取Excel数据

    private bool ReadExcelEnable_NPOI = true;
    private bool ReadExcelEnable_ExcelDataReader = false;

    public Dictionary<string, INya> ReadExcel(string filePath)
    {
        if (ReadExcelEnable_NPOI == true)
        {

            HSSFWorkbook MyBook;

            using (FileStream MyAddress_Read = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite))
            {
                MyBook = new HSSFWorkbook(MyAddress_Read);
            }

            Dictionary<string, INya> datas = new Dictionary<string, INya>();

            //获取工作表
            for (int i = 0; i < MyBook.NumberOfSheets; i++)
            {
                ISheet Sheet_Read = MyBook.GetSheetAt(i);

                INya data = new NyaMap();
                

                Debug.Log(Sheet_Read.SheetName);

                //获取条目
                for (int row = 0; row <= Sheet_Read.LastRowNum; row++)
                {
                    IRow Row_Read = Sheet_Read.GetRow(row);

                    if (Row_Read.GetCell(0)==null||Row_Read.GetCell(0).CellType == CellType.Blank || Row_Read.GetCell(0).ToSafeString()[0] == '#')
                    {
                        continue;
                    }
                    //获取条目名
                    string key = Row_Read.GetCell(0).ToSafeString();
                    if(key=="")
                    Debug.LogFormat("当前的键为:{0}，当前数组长度为:{1}", key,Row_Read.Cells.Count);

                    //获取条目内容
                    for (int column = 1; column < Row_Read.Cells.Count; column++)
                    {
                        if (Row_Read.GetCell(column)==null|| Row_Read.GetCell(column).CellType == CellType.Blank || Row_Read.GetCell(0).ToSafeString()[0] == '#')
                        {
                            break;
                        }
                        //Debug.LogFormat("{0} {1} {2}",Row_Read.GetCell(column).CellType,Row_Read.GetCell(column),column);
                        

                        if (Row_Read.GetCell(column).CellType == CellType.Numeric && Row_Read.GetCell(column).ToSafeString().Contains('.'))
                        {
                            data.Add(key,new NyaFloat((float)Row_Read.GetCell(column).NumericCellValue));
                        }
                        else if (Row_Read.GetCell(column).CellType == CellType.Numeric)
                        {
                            data.Add(key,new NyaInt((int)Row_Read.GetCell(column).NumericCellValue));
                        }
                        else if (Row_Read.GetCell(column).CellType == CellType.Boolean)
                        {
                            data.Add(key,new NyaBool((bool)Row_Read.GetCell(column).BooleanCellValue));
                        }
                        else
                        {
                            data.Add(key,new NyaString(Row_Read.GetCell(column).ToSafeString()));
                        }
                    }
                }

                data.SetMapReference();
                datas.Add(Sheet_Read.SheetName, data);
            }

            return datas;
        }

        return null;
    }

    #endregion

    #region 设置实体
    public void SetEntity(int id,Entity entity)
    {

        if (entityMap.ContainsKey(id))
        {
            entityMap[id] = entity;
        }
        else
        {
            entityMap.Add(id, entity);
        }

        entity.data.Set("EntityID", 0, id);
    }

    public Entity GetEntity(int id)
    {
        return entityMap[id];
    }

    public static void SetGameObject(int id, GameObject entity)
    {

        if (gameObjectMap.ContainsKey(id))
        {
            gameObjectMap[id] = entity;
        }
        else
        {
            gameObjectMap.Add(id, entity);
        }
    }

    public static GameObject GetGameObject(int id)
    {
        return gameObjectMap[id];
    }



    #endregion
}

#region 老方法
public enum PropertyType { Empty,Data,Int,String,Float,Bool,List,Map}
//public class INya
//{
//    public static string EntityID = "EntityID";
//    public static string StartEvent = "StartEvent";



//    private List<INya> list;

//    private Dictionary<string, INya> map;

//    private PropertyType type;

//    public INya()
//    {
//        list = new List<INya>();
//        map = new Dictionary<string, INya>();
//    }

//    public INya(PropertyType type)
//    {
//        this.type = type;
//        list = new List<INya>();
//        map = new Dictionary<string, INya>();
//    }

//    public INya(INya property)
//    {
//        intValue = property.intValue;
//        floatValue = property.floatValue;
//        boolValue = property.boolValue;
//        stringValue = property.stringValue;
//        data = property.data;
//        type = property.type;
//        list = new List<INya>(property.list);
//        map = new Dictionary<string, INya>(property.map);
//    }

//    #region 构造函数

//    string stringValue;
//    public INya(string value)
//    {
//        Set(value);
//    }

//    int intValue;
//    public INya(int value)
//    {
//        Set(value);
//    }

//    float floatValue;
//    public INya(float value)
//    {
//        Set(value);
//    }

//    bool boolValue;
//    public INya(bool value)
//    {
//        Set(value);

//    }

//    INya data;
//    public INya Set(INya property)
//    {
//        data = property;
//        type = PropertyType.Data;
//        return this;
//    }

//    #endregion

//    #region 设置函数

//    //string类型
//    public void Set(string value)
//    {
//        stringValue = value;
//        type = PropertyType.String;
//    }
//    public void Set(int index, string value)
//    {
//        list[index].Set(value);
//    }
//    public void Set(string key, int index, string value)
//    {
//        map[key].Set(index, value);
//    }

//    //int类型
//    public void Set(int value)
//    {
//        if (type == PropertyType.Data)
//        {
//            data.Set(value);
//        }
//        else
//        {
//            intValue = value;
//            type = PropertyType.Int;
//        }


//    }
//    public INya Set(int index, int value)
//    {
//        list[index].Set(value);
//        return this;
//    }
//    public void Set(string key, int index, int value)
//    {
//        if (map.ContainsKey(key))
//            map[key].Set(index, value);
//        else
//        {
//            map.Add(key, new INya(value));
//        }
//    }

//    //float类型
//    public void Set(float value)
//    {
//        floatValue = value;
//        type = PropertyType.Float;
//    }
//    public void Set(int index, float value)
//    {
//        list[index].Set(value);
//    }
//    public void Set(string key, int index, float value)
//    {
//        map[key].Set(index, value);
//    }

//    //bool类型
//    public void Set(bool value)
//    {
//        boolValue = value;
//        type = PropertyType.Bool;
//    }
//    public void Set(string key, bool value)
//    {
//        map[key].Set(value);
//    }
//    public void Set(int index, bool value)
//    {
//        list[index].Set(value);
//    }
//    public void Set(string key, int index, bool value)
//    {
//        map[key].Set(index, value);
//    }
//    #endregion

//    #region 添加函数

//    //string类型
//    public void Add(string value)
//    {
//        list.Add(new INya(value));
//        type = PropertyType.List;
//    }
//    public void Add(string key, string value)
//    {
//        if (map.ContainsKey(key))
//            map[key].Add(value);
//        else
//        {
//            map.Add(key, new INya());
//            map[key].Add(value);
//        }

//        type = PropertyType.Map;

//    }

//    //int类型
//    public INya Add(int value)
//    {
//        list.Add(new INya(value));
//        type = PropertyType.List;
//        return this;
//    }
//    public void Add(string key, int value)
//    {
//        if (map.ContainsKey(key))
//            map[key].Add(value);
//        else
//        {
//            map.Add(key, new INya());
//            map[key].Add(value);
//        }

//        type = PropertyType.Map;
//    }

//    //float类型
//    public void Add(float value)
//    {
//        list.Add(new INya(value));
//        type = PropertyType.List;
//    }
//    public void Add(string key, float value)
//    {
//        if (map.ContainsKey(key))
//        {
//            map[key].Add(value);
//            type = PropertyType.Map;
//        }
//        else
//        {
//            map.Add(key, new INya(PropertyType.List));
//            map[key].Add(value);
//            type = PropertyType.Map;
//        }

//    }

//    //bool类型
//    public void Add(bool value)
//    {
//        list.Add(new INya(value));
//    }
//    public void Add(string key, bool value)
//    {
//        if (map.ContainsKey(key))
//            map[key].Add(value);
//        else
//        {
//            map.Add(key, new INya());
//            map[key].Add(value);
//        }
//    }

//    #endregion

//    #region 获取函数

//    //string
//    public string GetString(int index = 0)
//    {
//        if (type == PropertyType.String)
//            return stringValue;
//        else if (type == PropertyType.Data)
//            return data.GetString(index);
//        else if (type == PropertyType.List)
//            return list[index].GetString(index);
//        Debug.LogErrorFormat("错误的类型{0}而目标类型为String", type);
//        return null;

//    }
//    public string GetString(string key, int index = 0)
//    {
//        return map[key].GetString(index);
//    }

//    //int
//    public int GetInt(int index = 0)
//    {
//        if (type == PropertyType.Int)
//            return intValue;
//        else if (type == PropertyType.Data)
//            return data.GetInt(index);
//        else if (type == PropertyType.List)
//            return list[index].GetInt(index);
//        Debug.LogErrorFormat("类型错误:{0}", type);
//        return 0;
//    }
//    public int GetInt(string key, int index = 0)
//    {
//        return map[key].GetInt(index);
//    }

//    //float
//    public float GetFloat(int index = 0)
//    {
//        if (type == PropertyType.Float)
//            return floatValue;
//        else if (type == PropertyType.Data)
//            return data.GetFloat(index);
//        else if (type == PropertyType.List)
//            return list[index].GetFloat();
//        Debug.LogErrorFormat("类型错误:{0}", type);
//        return 0;
//    }
//    public float GetFloat(string key, int index = 0)
//    {
//        return map[key].GetFloat(index);
//    }
//    public Vector3 GetVector3()
//    {
//        if (type == PropertyType.List)
//            return new Vector3(GetFloat(0), GetFloat(1), GetFloat(2));
//        else if (type == PropertyType.Data)
//            return data.GetVector3();
//        Debug.LogErrorFormat("类型错误：需要为List或Data类型，但目标类型为{0}", type);
//        return Vector3.zero;
//    }
//    public Vector3 GetVector3(int index)
//    {
//        return list[index].GetVector3();
//    }
//    public Vector3 GetVector3(string key)
//    {
//        return map[key].GetVector3();
//    }
//    public Quaternion GetQuaternion()
//    {
//        return new Quaternion(GetFloat(0), GetFloat(1), GetFloat(2), GetFloat(3));
//    }
//    public Quaternion GetQuaternion(string key)
//    {
//        return map[key].GetQuaternion();
//    }

//    //bool
//    public bool GetBool()
//    {
//        return boolValue;
//    }
//    public bool GetBool(int index)
//    {
//        if (type == PropertyType.Bool)
//            return GetBool();
//        return list[index].GetBool();
//    }
//    public bool GetBool(string key, int index = 0)
//    {
//        return map[key].GetBool(index);
//    }

//    //data


//    #endregion





//    #region 数据组



//    public INya GetData()
//    {
//        if (type == PropertyType.Data)
//            return data;
//        return list[0];
//    }

//    public INya GetData(int index)
//    {
//        return list[index];
//    }

//    public INya GetData(string key)
//    {
//        return map[key];
//    }

//    public INya GetData(string key,int index)
//    {
//        return map[key].GetData(index);
//    }

//    /// <summary>
//    /// 返回数据数组
//    /// </summary>
//    /// <returns></returns>
//    public List<INya> GetDatas()
//    {
//        return list;
//    }

//    /// <summary>
//    /// 返回数据字典
//    /// </summary>
//    /// <returns></returns>
//    public Dictionary<string,INya> GetMap()
//    {
//        return map;
//    }

//    public List<INya> GetDatas(string key)
//    {
//        return map[key].GetDatas();
//    }

//    public void SetMapReference()
//    {
//        foreach (var item in map)
//        {
//            if (item.Value.list != null&&item.Value.list.Count>0)
//            {
//                for(int i = 0; i < item.Value.list.Count; i++)
//                {
//                    var target = item.Value.list[i];
//                    if (target.type==PropertyType.String && target.GetString() != null && target.GetString()[0] == '&')
//                    {
//                        Debug.Log(target.stringValue);
//                        target.Set( map[target.GetString()[1..]]);
//                    }
//                }
//            }
//        }
//    }

//    public Dictionary<string,INya>.KeyCollection GetKeys()
//    {
//        return map.Keys;
//    }

//    #endregion

//    public bool ContainsKey(string key)
//    {
//        return map.ContainsKey(key);
//    }

//    public PropertyType DataType()
//    {
//        return type;
//    }

//}

#endregion

public interface INyaInt
{
    int GetInt() { Debug.LogError("错误"); return 0; }
    int GetInt(string key) { Debug.LogError("错误"); return 0; }
    int GetInt(int index) { Debug.LogError("错误"); return 0; }
    int GetInt(string key, int index) { Debug.LogError("错误"); return 0; }
    void Set(string key,int index,int value) { Debug.LogError("错误"); }
}

public interface INyaString
{
    string GetString() { Debug.LogError("错误"); return null; }
    string GetString(string key) { Debug.LogError("错误"); return null; }
    string GetString(int index) { Debug.LogError("错误"); return null; }
    string GetString(string key, int index) { Debug.LogError("错误"); return null; }
}

public interface INyaFloat
{
    float GetFloat() { Debug.LogError("错误"); return 0; }
    float GetFloat(string key) { Debug.LogError("错误"); return 0; }
    float GetFloat(int index) { Debug.LogError("错误"); return 0; }
    float GetFloat(string key,int index) { Debug.LogError("错误"); return 0; }
    Vector3 GetVector3(string key) { Debug.LogError("错误"); return Vector3.zero; }
    Vector3 GetVector3(int index) { Debug.LogError("错误"); return Vector3.zero; }
    Quaternion GetQuaternion(string key) { Debug.LogError("错误"); return Quaternion.identity; }
    Quaternion GetQuaternion(int index) { Debug.LogError("错误"); return Quaternion.identity; }
}

public interface INyaBool
{
    bool GetBool() { Debug.LogError("错误"); return false; }
    bool GetBool(string key) { Debug.LogError("错误"); return false; }
    bool GetBool(int index) { Debug.LogError("错误"); return false; }
    bool GetBool(string key,int index) { Debug.LogError("错误"); return false; }
}

public interface INyaList
{
    List<INya> GetList() { Debug.LogError("错误"); return null; }
    NyaList Add(INya value) { Debug.LogError("错误"); return null; }
    List<INya> GetList(string key) { Debug.LogError("错误"); return null; }
}

public interface INyaMap
{

    Dictionary<string, INya> GetMap() { Debug.LogError("错误"); return null; }
    NyaMap Add(string key, INya value) { Debug.LogError("错误"); return null; }
    void SetMapReference() { Debug.LogError("错误"); }
    bool ContainsKey(string key) { Debug.LogError("错误"); return false; }
}

public interface INya:INyaInt,INyaString,INyaFloat,INyaBool,INyaList,INyaMap
{
    INya GetData() { Debug.LogError("错误"); return null; }
    INya GetData(string key) { Debug.LogError("错误"); return null; }
    INya GetData(int index) { Debug.LogError("错误"); return null; }
    INya GetData(string key,int index) { Debug.LogError("错误"); return null; }
}

public class NyaInt : INya
{
    public int data;
    public NyaInt(int data)
    {
        this.data = data;
    }

    public int GetInt()
    {
        return data;
    }

}

public class NyaFloat : INya
{
    public float data;
    public NyaFloat(float data)
    {
        this.data = data;
    }

    public float GetFloat()
    {
        return data;
    }
}

public class NyaBool : INya
{
    public bool data;
    public NyaBool(bool data)
    {
        this.data = data;
    }

    public bool GetBool()
    {
        return data;
    }
}

public class NyaString : INya
{
    public string data;
    public NyaString(string data)
    {
        this.data = data;
    }

    public string GetString()
    {
        return data;
    }
}

public class NyaData : INya
{
    public INya data;
    public NyaData(INya data)
    {
        this.data = data;
    }

    public INya GetData()
    {
        return data;
    }
}

public class NyaList : INya
{
    public List<INya> data;

    public NyaList()
    {
        data = new List<INya>();
    }

    public NyaList Add(INya value)
    {
        data.Add(value);
        return this;
    }
}

public class NyaMap : INya
{
    public Dictionary<string, INya> data;

    public NyaMap()
    {
        data = new Dictionary<string, INya>();
    }

    public NyaMap(NyaMap data)
    {
        this.data = data.data;
    }

    public NyaMap Add(string key,INya value)
    {
        data.Add(key, value);
        return this;
    }
}

