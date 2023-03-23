using UnityEngine;
using System.Collections;

public class LoadScene : MonoBehaviour {

	public int level;

	void OnTriggerEnter (Collider other)
	{
		Debug.Log ("Loading Level");
		Application.LoadLevel(level);
	}
}
