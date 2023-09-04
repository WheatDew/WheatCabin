using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterEntity : Entity
{
    public static string IsLifeBarKey = "IsLifeBar";

    public override void Init()
    {
        if (propertyData.GetBool(IsLifeBarKey))
        {
            FunctionMap.s.map[SLifeBar.CreateLifeBar].Invoke(propertyData);
        }
    }
}
