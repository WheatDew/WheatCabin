using UnityEngine;
using System.Collections;

public class SwichCameras : MonoBehaviour {


	public GameObject CameraHigh;
	public GameObject CameraMed;
	public GameObject CameraLow;


	void Start()
	{
		CameraHigh.SetActive(false);
		CameraMed.SetActive(true);
		CameraLow.SetActive(false);
	}

	void OnGUI()
	{
		//GUI.Label(new Rect(Screen.width / 2 - 50, Screen.height - 80,100,30), "Quality")



		if(GUI.Button (new Rect(240, Screen.height - 40, 100, 25), "PC High"))
		{
			CameraHigh.SetActive(true);
			CameraMed.SetActive(false);
			CameraLow.SetActive(false);
		}
		if(GUI.Button (new Rect(125, Screen.height - 40, 100, 25), "PC Low"))
		{
			CameraHigh.SetActive(false);
			CameraMed.SetActive(true);
			CameraLow.SetActive(false);
		}
		if(GUI.Button (new Rect(10, Screen.height - 40, 100, 25), "Unity Free"))
		{
			CameraHigh.SetActive(false);
			CameraMed.SetActive(false);
			CameraLow.SetActive(true);
		}


	}
}
