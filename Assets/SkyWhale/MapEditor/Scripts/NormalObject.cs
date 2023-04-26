using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalObject : MonoBehaviour
{
    public string type;

    private void Start()
    {
        SMapEditor.objMap.Add(this);
    }

}
