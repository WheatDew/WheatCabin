using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalObject : MonoBehaviour
{
    public string type;
    public string detailType;
    public PropertyData propertyData=new PropertyData();

    private void Start()
    {
        //SMapEditor.objMap.Add(this);
        Init();
    }

    public virtual void Init()
    {

    }
}
