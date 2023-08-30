using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

    //载入数据模板
    private void Init()
    {
        //获取Excel数据
        foreach (var item in ExcelSystem.ReadExcel("Core/MapEditor/Data/数据.xls"))
        {
            map.Add(item.Key,item.Value);
        }
        ////获取Json数据
        //foreach (var item in JsonSystem.ReadJsonProperty("Core/MapEditor/Data/Property.json"))
        //{
        //    map.Add(item.Key, item.Value);
        //}
    }

}

public class PropertyData
{
    public Dictionary<string, int> i;
    public Dictionary<string, float> f;
    public Dictionary<string, string> s;
    public Dictionary<string, bool> b;

    public PropertyData()
    {
        i = new Dictionary<string, int>();
        f = new Dictionary<string, float>();
        s = new Dictionary<string, string>();
        b = new Dictionary<string, bool>();
    }


    public PropertyData(PropertyData origin)
    {
        i = new Dictionary<string, int>(origin.i);
        f = new Dictionary<string, float>(origin.f);
        s = new Dictionary<string, string>(origin.s);
        b = new Dictionary<string, bool>(origin.b);
    }

    public PropertyData(Dictionary<string, int> intData, Dictionary<string, float> floatData, Dictionary<string, string> stringData,Dictionary<string,bool> boolData)
    {
        this.i = intData;
        this.f = floatData;
        this.s = stringData;
        this.b = boolData;
    }

    public PropertyData(string name, string value)
    {
        this.s = new Dictionary<string, string> { { name, value } };
    }


    public void SetData(string key, int value)
    {
        if (i.ContainsKey(key))
            i[key] = value;
        else
            i.Add(key, value);
    }

    public void SetData(string key, float value)
    {
        if (f.ContainsKey(key))
            f[key] = value;
        else
            f.Add(key, value);
    }

    public void SetData(string key, string value)
    {
        s.Add(key, value);
    }

    public void SetData(string key, bool value)
    {
        b.Add(key, value);
    }

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
}
