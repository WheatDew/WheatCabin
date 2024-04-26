using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangeCalculateSystem : MonoBehaviour
{
    private static RangeCalculateSystem _s;
    public static  RangeCalculateSystem s { get { return _s; } }

    private void Awake()
    {
        if (_s == null)
            _s = this;
    }

    public HashSet<DistenceCalculate> distencePairs = new HashSet<DistenceCalculate>();
    public Dictionary<WDEntity, HashSet<DistenceCalculate>> data = new Dictionary<WDEntity, HashSet<DistenceCalculate>>();

    public void Add(WDEntity entity)
    {

        if (!data.ContainsKey(entity))
        {
            HashSet<DistenceCalculate> list = new HashSet<DistenceCalculate>();
            foreach(var item in data)
            {
                var distencePair = new DistenceCalculate(entity, item.Key);
                list.Add(distencePair);
                distencePairs.Add(distencePair);
            }
            data.Add(entity, list);
        }
    }

    public HashSet<WDEntity> Calculate(WDEntity entity,float targetDistence)
    {
        HashSet<WDEntity> result = new HashSet<WDEntity>();
        foreach(var item in data[entity])
        {
            if (item.distence < targetDistence)
            {
                result.Add(item.entity2);
            }
        }
        Debug.Log(result.Count+" "+data.Count);
        return result;
    }

    private void Update()
    {
        foreach(var item in distencePairs)
        {
            item.Calculate();
        }
    }
}

public class DistenceCalculate
{
    public WDEntity entity1;
    public WDEntity entity2;
    public float distence;

    public DistenceCalculate(WDEntity entity1,WDEntity entity2)
    {
        this.entity1 = entity1;
        this.entity2 = entity2;
    }

    public void Calculate()
    {
        distence = Vector3.Distance(entity1.transform.position, entity2.transform.position);
    }
}
