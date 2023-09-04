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
        StartEvent();
        Init();
    }

    public void StartEvent()
    {
        if (propertyData.GetString(PropertyData.StartEvent) != null)
        {
            
            FunctionMap.s.map[propertyData.GetString(PropertyData.StartEvent)].Invoke(propertyData);
        }
    }

    public virtual void Init()
    {

    }
}
