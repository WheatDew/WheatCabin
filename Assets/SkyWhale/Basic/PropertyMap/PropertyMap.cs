
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
    public Dictionary<string, Property> map = new Dictionary<string, Property>();
    public Dictionary<int, Entity> entityMap = new Dictionary<int, Entity>();
    public static Dictionary<int, GameObject> gameObjectMap = new Dictionary<int, GameObject>();

    //载入数据模板
    private void Init()
    {
        ReadExcel("PropertyData/MapEditor/Data/数据.xls");

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

    public Dictionary<string, Property> ReadExcel(string filePath)
    {
        if (ReadExcelEnable_NPOI == true)
        {

            HSSFWorkbook MyBook;

            using (FileStream MyAddress_Read = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite))
            {
                MyBook = new HSSFWorkbook(MyAddress_Read);
            }

            Dictionary<string, Property> datas = new Dictionary<string, Property>();

            for (int i = 0; i < MyBook.NumberOfSheets; i++)
            {
                ISheet Sheet_Read = MyBook.GetSheetAt(i);

                Property data = new Property();

                Debug.Log(Sheet_Read.SheetName);

                for (int row = 0; row <= Sheet_Read.LastRowNum; row++)
                {
                    IRow Row_Read = Sheet_Read.GetRow(row);

                    if (Row_Read.GetCell(0).CellType == CellType.Blank || Row_Read.GetCell(0).ToSafeString()[0] == '#')
                    {
                        continue;
                    }

                    string key = Row_Read.GetCell(0).ToSafeString();
                    //Debug.LogFormat("当前的键为:{0}，当前数组长度为:{1}", key,Row_Read.Cells.Count);

                    for (int column = 1; column < Row_Read.Cells.Count; column++)
                    {
                        if (Row_Read.GetCell(column).CellType == CellType.Blank || Row_Read.GetCell(0).ToSafeString()[0] == '#')
                        {
                            break;
                        }
                        //Debug.LogFormat("{0} {1} {2}",Row_Read.GetCell(column).CellType,Row_Read.GetCell(column),column);
                        

                        if (Row_Read.GetCell(column).CellType == CellType.Numeric && Row_Read.GetCell(column).ToSafeString().Contains('.'))
                        {
                            data.Add(key, (float)Row_Read.GetCell(column).NumericCellValue);
                        }
                        else if (Row_Read.GetCell(column).CellType == CellType.Numeric)
                        {
                            data.Add(key, (int)Row_Read.GetCell(column).NumericCellValue);
                        }
                        else if (Row_Read.GetCell(column).CellType == CellType.Boolean)
                        {
                            data.Add(key, (bool)Row_Read.GetCell(column).BooleanCellValue);
                        }
                        else
                        {
                            data.Add(key, Row_Read.GetCell(column).ToSafeString());
                        }
                    }
                }

                data.SetMapReference();
                datas.Add(Sheet_Read.SheetName, new Property(data));
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

        entity.data.Set(Property.EntityID, 0, id);
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

public enum PropertyType { Empty,Data,Int,String,Float,Bool,List,Map}
public class Property
{
    public static string EntityID = "EntityID";
    public static string StartEvent = "StartEvent";



    private List<Property> list;

    private Dictionary<string, Property> map;

    private PropertyType type;

    public Property()
    {
        list = new List<Property>();
        map = new Dictionary<string, Property>();
    }

    public Property(Property property)
    {
        intValue = property.intValue;
        floatValue = property.floatValue;
        boolValue = property.boolValue;
        stringValue = property.stringValue;
        data = property.data;
        type = property.type;
        list = new List<Property>(property.list);
        map = new Dictionary<string, Property>(property.map);
    }
    

    #region String类型

    string stringValue;

    public Property(string value)
    {
        Set(value);
    }

    public void Add(string value)
    {
        list.Add(new Property(value));
        type = PropertyType.List;
    }

    public void Add(string key,string value)
    {
        if (map.ContainsKey(key))
            map[key].Add(value);
        else
        {
            map.Add(key, new Property());
            map[key].Add(value);
        }

        type = PropertyType.Map;

    }

    public void Set(string value)
    {
        stringValue = value;
        type = PropertyType.String;
    }


    public void Set(int index,string value)
    {
        list[index].Set(value);
    }


    public void Set(string key,int index,string value)
    {
        map[key].Set(index, value);
    }


    public string GetString(int index=0)
    {
        if (type == PropertyType.String)
            return stringValue;
        else if (type == PropertyType.Data)
            return data.GetString();
        else if (type == PropertyType.List)
            return list[index].GetString();
        Debug.LogErrorFormat("错误的类型{0}",type);
        return null;

    }

    public string GetString(string key, int index = 0)
    {
        return map[key].GetString(index);
    }


    #endregion

    #region Int类型

    int intValue;

    public Property(int value)
    {
        Set(value);
    }

    public Property Add(int value)
    {
        list.Add(new Property(value));
        type = PropertyType.List;
        return this;
    }

    public void Add(string key, int value)
    {
        if (map.ContainsKey(key))
            map[key].Add(value);
        else
        {
            map.Add(key, new Property());
            map[key].Add(value);
        }

        type = PropertyType.Map;
    }

    public void Set(int value)
    {
        if (type == PropertyType.Data)
        {
            data.Set(value);
        }
        else
        {
            intValue = value;
            type = PropertyType.Int;
        }


    }


    public Property Set(int index, int value)
    {
        list[index].Set(value);
        return this;
    }

    public void Set(string key, int index, int value)
    {
        if (map.ContainsKey(key))
            map[key].Set(index, value);
        else
        {
            map.Add(key, new Property(value));
        }
    }

    public int GetInt(int index=0)
    {
        if (type == PropertyType.Int)
            return intValue;
        else if (type == PropertyType.Data)
            return data.GetInt();
        else if (type == PropertyType.List)
            return list[index].GetInt();
        Debug.LogErrorFormat("类型错误:{0}",type);
        return 0;
    }


    public int GetInt(string key, int index=0)
    {
        return map[key].GetInt(index);
    }

    #endregion

    #region Float类型

    float floatValue;

    public Property(float value)
    {
        Set(value);
    }

    public void Add(float value)
    {
        list.Add(new Property(value));
    }

    public void Add(string key, float value)
    {
        if (map.ContainsKey(key))
            map[key].Add(value);
        else
        {
            map.Add(key, new Property());
            map[key].Add(value);
        }

    }

    public void Set(float value)
    {
        floatValue = value;
        type = PropertyType.Float;
    }


    public void Set(int index, float value)
    {
        list[index].Set(value);
    }

    public void Set(string key, int index, float value)
    {
        map[key].Set(index, value);
    }

    public float GetFloat()
    {
        return floatValue;
    }

    public float GetFloat(int index)
    {
        if (type==PropertyType.Float)
            return GetFloat();
        return list[index].GetFloat();
    }


    public float GetFloat(string key, int index=0)
    {
        return map[key].GetFloat(index);
    }

    public Vector3 GetVector3()
    {
        return new Vector3(GetFloat(0), GetFloat(1), GetFloat(2));
    }

    public Vector3 GetVector3(int index)
    {
        return list[index].GetVector3();
    }

    public Vector3 GetVector3(string key)
    {
        return map[key].GetVector3();
    }

    public Quaternion GetQuaternion()
    {
        return new Quaternion(GetFloat(0), GetFloat(1), GetFloat(2),GetFloat(3));
    }

    public Quaternion GetQuaternion(string key)
    {
        return map[key].GetQuaternion();
    }

    #endregion

    #region Bool类型

    bool boolValue;

    public Property(bool value)
    {
        Set(value);

    }

    public void Add(bool value)
    {
        list.Add(new Property(value));
    }

    public void Add(string key, bool value)
    {
        if (map.ContainsKey(key))
            map[key].Add(value);
        else
        {
            map.Add(key, new Property());
            map[key].Add(value);
        }
    }

    public void Set(bool value)
    {
        boolValue = value;
        type = PropertyType.Bool;
    }

    public void Set(string key, bool value)
    {
        map[key].Set(value);
    }

    public void Set(int index, bool value)
    {
        list[index].Set(value);
    }

    public void Set(string key, int index, bool value)
    {
        map[key].Set(index, value);
    }

    public bool GetBool()
    {
        return boolValue;
    }

    public bool GetBool(int index)
    {
        if (type==PropertyType.Bool)
            return GetBool();
        return list[index].GetBool();
    }


    public bool GetBool(string key, int index=0)
    {
        return map[key].GetBool(index);
    }

    #endregion


    #region 数据组

    Property data;

    public Property Set(Property property)
    {
        data = property;
        type = PropertyType.Data;
        return this;
    }

    public Property GetData()
    {
        if (type == PropertyType.Data)
            return data;
        return list[0];
    }

    public Property GetData(int index)
    {
        return list[index];
    }

    public Property GetData(string key)
    {
        return map[key];
    }

    public Property GetData(string key,int index)
    {
        return map[key].GetData(index);
    }

    /// <summary>
    /// 返回数据数组
    /// </summary>
    /// <returns></returns>
    public List<Property> GetDatas()
    {
        return list;
    }

    /// <summary>
    /// 返回数据字典
    /// </summary>
    /// <returns></returns>
    public Dictionary<string,Property> GetMap()
    {
        return map;
    }

    public List<Property> GetDatas(string key)
    {
        return map[key].GetDatas();
    }

    public void SetMapReference()
    {
        foreach (var item in map)
        {
            if (item.Value.list != null&&item.Value.list.Count>0)
            {
                for(int i = 0; i < item.Value.list.Count; i++)
                {
                    var target = item.Value.list[i];
                    if (target.type==PropertyType.String && target.GetString() != null && target.GetString()[0] == '&')
                    {
                        Debug.Log(target.stringValue);
                        target.Set( map[target.GetString()[1..]]);
                    }
                }
            }
        }
    }

    public Dictionary<string,Property>.KeyCollection GetKeys()
    {
        return map.Keys;
    }

    #endregion

    public bool ContainsKey(string key)
    {
        return map.ContainsKey(key);
    }


}




