using SkyWhale;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SBuilding : MonoBehaviour
{
    private static SBuilding _s;
    public static SBuilding s { get { return _s; } }


    private void Awake()
    {
        if(_s == null)
        {
            _s = this;
        }

        if (!SMapEditor.funMap.ContainsKey("Building"))
            SMapEditor.funMap.Add("Building", LoadBuilding);
    }

    public void LoadBuilding(string data)
    {
        
    }
}

#region ¿‡–Õ
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
