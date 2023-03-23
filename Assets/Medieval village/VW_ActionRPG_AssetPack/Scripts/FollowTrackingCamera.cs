using UnityEngine;

public class FollowTrackingCamera : MonoBehaviour
{
	// Camera target to look at.
	public Transform target;


	public Vector3 CameraRotation; 

	// Exposed vars for the camera position from the target.
	public float height = 20f;
	public float distance = 20f;
	
	// Camera limits.
	public float min = 10f;
	public float max = 60;
	
	// Options.
	//public bool doRotate;
	public bool doZoom;
	
	// The movement amount when zooming.
	public float zoomStep = 30f;
	public float zoomSpeed = 5f;
	private float heightWanted;
	private float distanceWanted;
	
	// Result vectors.
	private Vector3 zoomResult;
	private Quaternion rotationResult;
	private Vector3 targetAdjustedPosition;
	
	void Start(){
		// Initialise default zoom vals.
		heightWanted = height;
		distanceWanted = distance;
		
		// Setup our default camera.  We set the zoom result to be our default position.
		zoomResult = new Vector3(0f, height, -distance);
	}
	
	void LateUpdate(){
		// Check target.
		if( !target ){
			Debug.LogError("This camera has no target, you need to assign a target in the inspector.");
			return;
		}
		
		if( doZoom ){
			// Record our mouse input.  If we zoom add this to our height and distance.
			float mouseInput = Input.GetAxis("Mouse ScrollWheel");
			heightWanted -= zoomStep * mouseInput;
			distanceWanted -= zoomStep * mouseInput;
			
			// Make sure they meet our min/max values.
			heightWanted = Mathf.Clamp(heightWanted, min, max);
			distanceWanted = Mathf.Clamp(distanceWanted, min, max);
			
			height = Mathf.Lerp(height, heightWanted, Time.deltaTime * zoomSpeed);
			distance = Mathf.Lerp(distance, distanceWanted, Time.deltaTime * zoomSpeed);
			
			// Post our result.
			zoomResult = new Vector3(0f, height, -distance);
		}
		

		// Convert the angle into a rotation.
		rotationResult = Quaternion.Euler(CameraRotation);

		// Set the camera position reference.
		targetAdjustedPosition = rotationResult * zoomResult;
		//targetAdjustedPosition = AnguloInicial * zoomResult;
		transform.position = target.position + targetAdjustedPosition;

		// Face the desired position.
		transform.LookAt(target);

	}
}