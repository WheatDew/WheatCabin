using UnityEngine;
using System.Collections;

public class OffsetOnNormal : MonoBehaviour
{
  public float offset = 1;
  public GameObject offsetGameObject;

  private Vector3 startPosition;

  void Awake()
  {
    startPosition = transform.position;
  }

	// Use this for initialization
	void OnEnable () {
    RaycastHit verticalHit;
    Physics.Raycast(startPosition, Vector3.down, out verticalHit);
    if(offsetGameObject!=null) transform.position = offsetGameObject.transform.position + verticalHit.normal * offset;
    else {
      transform.position = verticalHit.point + verticalHit.normal * offset;
    }
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
