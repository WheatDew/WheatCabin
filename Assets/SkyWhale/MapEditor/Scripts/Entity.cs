using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    public string type;
    public string detailType;
    public PropertyData propertyData=new PropertyData();

    private void Start()
    {
        PropertyMap.s.SetEntity(transform.GetInstanceID(), this);
        Init();
    }

    public virtual void Init()
    {

    }
}
