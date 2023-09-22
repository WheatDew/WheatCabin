using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FunctionMap
{

    public static Dictionary<string, UnityAction<INya>> map = new Dictionary<string, UnityAction<INya>>();


    public static void Add(string name, UnityAction<INya> action)
    {
        map.Add(name, action);
    }

}
