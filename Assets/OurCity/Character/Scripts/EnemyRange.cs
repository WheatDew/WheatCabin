using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemyRange : MonoBehaviour
{
    public HashSet<GameObject> entities = new HashSet<GameObject>();
    public bool isSave = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag=="Player"&&other.gameObject != gameObject)
        {
            entities.Add(other.gameObject);
            
        }
    }

    private void OnTriggerExit(Collider other)
    {

        if(!isSave&& other.tag == "Player" && other.gameObject!=gameObject)
        {
            if(entities.Contains(other.gameObject))
            entities.Remove(other.gameObject);
        }
    }
}
