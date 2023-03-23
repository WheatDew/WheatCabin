using UnityEngine;
using System.Collections;

public class MovementScript : MonoBehaviour
{
	public float speed = 10.0f;
	CharacterController cc;
	
	void Awake()
	{
		cc = GetComponent<CharacterController>();
	}
	
	void FixedUpdate()
	{
		Vector3 move = Vector3.zero;
		move.x = Input.GetAxis("Horizontal") * speed;
		move.z = Input.GetAxis("Vertical") * speed;
		cc.SimpleMove(move);
	}	
}
