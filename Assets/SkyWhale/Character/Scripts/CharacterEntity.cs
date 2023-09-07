using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterEntity : Entity
{
    public static string LifeBarKey = "LifeBar";

    public override void Init()
    {
        if (propertyData.ContainsKey(LifeBarKey))
        {
            var data = propertyData.GetData(LifeBarKey);
            data.Add(transform.GetInstanceID());
            FunctionMap.map[SLifeBar.CreateLifeBar].Invoke(propertyData);
        }
    }
}
