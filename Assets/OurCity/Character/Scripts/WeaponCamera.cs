using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponCamera : MonoBehaviour
{
    public Transform target;
    public RectTransform bead;
    public float speed = 1;

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
            SetBead(Vector3.Distance(transform.position, target.position)*5+1);
        }
    }

    public void SetBead(float size)
    {
        bead.sizeDelta = Vector2.one * size * 100;
    }
}
