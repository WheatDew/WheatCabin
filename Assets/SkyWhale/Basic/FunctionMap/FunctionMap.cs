using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FunctionMap
{

    public static Dictionary<string, UnityAction<Property>> map = new Dictionary<string, UnityAction<Property>>();


    public static void Add(string name, UnityAction<Property> action)
    {
        map.Add(name, action);
    }

}
