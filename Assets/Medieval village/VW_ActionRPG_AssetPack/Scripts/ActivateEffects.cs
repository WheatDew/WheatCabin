using UnityEngine;
using System.Collections;

public class ActivateEffects : MonoBehaviour {

	public ParticleSystem particleEffect;
	
	void OnTriggerEnter (Collider other) 
	{
		if(other.gameObject.tag == "Player")
		{
			particleEffect.Play ();
			GetComponent<AudioSource>().Play ();
		}
	}

	void OnTriggerExit (Collider other)
	{
			particleEffect.Stop();
			GetComponent<AudioSource>().Stop();	
	}
}
