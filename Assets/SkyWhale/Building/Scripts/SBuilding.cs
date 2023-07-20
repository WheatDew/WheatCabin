using SkyWhale;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEditor;
using UnityEngine;

public class SBuilding : MonoBehaviour
{

    #region ��������
    private static SBuilding _s;
    public static SBuilding s { get { return _s; } }



    private void Awake()
    {
        if(_s == null)
        {
            _s = this;
        }
        //��ӳ�ʼ����������
        mapEditor.mapEditorModelEvent.AddListener(InitBuilding);
    }
    #endregion


    public SMapEditor mapEditor;

    public void InitBuilding(string name, string type, string detailType, Dictionary<string, int> intStatus, Dictionary<string, float> floatStatus, Dictionary<string, string> stringStatus, GameObject obj)
    {
        if (type == "Building")
        {
            var cobj = obj.AddComponent<NormalObject>();
            cobj.propertyData = new PropertyData(intStatus, floatStatus, stringStatus);
            cobj.type = "Building";
            cobj.detailType = detailType;
        }
    }

    //��ʼ�������ı༭��ϵͳ����
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

#region ����
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
