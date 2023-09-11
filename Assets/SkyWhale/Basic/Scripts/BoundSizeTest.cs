using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundSizeTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Bounds bounds = this.GetComponent<Collider>().bounds;
        Debug.DrawLine(bounds.center, bounds.center + bounds.extents, Color.red,1000);
    }

}
