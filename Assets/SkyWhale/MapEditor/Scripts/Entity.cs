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
        if (propertyData.GetString(Property.StartEvent) != null)
        {
            
            FunctionMap.map[propertyData.GetString(Property.StartEvent)].Invoke(propertyData);
        }
    }

    public virtual void Init()
    {

    }
}
