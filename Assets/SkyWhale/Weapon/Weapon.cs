using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : Entity
{
    public GameObject start,end;

    public void Damage()
    {
        RaycastHit result;
        if(Physics.Raycast(start.transform.position,end.transform.position-start.transform.position,out result, Vector3.Distance(start.transform.position, end.transform.position)))
        {
            Debug.Log(result.collider.gameObject.name);
        }
    }
}
