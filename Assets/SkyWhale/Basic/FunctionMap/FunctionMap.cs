using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FunctionMap : MonoBehaviour
{
    #region µ¥Àý

    private static FunctionMap _s;
    public static FunctionMap s { get { return _s; } }

    private void Awake()
    {
        if(_s==null)
        {
            _s = this;
        }
    }

    #endregion


    public Dictionary<string, UnityAction<PropertyData>> map = new Dictionary<string, UnityAction<PropertyData>>();

    public void Start()
    {
        
    }

    public void Add(string name, UnityAction<PropertyData> action)
    {
        map.Add(name, action);
    }
}
