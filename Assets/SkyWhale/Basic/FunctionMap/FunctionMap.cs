using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FunctionMap
{

    public static Dictionary<string, UnityAction<PropertyData>> map = new Dictionary<string, UnityAction<PropertyData>>();


    public static void Add(string name, UnityAction<PropertyData> action)
    {
        map.Add(name, action);
    }

}
