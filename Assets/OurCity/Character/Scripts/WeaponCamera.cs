using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponCamera : MonoBehaviour
{
    public Transform target;
    public RectTransform bead;
    public float speed = 1;

    [HideInInspector] public float beadFixed = 0;
    [HideInInspector] public float beadFixedDrag = 4;

    private void OnEnable()
    {
        transform.parent = null;
        transform.position = target.position;
        transform.rotation = target.rotation;
    }

    private void Update()
    {
        if (target != null)
        {
            float step = speed * Time.deltaTime; // 计算这一帧要移动的步长
            transform.position = Vector3.Lerp(transform.position, target.position, step);
            transform.rotation = Quaternion.Lerp(transform.rotation, target.rotation, step);

            float moveDisperse = Vector3.Distance(transform.position, target.position);
            float fireDisperse = beadFixed;

            moveDisperse *= 2;
            if (moveDisperse > 2)
                moveDisperse = 2;
            if (fireDisperse > 2)
                fireDisperse = 2;

            SetBead(moveDisperse+fireDisperse+1);
            beadFixed -= Time.deltaTime * beadFixedDrag;
            if (beadFixed < 0)
                beadFixed = 0;
        }
    }

    public void SetBead(float size)
    {
        bead.sizeDelta = Vector2.one * size * 100;
    }
}
