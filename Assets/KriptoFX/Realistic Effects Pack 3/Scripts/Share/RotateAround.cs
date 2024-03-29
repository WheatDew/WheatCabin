using UnityEngine;
using System.Collections;

public class RotateAround : MonoBehaviour
{

  public float Speed = 1;
  public float LifeTime = 1;
  public float TimeDelay = 0;
  public float SpeedFadeInTime = 0;
  public bool UseCollision;
  public EffectSettings EffectSettings;

  private bool canUpdate;
  private float currentSpeedFadeIn;
  private float allTime;
	// Use this for initialization
	void Start ()
	{
    if(UseCollision) EffectSettings.CollisionEnter += EffectSettings_CollisionEnter;
	  if (TimeDelay > 0)
	    Invoke("ChangeUpdate", TimeDelay);
	  else
	    canUpdate = true;
	}

  void OnEnable()
  {
    canUpdate = true;
    allTime = 0;
  }

  void EffectSettings_CollisionEnter(object sender, CollisionInfo e)
  {
    canUpdate = false;
  }

  void ChangeUpdate()
  {
    canUpdate = true;
  }
	
	// Update is called once per frame
  private void Update()
  {
    if (!canUpdate)
      return;

    allTime += Time.deltaTime;
    if (allTime >= LifeTime && LifeTime > 0.0001f)
      return;
    if (SpeedFadeInTime > 0.001f) {
      if (currentSpeedFadeIn < Speed)
        currentSpeedFadeIn += (Time.deltaTime / SpeedFadeInTime) * Speed;
      else
        currentSpeedFadeIn = Speed;
    }
    else
      currentSpeedFadeIn = Speed;

    transform.Rotate(Vector3.forward * Time.deltaTime * currentSpeedFadeIn);
  }
}
