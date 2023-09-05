using Cinemachine.Examples;
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
using UnityEngine.InputSystem;

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
    public Dictionary<string, PropertyData> map = new Dictionary<string, PropertyData>();
    public Dictionary<int, Entity> entityMap = new Dictionary<int, Entity>();

    //载入数据模板
    private void Init()
    {
        //获取Excel数据
        foreach (var item in ReadExcel("PropertyData/MapEditor/Data/数据.xls"))
        {
            map.Add(item.Key,item.Value);
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

    public Dictionary<string, PropertyData> ReadExcel(string filePath)
    {
        if (ReadExcelEnable_NPOI == true)
        {

            HSSFWorkbook MyBook;

            using (FileStream MyAddress_Read = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite))
            {
                MyBook = new HSSFWorkbook(MyAddress_Read);
            }

            Dictionary<string, PropertyData> datas = new Dictionary<string, PropertyData>();

            for (int i = 0; i < MyBook.NumberOfSheets; i++)
            {
                ISheet Sheet_Read = MyBook.GetSheetAt(i);

                PropertyData data = new PropertyData();

                Debug.Log(Sheet_Read.SheetName);

                for (int row = 0; row <= Sheet_Read.LastRowNum; row++)
                {
                    IRow Row_Read = Sheet_Read.GetRow(row);

                    if (Row_Read.GetCell(0).CellType == CellType.Blank || Row_Read.GetCell(0).ToSafeString()[0]=='#')
                    {
                        continue;
                    }

                    string key = Row_Read.GetCell(0).ToSafeString();


                    for (int column = 1; column < Row_Read.Cells.Count; column++)
                    {
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

                datas.Add(Sheet_Read.SheetName, new PropertyData(data));
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

        entity.propertyData.Add(PropertyData.EntityID, id);
    }

    public Entity GetEntity(int id)
    {
        return entityMap[id];
    }

    #endregion
}

public class PropertyData
{
    public static string EntityID = "EntityID";
    public static string StartEvent = "StartEvent";

    public string stringData;
    public float floatData;
    public int intData;
    public bool boolData;

    public List<PropertyData> list;
    public Dictionary<string,PropertyData> map;


    public PropertyData()
    {
        list = new List<PropertyData>();
        map = new Dictionary<string, PropertyData>();
    }

    public PropertyData(PropertyData origin)
    {
        stringData = origin.stringData;
        floatData = origin.floatData;
        intData = origin.intData;
        boolData = origin.boolData;
        list = new List<PropertyData>(origin.list);
        map = new Dictionary<string, PropertyData>(origin.map);
    }

    public PropertyData(string data)
    {
        stringData = data;
    }

    public PropertyData(float data)
    {
        floatData = data;
    }

    public PropertyData(int data)
    {
        intData = data;
    }

    public PropertyData(bool data)
    {
        boolData = data;
    }

    #region 添加值

    public void Add(string key,int value)
    {
        if (!map.ContainsKey(key))
            map.Add(key, (new PropertyData()).Add(value));
        else
            map[key].Add(value);

    }

    public PropertyData Add(int value)
    {
        list.Add(new PropertyData(value));
        return this;
    }

    public void Add(string key, float value)
    {
        if (!map.ContainsKey(key))
            map.Add(key, (new PropertyData()).Add(value));
        else
            map[key].Add(value);
    }

    public PropertyData Add(float value)
    {
        list.Add(new PropertyData(value));
        return this;
    }

    public void Add(string key, string value)
    {
        if (!map.ContainsKey(key))
            map.Add(key, (new PropertyData()).Add(value));
        else
            map[key].Add(value);
    }

    public PropertyData Add(string value)
    {
        list.Add(new PropertyData(value));
        return this;
    }

    public void Add(string key, bool value)
    {
        if (!map.ContainsKey(key))
            map.Add(key, (new PropertyData()).Add(value));
        else
            map[key].Add(value);
    }

    public PropertyData Add(bool value)
    {
        list.Add(new PropertyData(value));
        return this;
    }


    public void AddPropertyData(string key, PropertyData value)
    {
        if (!map.ContainsKey(key))
            map.Add(key,value);
    }

    public void Add(string key, PropertyData value)
    {
        if (!map.ContainsKey(key))
            map.Add(key, new PropertyData(value));
    }

    #endregion

    #region 设置值


    /*设置单个值*/

    public void SetData(string key, int value,int index=0)
    {
        if (map.ContainsKey(key))
            map[key].list[index] = new PropertyData(value);
        else
            map.Add(key, new PropertyData(value));
    }

    public void SetData(string key, float value,int index=0)
    {
        if (map.ContainsKey(key))
            map[key].list[index] = new PropertyData(value);
        else
            map.Add(key, new PropertyData(value));
    }

    public void SetData(string key, string value,int index=0)
    {
        if (map.ContainsKey(key))
            map[key].list[index] = new PropertyData(value);
        else
            map.Add(key, new PropertyData(value));
    }

    public void SetData(string key, bool value,int index=0)
    {
        if (map.ContainsKey(key))
            map[key].list[index] = new PropertyData(value);
        else
            map.Add(key, new PropertyData(value));
    }


    #endregion

    #region 获取值



    public int GetIntData(string key)
    {
        return map[key].intData;
    }

    public int GetInt(string key,int index=0)
    {
        return map[key].list[index].intData;
    }

    public float GetFloat(string key,int index=0)
    {
        return map[key].list[index].floatData;
    }


    public Vector3 GetVector3(string key)
    {
        return new Vector3(GetFloat(key,0), GetFloat(key, 1), GetFloat(key, 2));
    }

    public Quaternion GetQuaternion(string key)
    {
        return new Quaternion(GetFloat(key, 0), GetFloat(key, 1), GetFloat(key, 2), GetFloat(key, 3));
    }

    public string GetString()
    {
        return stringData;
    }

    public string GetString(string key,int index=0)
    {
        return map[key].list[index].stringData;
    }


    public bool GetBool(string key,int index = 0)
    {
        return map[key].list[index].boolData;
    }


    public List<PropertyData> GetDatas()
    {
        return list;
    }

    public List<string> GetStrings()
    {
        var stringList = new List<string>();
        for(int i = 0; i < list.Count; i++)
        {
            if (list[i].GetString() != null)
            {
                stringList.Add(list[i].GetString());
            }
        }
        return stringList;
    }

    public List<string> GetStrings(string key)
    {
        if (map.ContainsKey(key))
            return map[key].GetStrings();
        return null;
    }

    #endregion

    #region 判断键是否存在

    public bool ContainsKey(string key)
    {
        return map.ContainsKey(key);
    }



    #endregion

    #region 自定义函数



    #endregion
}
