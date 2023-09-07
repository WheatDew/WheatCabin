using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    public string type;
    public string detailType;
    public Property propertyData=new Property();

    private void Start()
    {
        PropertyMap.s.SetEntity(transform.GetInstanceID(), this);
        StartEvent();
        Init();
    }

    public void StartEvent()
    {

    }

    public virtual void Init()
    {

    }
}
