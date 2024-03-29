using UnityEngine;
using System.Collections;

public class AnimationSpeedDebuff : MonoBehaviour
{
  public AnimationCurve AnimationSpeenOnTime;
  public float MaxTime = 1;

  private Animator myAnimator;
  private Transform root;
  private float oldSpeed;
  private float time;
  void GetAnimatorOnParent(Transform t)
  {
    var anim = t.parent.GetComponent<Animator>();
    if (anim==null) {
      if (root == t.parent) return;
      GetAnimatorOnParent(t.parent);
    }
    else
      myAnimator = anim;
  }

	// Use this for initialization
	void Start ()
	{
	  root = transform.root;
    GetAnimatorOnParent(transform);
    if(myAnimator==null) return;
    oldSpeed= myAnimator.speed;
	}
	
	// Update is called once per frame
	void Update () {
	  if(myAnimator==null || AnimationSpeenOnTime.length == 0) return;
	  time += Time.deltaTime;
      myAnimator.speed = Mathf.Clamp01(AnimationSpeenOnTime.Evaluate(time / MaxTime) * oldSpeed);
	}
}
