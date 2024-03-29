using UnityEngine;
using System.Collections;

public enum Position{
	Middle,
	Bottom,
	Bottom02
}

public class DemoGUI : MonoBehaviour {

	public Texture HUETexture;
	public Material mat;
	public Position[] Positions;
	public GameObject[] Prefabs;

	private int currentNomber;
	private GameObject currentInstance;
	private GUIStyle guiStyleHeader = new GUIStyle();
	private float colorHUE;
	float dpiScale;

	// Use this for initialization
	void Start () {

		if(Screen.dpi < 1) dpiScale  = 1;
		if(Screen.dpi < 200) dpiScale = 1;
		else dpiScale = Screen.dpi / 200f;
		guiStyleHeader.fontSize = (int)(15f*dpiScale);
		guiStyleHeader.normal.textColor = new Color(1,1,1);
		currentInstance = Instantiate(Prefabs[currentNomber], transform.position, new Quaternion()) as GameObject;

		//var reactivator = currentInstance.AddComponent<DemoReactivator>();
		//reactivator.TimeDelayToReactivate = 3f;
		//int n = 0;
		//for (int i=0; i<7; i++)
		//	for (int j = 0; j<7; j++)
	//	{
			//var pos = transform.position;
			//var offset = new Vector3(i*2, 0, j*2);
			//pos = pos - new Vector3(7, 0, 7) + offset;

			///if(n < Prefabs.Length) Instantiate(Prefabs[n], pos, new Quaternion());
			//n++;
		//}
	}

	private void OnGUI()
	{
		if (GUI.Button(new Rect(10*dpiScale, 15*dpiScale, 105*dpiScale, 30*dpiScale), "Previous Effect")) {
			ChangeCurrent(-1);
		}
		if (GUI.Button(new Rect(130*dpiScale, 15*dpiScale, 105*dpiScale, 30*dpiScale), "Next Effect"))
		{
			ChangeCurrent(+1);
		}
		GUI.Label(new Rect(300*dpiScale, 15*dpiScale, 100*dpiScale, 20*dpiScale), "Prefab name is \"" + Prefabs[currentNomber].name + "\"  \r\nHold any mouse button that would move the camera", guiStyleHeader);
		
		GUI.DrawTexture(new Rect(12*dpiScale, 80*dpiScale, 220*dpiScale, 15*dpiScale), HUETexture, ScaleMode.StretchToFill, false, 0);
		float oldColorHUE = colorHUE;
		colorHUE = GUI.HorizontalSlider(new Rect(12*dpiScale, 105*dpiScale, 220*dpiScale, 15*dpiScale), colorHUE, 0, 255*6);
		if(Mathf.Abs(oldColorHUE - colorHUE) > 0.001) ChangeColor();
		GUI.Label(new Rect(240*dpiScale, 105*dpiScale, 30*dpiScale, 30*dpiScale), "Effect color", guiStyleHeader);	
	}

	void ChangeColor()
	{	var color = Hue (colorHUE/255f);
		var rend = currentInstance.GetComponentsInChildren<Renderer>();
		foreach (var r in rend) {
			var mat = r.material;
			if(mat==null || !mat.HasProperty("_TintColor")) continue;
			var oldColor = mat.GetColor("_TintColor");
			color.a = oldColor.a;
			mat.SetColor("_TintColor", color);
		}
		var light = currentInstance.GetComponentInChildren<Light>();
		if(light!=null) light.color = color;
	}

	Color Hue(float H)
	{
		Color col = new Color(1, 0, 0);
		if(H>=0 && H<1) col = new Color(1, 0, H);
		if(H>=1 && H<2) col = new Color(2-H, 0, 1);
		if(H>=2 && H<3) col = new Color(0, H-2, 1);
		if(H>=3 && H<4) col = new Color(0, 1, 4-H);
		if(H>=4 && H<5) col = new Color(H-4, 1, 0);
		if(H>=5 && H<6) col = new Color(1, 6-H, 0);
		return col;
	}
	// Update is called once per frame
	void ChangeCurrent(int delta) {
		currentNomber+=delta;
		if (currentNomber> Prefabs.Length - 1)
			currentNomber = 0;
		else if (currentNomber < 0)
			currentNomber = Prefabs.Length - 1;
		if(currentInstance!=null) Destroy(currentInstance);
		var pos = transform.position;
		if(Positions[currentNomber]==Position.Bottom) pos.y-=1;
		if(Positions[currentNomber]==Position.Bottom02) pos.y-=0.8f;
		currentInstance = Instantiate(Prefabs[currentNomber], pos, new Quaternion()) as GameObject;
		//var reactivator = currentInstance.AddComponent<DemoReactivator>();
		//reactivator.TimeDelayToReactivate = 1.5f;
	}
}
