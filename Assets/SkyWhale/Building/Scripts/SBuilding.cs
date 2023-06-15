using SkyWhale;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEditor;
using UnityEngine;

public class SBuilding : MonoBehaviour
{

    #region 单例代码
    private static SBuilding _s;
    public static SBuilding s { get { return _s; } }


    private void Awake()
    {
        if(_s == null)
        {
            _s = this;
        }
    }
    #endregion

    public void InitBuilding(string name, string type, string detailType, Dictionary<string, int> intStatus, Dictionary<string, float> floatStatus, Dictionary<string, string> stringStatus, GameObject obj)
    {
        if (type == "Building")
        {
            var cobj = obj.AddComponent<NormalObject>();
            cobj.intStatus = intStatus;
            cobj.floatStatus = floatStatus;
            cobj.stringStatus = stringStatus;
            cobj.type = "Building";
            cobj.detailType = detailType;
        }
    }

    public void InitBuilding(SceneObjData data, GameObject obj)
    {
        InitBuilding(data.name, data.type, data.detailType, data.intStatus, data.floatStatus, data.stringStatus, obj);
    }

    public void InitBuilding(StoreItem data, GameObject obj)
    {
        Debug.LogFormat("{0} {1} {2}", data.name, data.type, data.detailType);
        InitBuilding(data.name, data.type, data.detailType, new Dictionary<string, int>(), new Dictionary<string, float>(), new Dictionary<string, string>(), obj);
    }
}

#region 类型
public class BuildingPrefabData
{
    public string name;
    public string detailType;
}

public class BuildingPrefabDataList
{
    public BuildingPrefabData[] buildings;
}

#endregion
