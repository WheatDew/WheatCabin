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

    public HashSet<Entity> pool = new HashSet<Entity>();

    public void Add(Entity entity)
    {
        if (!pool.Contains(entity))
            pool.Add(entity);
    }

    public HashSet<Entity> Calculate(Vector3 position,float distence)
    {
        HashSet<Entity> result = new HashSet<Entity>();
        foreach(var item in pool)
        {
            if (Vector3.Distance(position, item.transform.position) < distence)
            {
                result.Add(item);
            }
        }
        return result;
    }
}
