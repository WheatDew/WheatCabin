using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{


    public string type;
    public string detailType;
    public INya data=new NyaMap();

    private void Start()
    {
        
    }

    public void StartEvent()
    {
        if (data.Map.ContainsKey(DataKey.StartEvent))
        {
            var startEventData = data.Map[DataKey.StartEvent];
            FunctionMap.map[startEventData.List[0].String].Invoke(startEventData.List[1]);
        }
    }

    public virtual void Init()
    {
        
    }

    public void InitData(INya data)
    {
        this.data = data;
        PropertyMap.s.SetEntity(transform.GetInstanceID(), this);
        Init();
        StartEvent();
    }
}
