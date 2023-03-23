using UnityEngine;
using System.Collections;

public class HashIDs : MonoBehaviour
{
	// Here we store the hash tags for various strings used in our animators.
	public int locomotionState;
	public int speedFloat;
	public int angularSpeedFloat;

	void Awake ()
	{
		locomotionState = Animator.StringToHash("Base Layer.Locomotion");
		speedFloat = Animator.StringToHash("Speed");
        angularSpeedFloat = Animator.StringToHash("AngularSpeed");
	}
}
