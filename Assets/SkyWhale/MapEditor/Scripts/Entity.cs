using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    public string type;
    public string detailType;
    public Property data=new Property();

    private void Start()
    {
        
    }

    public void StartEvent()
    {

    }

    public virtual void Init()
    {
        
    }

    public void InitData(Property data)
    {
        this.data = data;
        PropertyMap.s.SetEntity(transform.GetInstanceID(), this);
        StartEvent();
        Init();
    }
}
