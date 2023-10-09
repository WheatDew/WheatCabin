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
            var startEventData = data.Map[DataKey.StartEvent].List;
            for(int i = 0; i < startEventData.Count; i++)
            {
                FunctionMap.map[startEventData[i].List[0].String].Invoke(startEventData[i]);
            }
        }
    }

    public virtual void Init()
    {
        
    }

    public void InitData(INya data)
    {
        this.data =new NyaData(data);
        PropertyMap.s.SetEntity(transform.GetInstanceID(), this);
        Init();
        StartEvent();
    }
}
