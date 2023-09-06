using Battlehub.Spline3;
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
using UnityWeld.Binding.Adapters;

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

        entity.propertyData.Add(Property.EntityID, id);
    }

    public Entity GetEntity(int id)
    {
        return entityMap[id];
    }

    #endregion
}

public class Property
{
    public static string EntityID = "EntityID";
    public static string StartEvent = "StartEvent";



    public List<Property> list;

    public Dictionary<string, Property> map;

    public Property()
    {
        list = new List<Property>();
        map = new Dictionary<string, Property>();
    }

    public Property(Property property)
    {
        list = new List<Property>(property.list);
        map = new Dictionary<string, Property>(property.map);
    }
    

    #region String类型

    string stringValue;

    public Property(string value,bool isSynclist=false)
    {
        stringValue = value;
        if (isSynclist)
            list = new List<Property> { new Property(value) };
    }

    public void Add(string value)
    {
        if (list == null)
            Debug.Log("list为空");

        //list.Add(new Property(value));
    }

    public void Add(string key,string value)
    {
        if (map.ContainsKey(key))
            map[key].Add(value);
        else
            map.Add(key, new Property(value));
    }

    public void Set(string value)
    {
        stringValue = value;
    }

    public void Set(string key,string value)
    {
        map[key].Set(value);
    }

    public void Set(int index,string value)
    {
        list[index].Set(value);
    }

    public void Set(string key,int index,string value)
    {
        map[key].Set(index, value);
    }

    public string GetString()
    {
        return stringValue;
    }

    public string GetString(int index)
    {
        return list[index].GetString();
    }

    public string GetString(string key)
    {
        return map[key].GetString();
    }

    public string GetString(string key,int index)
    {
        return map[key].GetString(index);
    }

    #endregion

    #region Int类型

    int intValue;

    public Property(int value,bool isSynclist=false)
    {
        intValue = value;
        if (isSynclist)
            list = new List<Property> { new Property(value) };
    }

    public void Add(int value)
    {
        list.Add(new Property(value));
    }

    public void Add(string key, int value)
    {
        if (map.ContainsKey(key))
            map[key].Add(value);
        else
            map.Add(key, new Property(value));
    }

    public void Set(int value)
    {
        intValue = value;
    }

    public void Set(string key, int value)
    {
        map[key].Set(value);
    }

    public void Set(int index, int value)
    {
        list[index].Set(value);
    }

    public void Set(string key, int index, int value)
    {
        map[key].Set(index, value);
    }

    public int GetInt()
    {
        return intValue;
    }

    public int GetInt(int index)
    {
        return list[index].GetInt();
    }

    public int GetInt(string key)
    {
        return map[key].GetInt();
    }

    public int GetInt(string key, int index)
    {
        return map[key].GetInt(index);
    }

    #endregion

    #region Float类型

    float floatValue;

    public Property(float value,bool isSynclist=false)
    {
        floatValue = value;
        if (isSynclist)
            list = new List<Property> { new Property(value) };
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
            map.Add(key, new Property(value));
    }

    public void Set(float value)
    {
        floatValue = value;
    }

    public void Set(string key, float value)
    {
        map[key].Set(value);
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
        return list[index].GetFloat();
    }

    public float GetFloat(string key)
    {
        return map[key].GetFloat();
    }

    public float GetFloat(string key, int index)
    {
        return map[key].GetFloat(index);
    }

    public Vector3 GetVector3()
    {
        return new Vector3(GetFloat(0), GetFloat(1), GetFloat(2));
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

    public Property(bool value,bool isSynclist=false)
    {
        boolValue = value;
        if(isSynclist)
        list = new List<Property> { new Property(value) };
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
            map.Add(key, new Property(value));
    }

    public void Set(bool value)
    {
        boolValue = value;
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
        return list[index].GetBool();
    }

    public bool GetBool(string key)
    {
        return map[key].GetBool();
    }

    public bool GetBool(string key, int index)
    {
        return map[key].GetBool(index);
    }

    #endregion


    #region 数据组

    public Property GetData(string key)
    {
        return map[key];
    }

    public Property GetData(int index)
    {
        return list[index];
    }

    public List<Property> GetDatas()
    {
        return list;
    }

    public List<Property> GetDatas(string key)
    {
        return map[key].GetDatas();
    }

    #endregion

    public bool ContainsKey(string key)
    {
        return map.ContainsKey(key);
    }

}




