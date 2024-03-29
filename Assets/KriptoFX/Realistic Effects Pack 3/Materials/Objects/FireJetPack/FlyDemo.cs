using UnityEngine;
using System.Collections;

public class FlyDemo : MonoBehaviour
{

  public float Speed = 1;
  public float Height = 1;

  private Transform t;
  private float time;
	// Use this for initialization
	void Start ()
	{
	  t = transform;
	}
	
	// Update is called once per frame
	void Update ()
	{
	  time += Time.deltaTime;
    var sin = Mathf.Cos(time / Speed);
    t.localPosition = new Vector3(0, 0, sin*Height);
	}
}
