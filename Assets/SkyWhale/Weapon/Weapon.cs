using Battlehub.UIControls;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : Entity
{
    public GameObject start,end;

    private HashSet<GameObject> triggerList = new HashSet<GameObject>();

    private Vector3[] _determinData=new Vector3[4];

    public void DamageStart()
    {
        _determinData[0] = start.transform.position;
        _determinData[1] = end.transform.position;

    }

    public void DamageEnd()
    {
        _determinData[2] = start.transform.position;
        _determinData[3] = end.transform.position;
        Debug.DrawLine(_determinData[0], _determinData[2], Color.green, 1000);
        Debug.DrawLine(_determinData[1], _determinData[3], Color.green, 1000);

        if (_determinData[0] != Vector3.zero && _determinData[1] != Vector3.zero)
        {
            for(int i = 0; i < 10; i++)
            {
                Vector3 _startPoint = Vector3.Lerp(_determinData[0], _determinData[2], i / 10f);
                Vector3 _endPoint = Vector3.Lerp(_determinData[1], _determinData[3], i / 10f);
                Damage(_startPoint, _endPoint);
            }
        }
        _determinData[0] = Vector3.zero;
        _determinData[1] = Vector3.zero;
        _determinData[2] = Vector3.zero;
        _determinData[3] = Vector3.zero;
        Debug.Log("endEvent");
    }

    private void Damage(Vector3 start,Vector3 end)
    {
        Debug.DrawLine(start, end, Color.red, 1000);
        RaycastHit result;
        if (Physics.Raycast(start, end - start, out result, Vector3.Distance(start, end),1<<7))
        {
            Debug.Log(result.collider.gameObject.name);
            
        }
    }

    public void DisplayWeaponRange()
    {
        
    }

}
