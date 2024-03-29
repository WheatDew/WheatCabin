using UnityEngine;
using System.Collections;

public class ScaleParticlesFromBound : MonoBehaviour
{

  private Collider targetCollider;

  void GetMeshFilterParent(Transform t)
  {
    var coll = t.parent.GetComponent<Collider>();
    if (coll == null)
      GetMeshFilterParent(t.parent);
    else
      targetCollider = coll;
  }

	// Use this for initialization
	void Start ()
	{
	  GetMeshFilterParent(transform);
    if (targetCollider == null) return;
	  var boundSize = targetCollider.bounds.size;
	  transform.localScale = boundSize;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
