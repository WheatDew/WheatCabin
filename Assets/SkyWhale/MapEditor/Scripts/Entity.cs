using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    public static string StartEventKey="StartEvent";

    public string type;
    public string detailType;
    public Property data=new Property();

    private void Start()
    {
        
    }

    public void StartEvent()
    {
        if (data.ContainsKey(StartEventKey))
        {
            var startEventData = data.GetData(StartEventKey);
            FunctionMap.map[startEventData.GetString(0)].Invoke(startEventData.GetData(1));
        }
    }

    public virtual void Init()
    {
        
    }

    public void InitData(Property data)
    {
        this.data = data;
        PropertyMap.s.SetEntity(transform.GetInstanceID(), this);
        Init();
    }
}
