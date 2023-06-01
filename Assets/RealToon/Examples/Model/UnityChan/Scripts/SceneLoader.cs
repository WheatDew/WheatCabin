using UnityEngine;
using System.Collections;

public class SceneLoader : MonoBehaviour {
    [System.Obsolete]
    void OnGUI()
	{
		GUI.Box(new Rect(10 , Screen.height - 100 ,100 ,90), "Change Scene");
		if(GUI.Button( new Rect(20 , Screen.height - 70 ,80, 20), "Next"))
			LoadNextScene();
		if(GUI.Button(new Rect(20 ,  Screen.height - 40 ,80, 20), "Back"))
			LoadPreScene();
	}

    [System.Obsolete]
    void LoadPreScene()
	{
		int nextLevel = Application.loadedLevel + 1;
		if( nextLevel <= 1)
			nextLevel = Application.levelCount;

		Application.LoadLevel(nextLevel);
	}

    [System.Obsolete]
    void LoadNextScene()
	{
		int nextLevel = Application.loadedLevel + 1;
		if( nextLevel >= Application.levelCount)
			nextLevel = 1;

		Application.LoadLevel(nextLevel);

	}
}
