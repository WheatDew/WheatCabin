using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Global : MonoBehaviour
{
    public static Action circle;

    public void Start()
    {
        StartCoroutine(Circle());
    }

    IEnumerator Circle()
    {
        while (true)
        {
            yield return new WaitForSeconds(5);
            circle?.Invoke();
        }
    }

    public static void SetCircle(NyaFloat nyaFloat,float value)
    {
        circle += delegate { nyaFloat.Float += value; };
    }
}
