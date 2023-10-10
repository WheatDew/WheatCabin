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
            Debug.LogFormat("{0} {1}", startEventData[0].Type, startEventData[0].String);
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
        Debug.Log(data.Type);
        this.data = data.Clone;
        this.data.SetMapReference();
        PropertyMap.s.SetEntity(transform.GetInstanceID(), this);
        Init();
        StartEvent();
    }
}
