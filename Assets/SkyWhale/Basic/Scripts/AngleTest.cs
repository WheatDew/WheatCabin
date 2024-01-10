using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AngleTest : MonoBehaviour
{
    Rigidbody _rigidbody;
    // Start is called before the first frame update
    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 velocity = Vector3.right * Input.GetAxisRaw("Horizontal") + Vector3.forward * Input.GetAxisRaw("Vertical");

        transform.forward = Vector3.Lerp(transform.forward,velocity.normalized,0.1f);
        var angle = Vector3.SignedAngle(transform.forward.normalized, velocity.normalized, Vector3.up);

        //Debug.Log()

        //var angle = Vector3.SignedAngle(transform.forward, velocity, Vector3.up);
        //Debug.Log(angle);
        //transform.rotation = Quaternion.AngleAxis(30, Vector3.up);

        velocity.y = _rigidbody.velocity.y;
        _rigidbody.velocity = velocity;
    }
}
