using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalObject : MonoBehaviour
{
    public string type;
    public string detailType;
    public Dictionary<string, int> intStatus = new Dictionary<string, int>();
    public Dictionary<string, float> floatStatus = new Dictionary<string, float>();
    public Dictionary<string, string> stringStatus = new Dictionary<string, string>();

    private void Start()
    {
        SMapEditor.objMap.Add(this);
    }

}
