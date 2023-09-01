using Cinemachine.Examples;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

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
    }
    #endregion
}

public class PropertyData
{
    public Dictionary<string, List<int>> i;
    public Dictionary<string, List<float>> f;
    public Dictionary<string, List<string>> s;
    public Dictionary<string, List<bool>> b;


    public PropertyData()
    {
        i = new Dictionary<string, List<int>>();
        f = new Dictionary<string, List<float>>();
        s = new Dictionary<string, List<string>>();
        b = new Dictionary<string, List<bool>>();

    }

    public PropertyData(PropertyData origin)
    {
        i = new Dictionary<string, List<int>>(origin.i);
        f = new Dictionary<string, List<float>>(origin.f);
        s = new Dictionary<string, List<string>>(origin.s);
        b = new Dictionary<string, List<bool>>(origin.b);
    }

    #region 添加值

    public void Add(string key,int value)
    {
        if (i.ContainsKey(key))
            i[key].Add(value);
        else
            i[key] = new List<int> { value };
    }

    public void Add(string key, float value)
    {
        if (f.ContainsKey(key))
            f[key].Add(value);
        else
            f[key] = new List<float> { value };
    }

    public void Add(string key, string value)
    {
        if (s.ContainsKey(key))
            s[key].Add(value);
        else
            s[key] = new List<string> { value };
    }

    public void Add(string key, bool value)
    {
        if (b.ContainsKey(key))
            b[key].Add(value);
        else
            b[key] = new List<bool> { value };
    }

    #endregion

    #region 设置值

    /*初始化值*/
    public void InitDatas(Dictionary<string, List<int>> datas)
    {
        i =new Dictionary<string, List<int>>(datas);
    }

    public void InitDatas(Dictionary<string, List<float>> datas)
    {
        f = new Dictionary<string, List<float>>(datas);
    }

    public void InitDatas(Dictionary<string, List<string>> datas)
    {
        s = new Dictionary<string, List<string>>(datas);
    }

    public void InitDatas(Dictionary<string, List<bool>> datas)
    {
        b = new Dictionary<string, List<bool>>(datas);
    }


    /*设置单个值*/

    public void SetData(string key, int value)
    {
        if (i.ContainsKey(key))
            i[key][0] = value;
        else
            i.Add(key, new List<int> { value });
    }

    public void SetData(string key, float value)
    {
        if (f.ContainsKey(key))
            f[key][0] = value;
        else
            f.Add(key, new List<float> { value });
    }

    public void SetData(string key, string value)
    {
        if (s.ContainsKey(key))
            s[key][0] = value;
        else
            s.Add(key, new List<string> { value });
    }

    public void SetData(string key, bool value)
    {
        if (b.ContainsKey(key))
            b[key][0] = value;
        else
            b.Add(key, new List<bool> { value });
    }

    /*设置数组*/

    public void SetDatas(string key, int[] value)
    {
        if (i.ContainsKey(key))
            i[key] = value.ToList();
        else
            i.Add(key, value.ToList());
    }

    public void SetDatas(string key, float[] value)
    {
        if (f.ContainsKey(key))
            f[key] = value.ToList();
        else
            f.Add(key, value.ToList());
    }

    public void SetDatas(string key, string[] value)
    {
        if (s.ContainsKey(key))
            s[key] = value.ToList();
        else
            s.Add(key, value.ToList());
    }

    public void SetDatas(string key, bool[] value)
    {
        if (b.ContainsKey(key))
            b[key] = value.ToList();
        else
            b.Add(key, value.ToList());
    }

    #endregion

    #region 获取值

    /*获取单个值*/

    

    public void GetData(string key,out int value)
    {
        value = i[key][0];
    }

    public int GetIntData(string key)
    {
        return i[key][0];
    }

    public void GetData(string key, out float value)
    {
        value = f[key][0];
    }

    public float GetFloatData(string key)
    {
        return f[key][0];
    }

    public float GetFloat(string key,int index)
    {
        return f[key][index];
    }

    public Vector3 GetVector3(string key)
    {
        return new Vector3(f[key][0], f[key][1], f[key][2]);
    }

    public Quaternion GetQuaternion(string key)
    {
        return new Quaternion(f[key][0], f[key][1], f[key][2], f[key][3]);
    }

    public void GetData(string key, out string value)
    {
        value = s[key][0];
    }

    public string GetString(string key)
    {
        if (s.ContainsKey(key))
            return s[key][0];
        else
            return null;
    }

    public void GetData(string key, out bool value)
    {
        value = b[key][0];
    }

    public bool GetBoolData(string key)
    {
        return b[key][0];
    }

    /*获取数组值*/

    public void GetDatas(string key, out int[] value)
    {
        value = i[key].ToArray();
    }

    public void GetDatas(string key, out float[] value)
    {
        value = f[key].ToArray();
    }

    public void GetDatas(string key, out string[] value)
    {
        value = s[key].ToArray();
    }

    public List<string> GetStrings(string key)
    {
        return s[key];
    }

    public void GetData(string key, out bool[] value)
    {
        value = b[key].ToArray();
    }

    #endregion

    #region 判断键是否存在

    public bool IsIntExist(string key)
    {
        return i.ContainsKey(key);
    }

    public bool IsFloatExist(string key)
    {
        return f.ContainsKey(key);
    }

    public bool IsStringExist(string key)
    {
        return s.ContainsKey(key);
    }

    public bool IsBoolExist(string key)
    {
        return b.ContainsKey(key);
    }


    #endregion

    #region 自定义函数

    public void Print()
    {
        string s = "";
        foreach (var item in i)
        {
            s += item.ToString() + " ";
        }
        s += '\n';
        foreach (var item in f)
        {
            s += item.ToString() + " ";
        }
        s += '\n';
        foreach (var item in this.s)
        {
            s += item.ToString() + " ";
        }
        s += '\n';
        foreach (var item in this.b)
        {
            s += item.ToString() + " ";
        }
        Debug.Log(s);
    }

    #endregion
}
