using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider))]
public class CHitbox : MonoBehaviour
{
    public BoxCollider boxCollider;
    public MeshRenderer meshRenderer;

    public HashSet<string> tags = new HashSet<string>();

    public UnityEvent<Collider> onTriggerEnter=new UnityEvent<Collider>();
    public UnityEvent<Collider> onTriggerExit = new UnityEvent<Collider>();
    public UnityEvent<Collider> onTriggerStay = new UnityEvent<Collider>();

    private void OnTriggerEnter(Collider other)
    {
        if (tags.Contains(other.tag))
            onTriggerEnter.Invoke(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if (tags.Contains(other.tag))
            onTriggerExit.Invoke(other);
    }

    private void OnTriggerStay(Collider other)
    {
        if (tags.Contains(other.tag))
            onTriggerStay.Invoke(other);
    }
}
